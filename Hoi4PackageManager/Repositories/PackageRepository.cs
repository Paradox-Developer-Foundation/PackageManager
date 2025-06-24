using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hoi4PackageManager.Models;
using Microsoft.EntityFrameworkCore;

namespace Hoi4PackageManager.Repositories;

public class PackageRepository : IPackageRepository
{
    private readonly PackageContext _context;

    public PackageRepository(PackageContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Package>> GetPackagesAsync(bool isActiveOnly = true)
    {
        return await _context
            .Packages.AsNoTracking()
            .Where(p => !isActiveOnly || p.IsActive)
            .OrderBy(p => p.NormalizedName)
            .ToListAsync();
    }

    public async Task<Package> GetPackageAsync(
        int packageId,
        string packageNormalizedName,
        bool isActiveOnly = true
    )
    {
        var package = await _context
            .Packages.AsNoTracking()
            .Where(p =>
                (!isActiveOnly || p.IsActive)
                && p.Id == packageId
                && p.NormalizedName == packageNormalizedName
            )
            .FirstOrDefaultAsync();
        if (package == null)
        {
            throw new KeyNotFoundException(packageId.ToString());
        }
        return package;
    }

    public async Task CheckDependenciesAsync(IEnumerable<string> dependencies)
    {
        foreach (var dependency in dependencies)
        {
            var exists = await _context
                .Packages.AsNoTracking()
                .AnyAsync(p => p.NormalizedName == dependency);
            if (!exists)
            {
                throw new KeyNotFoundException($"依赖包 '{dependency}' 未找到");
            }
        }
    }

    public async Task AddPackageDownloadCountAsync(int packageId, string packageNormalizedName)
    {
        var affectedRows = await _context
            .Packages.Where(p => p.Id == packageId && p.NormalizedName == packageNormalizedName)
            .ExecuteUpdateAsync(p => p.SetProperty(x => x.DownloadCount, x => x.DownloadCount + 1));
        if (affectedRows == 0)
        {
            throw new KeyNotFoundException();
        }
    }

    public async Task AddPackageAsync(Package package)
    {
        ArgumentNullException.ThrowIfNull(package);
        _context.Packages.Add(package);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePackageAsync(Package updatePackage)
    {
        ArgumentNullException.ThrowIfNull(updatePackage);
        var affectedRows = await _context
            .Packages.Where(p => p.Id == updatePackage.Id)
            .ExecuteUpdateAsync(p =>
                p.SetProperty(x => x.Name, x => updatePackage.Name)
                    .SetProperty(x => x.NormalizedName, x => updatePackage.NormalizedName)
                    .SetProperty(x => x.Version, x => updatePackage.Version)
                    .SetProperty(x => x.Description, x => updatePackage.Description)
                    .SetProperty(x => x.License, x => updatePackage.License)
                    .SetProperty(x => x.Size, x => updatePackage.Size)
                    .SetProperty(x => x.SHA256, x => updatePackage.SHA256)
                    .SetProperty(x => x.UploadDate, x => updatePackage.UploadDate)
                    .SetProperty(x => x.IsActive, x => updatePackage.IsActive)
                    .SetProperty(x => x.FilePath, x => updatePackage.FilePath)
                    .SetProperty(x => x.Dependencies, x => updatePackage.Dependencies)
            );
        if (affectedRows == 0)
        {
            throw new KeyNotFoundException();
        }
    }

    public async Task DeletePackageAsync(int packageId, string packageNormalizedName)
    {
        var affectedRows = await _context
            .Packages.Where(p => p.Id == packageId && p.NormalizedName == packageNormalizedName)
            .ExecuteDeleteAsync();

        if (affectedRows == 0)
        {
            throw new KeyNotFoundException();
        }
    }
}
