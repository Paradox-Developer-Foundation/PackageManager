using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using ParadoxPM.Server.Models;
using ParadoxPM.Server.Repositories;
using ParadoxPM.Server.ViewModels;
using ZLogger;

namespace ParadoxPM.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PackagesController : ControllerBase
{
    private readonly IPackageRepository _packageRepository;
    private readonly IFileRepository _fileRepository;
    private readonly ILogger<PackagesController> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public PackagesController(
        IPackageRepository packageRepository,
        IFileRepository fileRepository,
        ILogger<PackagesController> logger
    )
    {
        _packageRepository = packageRepository;
        _fileRepository = fileRepository;
        _logger = logger;
    }

    // 查询指定的包
    // GET: api/packages/query/meta/{id}
    [HttpGet("query/{packageId:int}/meta")]
    public async Task<ActionResult<ApiResponse<Package>>> GetPackage(int packageId)
    {
        try
        {
            var package = await _packageRepository.GetPackageAsync(packageId, HttpContext.RequestAborted);
            return Ok(new ApiResponse<Package>(StatusCodes.Status200OK, "请求成功", package));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.ZLogWarning(ex, $"包不存在, Id: {packageId}");
            return NotFound(new ApiResponse<object?>(StatusCodes.Status404NotFound, ex.Message, null));
        }
        catch (DbUpdateException ex)
        {
            _logger.ZLogError(ex, $"获取包时发生数据库错误, Id: {packageId}");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<object?>(
                    StatusCodes.Status500InternalServerError,
                    $"数据库错误: {ex.Message}",
                    null
                )
            );
        }
    }

    // 搜索合适的包
    // GET: api/packages/query/search
    [HttpGet("query/search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Package>>>> SearchPackage(
        [FromQuery] string keyword,
        [FromQuery] string? arch
    )
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
            {
                return BadRequest(
                    new ApiResponse<object?>(StatusCodes.Status400BadRequest, "关键字至少为两个字符", null)
                );
            }

            var packages = await _packageRepository.SearchPackageAsync(
                keyword,
                arch,
                HttpContext.RequestAborted
            );

            return Ok(new ApiResponse<IEnumerable<Package>>(StatusCodes.Status200OK, "请求成功", packages));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse<object?>(StatusCodes.Status404NotFound, ex.Message, null));
        }
        catch (DbUpdateException ex)
        {
            _logger.ZLogError(ex, $"搜索包时发生数据库错误, Keyword: {keyword}, Arch: {arch}");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<object?>(
                    StatusCodes.Status500InternalServerError,
                    $"数据库错误: {ex.Message}",
                    null
                )
            );
        }
    }

    // 创建并上传包
    // POST: api/packages/upload
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<Package>>> UploadPackage(
        [FromForm] PackageUploadViewModel model
    )
    {
        try
        {
            string? authToken = Request.Cookies["Token"];
            var packageInfo = JsonSerializer.Deserialize<PackageUploadInfo>(
                model.PackageInfoJson,
                JsonOptions
            );

            if (packageInfo is null)
            {
                return BadRequest(
                    new ApiResponse<object?>(StatusCodes.Status400BadRequest, "无效的包 JSON 信息", null)
                );
            }
            if (!packageInfo.IsValid(out string errorMessage))
            {
                return BadRequest(
                    new ApiResponse<object?>(StatusCodes.Status400BadRequest, errorMessage, null)
                );
            }

            var dependencyList = packageInfo.Dependencies;

            if (!await _packageRepository.IsValidDependenciesAsync(dependencyList))
            {
                return BadRequest(
                    new ApiResponse<object?>(StatusCodes.Status400BadRequest, "使用不存在的依赖项", null)
                );
            }

            // 检查文件SHA256
            var fileStream = model.File.OpenReadStream();

            if (!await _fileRepository.CheckFileIntegrityAsync(fileStream, packageInfo.Integrity))
            {
                return BadRequest(
                    new ApiResponse<object?>(StatusCodes.Status400BadRequest, "文件的哈希校验失败", null)
                );
            }

            int? id = await _packageRepository.GetNextIdAsync();
            if (id is null)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "无法获取下一个包 ID");
            }

            var versions = new List<PackageVersion>();
            var version = new PackageVersion
            {
                Version = packageInfo.Version,
                Integrity = packageInfo.Integrity,
                Tarball = $"{packageInfo.NormalizedName}-{packageInfo.Version}.7z",
                UploadTime = DateTime.UtcNow,
                DownloadCount = 0,
                Dependencies = dependencyList
                    .Select(d => new Dependency
                    {
                        DependencyId = d.Id,
                        Name = d.Name,
                        NormalizedName = d.NormalizedName,
                        MinVersion = d.MinVersion,
                    })
                    .ToList(),
            };
            versions.Add(version);

            var package = new Package
            {
                Id = id.Value,
                Name = packageInfo.Name,
                NormalizedName = packageInfo.NormalizedName,
                Description = packageInfo.Description,
                Arch = packageInfo.Arch,
                IsActive = true,
                Author = packageInfo.Author,
                License = packageInfo.License,
                Repository = packageInfo.Repository,
                Homepage = packageInfo.Homepage,
                Versions = versions,
            };

            await _packageRepository.AddPackageAsync(package);
            await _fileRepository.SaveFileAsync(
                $"{package.NormalizedName}/{version.Version}/{version.Tarball}",
                fileStream
            );

            package = await _packageRepository.GetPackageAsync(package.Id, HttpContext.RequestAborted);

            return StatusCode(
                StatusCodes.Status201Created,
                new ApiResponse<Package>(StatusCodes.Status201Created, "创建成功", package)
            );
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<object?>(StatusCodes.Status400BadRequest, ex.Message, null));
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException { SqlState: "23505" })
            {
                return BadRequest(
                    new ApiResponse<object?>(
                        StatusCodes.Status400BadRequest,
                        "创建失败: 相同规范化名称的包已存在",
                        null
                    )
                );
            }

            _logger.ZLogError(ex, $"创建包时发生数据库错误");
            string entryDetails = JsonSerializer.Serialize(ex.Entries);
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                $"数据库错误: {ex.Message}. 详细信息: {entryDetails}"
            );
        }
        catch (IOException ex)
        {
            _logger.ZLogError(ex, $"创建包时发生文件存储错误");
            return StatusCode(StatusCodes.Status500InternalServerError, $"文件存储错误: {ex.Message}");
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (JsonException ex)
        {
            return BadRequest(new ApiResponse<object?>(StatusCodes.Status400BadRequest, ex.Message, null));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传文件时发生错误");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<object?>(StatusCodes.Status500InternalServerError, "内部错误", null)
            );
        }
    }

    // 修改包的启用状态
    // PATCH: api/packages/update/{packageId:int}/status
    [HttpPatch("update/{packageId:int}/status")]
    public async Task<ActionResult<ApiResponse<Package>>> UpdatePackageStatus(
        int packageId,
        [FromQuery] bool isActive
    )
    {
        try
        {
            var package = await _packageRepository.GetPackageAsync(packageId, HttpContext.RequestAborted);
            package.IsActive = isActive;
            await _packageRepository.UpdatePackageAsync(package);
            return Ok(new ApiResponse<Package>(StatusCodes.Status200OK, "状态更新成功", package));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.ZLogWarning(ex, $"包不存在, Id: {packageId}");
            return NotFound(new ApiResponse<object?>(StatusCodes.Status404NotFound, ex.Message, null));
        }
        catch (DbUpdateException ex)
        {
            _logger.ZLogError(ex, $"更新包状态时发生数据库错误, Id: {packageId}");
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<object?>(
                    StatusCodes.Status500InternalServerError,
                    $"数据库错误: {ex.Message}",
                    null
                )
            );
        }
    }

    // 下载指定的包
    // GET: api/packages/download/{normalizedName}/{version}
    [HttpGet("download/{normalizedName}/{version}")]
    public async Task<IActionResult> DownloadPackage(string normalizedName, string version)
    {
        try
        {
            string tarball = $"{normalizedName}/{version}/{normalizedName}-{version}.7z";
            var fileStream = await _fileRepository.GetFileAsync(tarball);
            return File(fileStream, "application/x-7z-compressed", tarball);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new ApiResponse<object?>(StatusCodes.Status404NotFound, ex.Message, null));
        }
        catch (IOException ex)
        {
            _logger.ZLogError(
                ex,
                $"下载包时发生文件错误, normalizedName: {normalizedName}, version: {version}"
            );
            return StatusCode(StatusCodes.Status500InternalServerError, "文件读取错误");
        }
        catch (Exception ex)
        {
            _logger.ZLogError(
                ex,
                $"下载包时发生未知错误, normalizedName: {normalizedName}, version: {version}"
            );
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new ApiResponse<object?>(StatusCodes.Status500InternalServerError, "内部错误", null)
            );
        }
    }
}
