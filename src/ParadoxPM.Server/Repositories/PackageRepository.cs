using Microsoft.EntityFrameworkCore;
using ParadoxPM.Server.Models;
using ParadoxPM.Server.ViewModels;
using AppContext = ParadoxPM.Server.Models.AppContext;

namespace ParadoxPM.Server.Repositories;

public sealed class PackageRepository : IPackageRepository
{
    private readonly AppContext _context;

    public PackageRepository(AppContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Package>> GetPackagesAsync(bool isActiveOnly = true)
    {
        return await _context
            .Packages.AsNoTracking()
            .Include(p => p.Versions)
            .ThenInclude(v => v.Dependencies)
            .Where(p => !isActiveOnly || p.IsActive)
            .ToListAsync();
    }

    public async Task<Package> GetPackageAsync(int packageId)
    {
        var package = await _context
            .Packages.AsNoTracking()
            .Include(p => p.Versions)
            .ThenInclude(v => v.Dependencies)
            .FirstOrDefaultAsync(p => p.Id == packageId);

        if (package is null)
        {
            throw new KeyNotFoundException(packageId.ToString());
        }
        return package;
    }

    public async Task<bool> IsValidDependenciesAsync(IEnumerable<PackageUploadDependencyInfo> dependencies)
    {
        foreach (var dependency in dependencies)
        {
            if (
                !await _context
                    .Packages.AsNoTracking()
                    .Select(p => new { p.Id, p.NormalizedName })
                    .AnyAsync(x => x.Id == dependency.Id && x.NormalizedName == dependency.NormalizedName)
            )
            {
                return false;
            }
        }

        return true;
    }

    public async Task AddPackageAsync(Package package)
    {
        ArgumentNullException.ThrowIfNull(package);
        _context.Packages.Add(package);

        // 更新包的最后修改时间
        var updateTime = await _context.UpdateTimes.AsNoTracking().FirstOrDefaultAsync(u => u.Id == 1);
        if (updateTime == null)
        {
            updateTime = new UpdateTime { Id = 1, PackageLastModified = DateTime.UtcNow };
            _context.UpdateTimes.Add(updateTime);
        }
        else
        {
            updateTime.PackageLastModified = DateTime.UtcNow;
            _context.UpdateTimes.Update(updateTime);
        }
        await _context.SaveChangesAsync();
    }

    public async Task<int?> GetNextIdAsync()
    {
        int[] array = await _context
            .Database.SqlQueryRaw<int>("SELECT nextval('public.\"Packages_Id_seq\"')")
            .ToArrayAsync();

        return array.Length == 0 ? null : array[0];
    }

    public async Task<DateTime> GetPackageLastModifiedAsync()
    {
        var updateTime = await _context.UpdateTimes.AsNoTracking().FirstOrDefaultAsync(u => u.Id == 1);
        var lastModified = DateTime.UtcNow;
        if (updateTime != null)
        {
            lastModified = updateTime.PackageLastModified;
        }
        return lastModified;
    }
}
