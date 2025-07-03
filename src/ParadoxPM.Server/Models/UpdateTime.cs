using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace ParadoxPM.Server.Models;

public sealed class UpdateTime
{
    [Required]
    public required int Id { get; set; } = 1;

    [Required]
    public required DateTime PackageLastModified { get; set; } = DateTime.UtcNow;
}
