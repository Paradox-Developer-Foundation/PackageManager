using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ParadoxPM.Server.ViewModels;

public sealed partial class FileUploadViewModel
{
    [Required]
    public required IFormFile File { get; set; }

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

    [Required]
    [MaxLength(64)]
    public required string Sha256 { get; set; }

    public bool IsActive { get; set; } = true;

    public string Dependencies { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public required string Arch { get; set; }

    [GeneratedRegex("^[a-fA-F0-9]{64}$")]
    private static partial Regex Sha256Regex();

    [GeneratedRegex(@"^[\P{C}\s]*$")]
    private static partial Regex ValidNameRegex();

    [GeneratedRegex(@"\s")]
    private static partial Regex WhitespaceRegex();

    public bool IsValid(out IEnumerable<string> errorMessages)
    {
        var errorList = new List<string>();
        if (File.Length == 0)
        {
            errorList.Add("文件不能为空");
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            errorList.Add("名称不能为空");
        }

        if (!ValidNameRegex().IsMatch(Name) || WhitespaceRegex().IsMatch(Name.Replace(" ", string.Empty)))
        {
            errorList.Add("名称不能包含空格或不可见字符");
        }

        if (string.IsNullOrWhiteSpace(NormalizedName))
        {
            errorList.Add("规范名称不能为空");
        }

        if (!NormalizedName.All(char.IsAsciiLetterLower))
        {
            errorList.Add("规范名称只能包含小写字母");
        }

        if (string.IsNullOrWhiteSpace(Version))
        {
            errorList.Add("版本不能为空");
        }

        if (!System.Version.TryParse(Version, out _))
        {
            errorList.Add("版本格式不正确，应为 x.y(.z(.e))");
        }

        if (string.IsNullOrWhiteSpace(Sha256))
        {
            errorList.Add("SHA256 不能为空");
        }

        if (!Sha256Regex().IsMatch(Sha256))
        {
            errorList.Add("SHA256 格式不正确");
        }

        if (
            Dependencies
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Any(dependent => dependent.Any(char.IsAsciiLetterUpper))
        )
        {
            errorList.Add("依赖项名称无效");
        }

        errorMessages = errorList;
        return errorList.Count == 0;
    }
}
