using Microsoft.EntityFrameworkCore;

namespace ParadoxPM.Server.Models;

public sealed class PackageContext : DbContext
{
    public PackageContext(DbContextOptions<PackageContext> options)
        : base(options) { }

    public DbSet<Package> Packages { get; set; }
    public DbSet<PackageVersion> PackageVersions { get; set; }
    public DbSet<Dependency> Dependencies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 配置关系
        modelBuilder
            .Entity<Package>()
            .HasMany(p => p.Versions)
            .WithOne(v => v.Package)
            .HasForeignKey(v => v.PackageId)
            .OnDelete(DeleteBehavior.Cascade); // 级联删除

        modelBuilder
            .Entity<PackageVersion>()
            .HasMany(v => v.Dependencies)
            .WithOne(d => d.PackageVersion)
            .HasForeignKey(d => d.PackageVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        // 配置唯一索引
        modelBuilder.Entity<Package>().HasIndex(p => new { p.NormalizedName, p.Arch }).IsUnique();

        modelBuilder.Entity<PackageVersion>().HasIndex(v => new { v.PackageId, v.Version }).IsUnique();

        // 配置列类型（PostgreSQL 特定）
        modelBuilder
            .Entity<PackageVersion>()
            .Property(v => v.UploadTime)
            .HasColumnType("timestamp with time zone");
    }
}
