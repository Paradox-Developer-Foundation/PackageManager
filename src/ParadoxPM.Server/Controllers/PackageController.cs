using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    // GET: api/packages
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Package>>>> GetAllPackages()
    {
        try
        {
            var packages = await _packageRepository.GetPackagesAsync(isActiveOnly: false);
            return Ok(new ApiResponse<IEnumerable<Package>>(StatusCodes.Status200OK, "请求成功", packages));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "获取所有包时发生错误");
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

    // GET: api/packages/active
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<Package>>> GetActivePackages()
    {
        try
        {
            var packages = await _packageRepository.GetPackagesAsync(isActiveOnly: true);
            return Ok(new ApiResponse<IEnumerable<Package>>(StatusCodes.Status200OK, "请求成功", packages));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "获取活动包时发生错误");
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

    // GET: api/packages/{packageId}
    [HttpGet("{packageId:int}")]
    public async Task<ActionResult<ApiResponse<Package>>> GetPackage(int packageId)
    {
        try
        {
            var package = await _packageRepository.GetPackageAsync(packageId);
            return Ok(new ApiResponse<Package>(StatusCodes.Status200OK, "请求成功", package));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.ZLogWarning(ex, $"未找到包, Id: {packageId}");
            return NotFound(new ApiResponse<object?>(StatusCodes.Status404NotFound, ex.Message, null));
        }
        catch (DbUpdateException ex)
        {
            _logger.ZLogWarning(ex, $"包文件未找到, Id: {packageId}");
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

    // POST: api/packages/upload
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<Package>>> UploadPackage(
        [FromForm] PackageUploadViewModel model
    )
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };

            var packageInfo = JsonSerializer.Deserialize<PackageUploadInfo>(model.PackageInfoJson, options)!;

            if (!packageInfo.IsValid(out var errorMessages))
            {
                string combinedErrors = string.Join("; ", errorMessages);
                return BadRequest(
                    new ApiResponse<object?>(
                        StatusCodes.Status400BadRequest,
                        combinedErrors, // 使用合并后的错误字符串
                        null
                    )
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
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }
            string fileSha256 = "sha256-" + await _fileRepository.GetFileSha256Async(fileStream);
            if (!fileSha256.Equals(packageInfo.Integrity, StringComparison.InvariantCultureIgnoreCase))
            {
                return BadRequest(
                    new ApiResponse<object?>(StatusCodes.Status400BadRequest, "文件的 SHA256 校验失败", null)
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
                Tarball = $"{id.Value}-{packageInfo.NormalizedName}-{packageInfo.Version}.7z",
                UploadTime = DateTime.UtcNow,
                DownloadCount = 0,
                Dependencies = dependencyList
                    .Select(d => new Dependency
                    {
                        DependencyId = d.Id,
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
            await _fileRepository.SaveFileAsync(version.Tarball, fileStream);

            return CreatedAtAction(
                nameof(GetAllPackages),
                new { packageId = package.Id, packageNormalizedName = package.NormalizedName },
                new ApiResponse<Package>(StatusCodes.Status201Created, "包上传成功", package)
            );
        }
        catch (DbUpdateException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"数据库错误: {ex.Message}");
        }
        catch (IOException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, $"文件存储错误: {ex.Message}");
        }
        catch (KeyNotFoundException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (JsonException ex)
        {
            return BadRequest($"Invalid JSON format: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传文件时发生错误");
            return BadRequest("内部错误");
        }
    }
}
