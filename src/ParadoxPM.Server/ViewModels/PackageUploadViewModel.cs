using System.ComponentModel.DataAnnotations;

namespace ParadoxPM.Server.ViewModels;

public sealed partial class PackageUploadViewModel
{
    [Required]
    public required IFormFile File { get; set; }

    [Required]
    public required string PackageInfoJson { get; set; }
}
