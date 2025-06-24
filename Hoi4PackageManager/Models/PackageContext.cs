using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Hoi4PackageManager.Models;

public class PackageContext : DbContext
{
    public PackageContext(DbContextOptions<PackageContext> options)
        : base(options) { }

    public DbSet<Package> Packages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasIndex(p => p.NormalizedName);
            entity.HasIndex(p => new { p.Name, p.Version }).IsUnique();

            // 配置依赖项列表的转换
            entity
                .Property(p => p.Dependencies)
                .HasConversion(
                    v => string.Join('|', v), // 存储时用管道符分隔
                    v => v.Split('|', StringSplitOptions.RemoveEmptyEntries).ToList()
                );
        });
    }
}

[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public class Package
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(100)]
    public required string NormalizedName { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Version { get; set; }

    [MaxLength(128)]
    public string Description { get; set; } = "";

    [MaxLength(50)]
    public string License { get; set; } = "";

    public long Size { get; set; }

    [Required]
    [MaxLength(64)]
    // ReSharper disable once InconsistentNaming
    public required string SHA256 { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    [ConcurrencyCheck]
    public int DownloadCount { get; set; }

    [Required]
    [MaxLength(64)]
    public required string FilePath { get; set; }

    public List<string> Dependencies { get; set; } = [];
}
