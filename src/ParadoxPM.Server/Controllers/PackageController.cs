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

    // Get: api/packages/files
    [HttpGet("files")]
    public async Task<ActionResult<IEnumerable<string>>> GetPackage(
        [FromQuery] int packageId,
        [FromQuery] string packageNormalizedName
    )
    {
        try
        {
            var package = await _packageRepository.GetPackageAsync(packageId, packageNormalizedName);
            var filePath = package.FilePath;
            var fileStream = await _fileRepository.GetFileAsync(filePath);
            await _packageRepository.AddPackageDownloadCountAsync(packageId, packageNormalizedName);
            return File(fileStream, "application/zip", Path.GetFileName(filePath));
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "获取包文件时发生错误");
            return StatusCode(StatusCodes.Status500InternalServerError, $"数据库错误: {ex.Message}");
        }
        catch (KeyNotFoundException ex)
        {
            _logger.ZLogWarning(ex, $"未找到包, Id: {packageId}, Name: {packageNormalizedName}");
            return NotFound(ex.Message);
        }
        catch (FileNotFoundException ex)
        {
            _logger.ZLogWarning(ex, $"包文件未找到, Id: {packageId}, Name: {packageNormalizedName}");
            return StatusCode(StatusCodes.Status500InternalServerError, "文件存储错误: 文件未找到");
        }
    }

    // Post: api/packages/dev
    [HttpPost("dev")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadFileWithData([FromForm] FileUploadViewModel model)
    {
        try
        {
            if (!model.IsValid(out var errorMessages))
            {
                return BadRequest(errorMessages);
            }

            var dependencyList = model
                .Dependencies.Split('|', StringSplitOptions.RemoveEmptyEntries)
                .ToList();

            if (!await _packageRepository.IsValidDependenciesAsync(dependencyList))
            {
                return BadRequest("使用不存在的依赖项");
            }

            // 检查文件SHA256
            var fileStream = model.File.OpenReadStream();
            string fileSha256 = await _fileRepository.GetFileSha256Async(fileStream);
            if (!fileSha256.Equals(model.Sha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return BadRequest("文件的 SHA256 校验失败");
            }
            var package = new Package
            {
                Name = model.Name,
                NormalizedName = model.NormalizedName.ToLowerInvariant(),
                Version = model.Version,
                Description = model.Description,
                License = model.License,
                Sha256 = model.Sha256,
                IsActive = model.IsActive,
                FilePath = "",
                Arch = model.Arch,
                Dependencies = dependencyList,
            };
            await _packageRepository.AddPackageAsync(package);
            package = await _packageRepository.GetPackageAsync(package.Id, package.NormalizedName);
            package.FilePath = $"{package.Id}_{package.NormalizedName}-{package.Version}.zip";
            await _packageRepository.UpdatePackageAsync(package);
            await _fileRepository.SaveFileAsync(package.FilePath, model.File.OpenReadStream());
            return CreatedAtAction(
                nameof(GetPackage),
                new { packageId = package.Id, packageNormalizedName = package.NormalizedName },
                package
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
    }
}
