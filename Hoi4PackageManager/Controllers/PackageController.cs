using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Hoi4PackageManager.Models;
using Hoi4PackageManager.Repositories;
using Hoi4PackageManager.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;

namespace Hoi4PackageManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PackagesController : ControllerBase
    {
        private readonly IPackageRepository _packageRepository;
        private readonly IFileRepository _fileRepository;

        public PackagesController(
            IPackageRepository packageRepository,
            IFileRepository fileRepository
        )
        {
            _packageRepository = packageRepository;
            _fileRepository = fileRepository;
        }

        // GET: api/packages
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Package>>> GetAllPackages()
        {
            try
            {
                var packages = await _packageRepository.GetPackagesAsync(isActiveOnly: false);
                return Ok(packages);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"数据库错误: {ex.Message}");
            }
        }

        // GET: api/packages/active
        [HttpGet("active")]
        public async Task<ActionResult<IEnumerable<Package>>> GetActivePackages()
        {
            try
            {
                var packages = await _packageRepository.GetPackagesAsync(isActiveOnly: true);
                return Ok(packages);
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"数据库错误: {ex.Message}");
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
                var package = await _packageRepository.GetPackageAsync(
                    packageId,
                    packageNormalizedName
                );
                var filePath = package.FilePath;
                var fileStream = await _fileRepository.GetFileAsync(filePath);
                await _packageRepository.AddPackageDownloadCountAsync(
                    packageId,
                    packageNormalizedName
                );
                return File(fileStream, "application/zip", Path.GetFileName(filePath));
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(500, $"数据库错误: {ex.Message}");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (FileNotFoundException)
            {
                return StatusCode(500, "文件存储错误: 文件未找到");
            }
        }

        // Post: api/packages/dev
        [HttpPost("dev")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFileWithData([FromForm] FileUploadViewModel model)
        {
            try
            {
                model.ValidCheck();
                var dependencyList =
                    model.Dependencies?.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList()
                    ?? [];

                await _packageRepository.CheckDependenciesAsync(dependencyList);
                // 检查文件SHA256
                var fileStream = model.File.OpenReadStream();
                // ReSharper disable once InconsistentNaming
                var fileSHA256 = await _fileRepository.GetFileSHA256Async(fileStream);
                if (!fileSHA256.Equals(model.SHA256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new ValidationException("文件的 SHA256 校验失败");
                }
                var package = new Package
                {
                    Name = model.Name,
                    NormalizedName = model.NormalizedName.ToLowerInvariant(),
                    Version = model.Version,
                    Description = model.Description,
                    License = model.License,
                    SHA256 = model.SHA256,
                    IsActive = model.IsActive,
                    FilePath = "",
                    Dependencies = dependencyList,
                };
                await _packageRepository.AddPackageAsync(package);
                package = await _packageRepository.GetPackageAsync(
                    package.Id,
                    package.NormalizedName
                );
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
                return StatusCode(500, $"数据库错误: {ex.Message}");
            }
            catch (IOException ex)
            {
                return StatusCode(500, $"文件存储错误: {ex.Message}");
            }
            catch (KeyNotFoundException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
