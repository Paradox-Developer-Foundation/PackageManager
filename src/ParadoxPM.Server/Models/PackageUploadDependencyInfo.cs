using System.ComponentModel.DataAnnotations;

namespace ParadoxPM.Server.Models;

public sealed class PackageUploadDependencyInfo
{
    [Required]
    public required int Id { get; set; }

    [Required]
    [StringLength(100)]
    public required string NormalizedName { get; set; }

    [Required]
    [StringLength(20)]
    public required string MinVersion { get; set; }
}