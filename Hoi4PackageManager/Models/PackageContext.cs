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
