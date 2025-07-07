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

    public async Task<Package> GetPackageAsync(int packageId, CancellationToken token)
    {
        var package = await _context
            .Packages.AsNoTracking()
            .Include(p => p.Versions)
            .ThenInclude(v => v.Dependencies)
            .FirstOrDefaultAsync(p => p.Id == packageId, token);

        if (package is null)
        {
            throw new KeyNotFoundException($"包不存在, Id: {packageId}");
        }
        return package;
    }

    public async Task<IEnumerable<Package>> SearchPackageAsync(
        string keyword,
        string? arch,
        CancellationToken token
    )
    {
        string pattern = $"%{keyword.ToLower()}%";
        var packages = await _context
            .Packages.AsNoTracking()
            .Where(p =>
                (
                    EF.Functions.Like(p.Name, pattern)
                    || EF.Functions.Like(p.NormalizedName, pattern)
                    || EF.Functions.Like(p.Description, pattern)
                ) && (arch == null || p.Arch == arch)
            )
            .ToListAsync(token);
        if (packages.Count == 0)
        {
            throw new KeyNotFoundException($"未找到符合要求的包, 关键词: {keyword}, 游戏类型: {arch}");
        }
        return packages;
    }

    public async Task<bool> IsValidDependenciesAsync(IEnumerable<PackageUploadDependencyInfo> dependencies)
    {
        foreach (var dependency in dependencies)
        {
            if (
                !await _context
                    .Packages.AsNoTracking()
                    .Select(p => new
                    {
                        p.Id,
                        p.NormalizedName,
                        p.Name,
                    })
                    .AnyAsync(x =>
                        x.Id == dependency.Id
                        && x.NormalizedName == dependency.NormalizedName
                        && x.Name == dependency.Name
                    )
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
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePackageAsync(Package package)
    {
        ArgumentNullException.ThrowIfNull(package);
        _context.Packages.Update(package);
        await _context.SaveChangesAsync();
    }

    public async Task<int?> GetNextIdAsync()
    {
        int[] array = await _context
            .Database.SqlQueryRaw<int>("SELECT nextval('public.\"Packages_Id_seq\"')")
            .ToArrayAsync();

        return array.Length == 0 ? null : array[0];
    }
}
