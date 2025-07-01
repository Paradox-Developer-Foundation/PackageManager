using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace ParadoxPM.Server.Models;

[SuppressMessage("ReSharper", "PropertyCanBeMadeInitOnly.Global")]
public sealed class Package
{
    /// <summary>
    /// 唯一序号
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// 规范化名称
    /// </summary>
    [Required]
    [StringLength(100)]
    public required string NormalizedName { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [StringLength(128)]
    public string? Description { get; set; }

    /// <summary>
    /// 游戏类型
    /// <br />
    /// 用于判断此包属于哪种游戏，如 "ck3" 为十字军之王Ⅲ, "eu4" 为欧陆风云Ⅳ等
    /// </summary>
    [Required]
    [StringLength(10)]
    public required string Arch { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [Required]
    public required bool IsActive { get; set; } = true;

    /// <summary>
    /// 作者
    /// </summary>
    [Required]
    [StringLength(50)]
    public required string Author { get; set; }

    /// <summary>
    /// 许可证
    /// </summary>
    [StringLength(50)]
    public string? License { get; set; }

    /// <summary>
    /// 仓库地址
    /// </summary>
    [Url]
    [StringLength(200)]
    public string? Repository { get; set; }

    /// <summary>
    /// 主页地址
    /// </summary>
    [Url]
    [StringLength(200)]
    public string? Homepage { get; set; }

    [Required]
    public required List<PackageVersion> Versions { get; set; } 
}
