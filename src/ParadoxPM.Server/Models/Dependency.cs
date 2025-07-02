using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ParadoxPM.Server.Models;

public sealed class Dependency
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public required int DependencyId { get; set; }

    [Required]
    [StringLength(100)]
    public required string NormalizedName { get; set; }

    [Required]
    [StringLength(20)]
    public required string MinVersion { get; set; }

    [Required]
    [ForeignKey("PackageVersion")]
    public int PackageVersionId { get; set; }

    [JsonIgnore] // 防止序列化循环引用
    public PackageVersion PackageVersion { get; set; } = null!;
}
