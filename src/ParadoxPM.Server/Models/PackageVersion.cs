using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ParadoxPM.Server.Models;

public sealed class PackageVersion
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(20)]
    public required string Version { get; set; }

    [Required]
    [StringLength(100)]
    public required string Integrity { get; set; }

    [Required]
    [StringLength(500)]
    public required string Tarball { get; set; }

    [Required]
    public required DateTime UploadTime { get; set; }

    [Required]
    public required int DownloadCount { get; set; }

    [Required]
    [ForeignKey("Package")]
    public int PackageId { get; set; }

    [JsonIgnore] // 防止序列化循环引用
    public Package Package { get; set; } = null!;

    [Required]
    public List<Dependency> Dependencies { get; set; } = new();
}
