using Microsoft.EntityFrameworkCore;
using ParadoxPM.Server.Models;

namespace ParadoxPM.Server.Repositories;

public sealed class PackageRepository : IPackageRepository
{
    private readonly PackageContext _context;

    public PackageRepository(PackageContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Package>> GetPackagesAsync(bool isActiveOnly = true)
    {
        return await _context.Packages.AsNoTracking().Where(p => !isActiveOnly || p.IsActive).ToListAsync();
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
        if (package is null)
        {
            throw new KeyNotFoundException(packageId.ToString());
        }
        return package;
    }

    public async Task<bool> IsValidDependenciesAsync(IEnumerable<string> dependencies)
    {
        foreach (string dependency in dependencies)
        {
            if (
                !await _context
                    .Packages.AsNoTracking()
                    .Select(package => package.NormalizedName)
                    .AnyAsync(normalizedName => normalizedName == dependency)
            )
            {
                return false;
            }
        }

        return true;
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
        int affectedRows = await _context
            .Packages.Where(p => p.Id == updatePackage.Id)
            .ExecuteUpdateAsync(p =>
                p.SetProperty(x => x.Name, updatePackage.Name)
                    .SetProperty(x => x.NormalizedName, updatePackage.NormalizedName)
                    .SetProperty(x => x.Version, updatePackage.Version)
                    .SetProperty(x => x.Description, updatePackage.Description)
                    .SetProperty(x => x.License, updatePackage.License)
                    .SetProperty(x => x.Size, updatePackage.Size)
                    .SetProperty(x => x.Sha256, updatePackage.Sha256)
                    .SetProperty(x => x.UploadDate, updatePackage.UploadDate)
                    .SetProperty(x => x.IsActive, updatePackage.IsActive)
                    .SetProperty(x => x.FilePath, updatePackage.FilePath)
                    .SetProperty(x => x.Dependencies, updatePackage.Dependencies)
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

    public async Task<int?> GetNextIdAsync()
    {
        int[] array = await _context
            .Database.SqlQueryRaw<int>("SELECT nextval('public.\"Packages_Id_seq\"')")
            .ToArrayAsync();

        return array.Length == 0 ? null : array[0];
    }
}
