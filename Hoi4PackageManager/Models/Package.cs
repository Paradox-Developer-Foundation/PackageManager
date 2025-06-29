using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Hoi4PackageManager.Models;

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
    [Column(TypeName = "char(64)")]
    public required string Sha256 { get; set; }

    public DateTime UploadDate { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    [ConcurrencyCheck]
    public int DownloadCount { get; set; }

    [Required]
    [MaxLength(64)]
    public required string FilePath { get; set; }

    public List<string> Dependencies { get; set; } = [];
}
