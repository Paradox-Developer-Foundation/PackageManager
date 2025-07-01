using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ParadoxPM.Server.ViewModels;

public sealed partial class FileUploadViewModel
{
    [Required]
    public required IFormFile File { get; set; }

    [Required]
    public required string PackageInfoJson { get; set; }
}

public sealed partial class PackageInfo
{
    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    [StringLength(100)]
    public required string NormalizedName { get; set; }

    [StringLength(128)]
    public string? Description { get; set; }

    [Required]
    [StringLength(10)]
    public required string Arch { get; set; }

    [Required]
    [StringLength(50)]
    public required string Version { get; set; }

    [Required]
    public List<UploadDependency> Dependencies { get; set; } = [];

    [Required]
    [StringLength(100)]
    public required string Integrity { get; set; }

    [Required]
    [StringLength(50)]
    public required string Author { get; set; }

    [StringLength(50)]
    public string? License { get; set; }

    [Url]
    [StringLength(100)]
    public string? Repository { get; set; }

    [Url]
    [StringLength(100)]
    public string? Homepage { get; set; }

    [GeneratedRegex("^sha256-[a-fA-F0-9]{64}$")]
    private static partial Regex Sha256Regex();

    [GeneratedRegex(@"^[\P{C}\s]*$")]
    private static partial Regex ValidNameRegex();
    
    [GeneratedRegex(@"^[a-z0-9]+$")]
    private static partial Regex ValidNormalizedNameRegex();

    [GeneratedRegex(@"\s")]
    private static partial Regex WhitespaceRegex();

    private static List<string> ValidArchList()
    {
        return ["hoi4", "ck3", "eu4", "eu5", "st", "vic3", "csl", "csl2"];
    }

    public bool IsValid(out IEnumerable<string> errorMessages)
    {
        var errorList = new List<string>();

        if (!ValidNameRegex().IsMatch(Name) || WhitespaceRegex().IsMatch(Name.Replace(" ", string.Empty)))
        {
            errorList.Add("名称不能包含空格或不可见字符");
        }

        if (!ValidNormalizedNameRegex().IsMatch(NormalizedName))
        {
            errorList.Add("规范名称只能包含小写字母");
        }

        if (!System.Version.TryParse(Version, out _))
        {
            errorList.Add("版本格式不正确，应为 x.y(.z(.e))");
        }

        if (!Sha256Regex().IsMatch(Integrity))
        {
            errorList.Add("SHA256 格式不正确");
        }

        if (!ValidArchList().Contains(Arch))
        {
            errorList.Add("游戏类型不正确");
        }

        errorMessages = errorList;
        return errorList.Count == 0;
    }
}

public sealed class UploadDependency
{
    [Required]
    public required int DependencyId { get; set; }

    [Required]
    [StringLength(100)]
    public required string NormalizedName { get; set; }

    [Required]
    [StringLength(20)]
    public required string MinVersion { get; set; }
}
