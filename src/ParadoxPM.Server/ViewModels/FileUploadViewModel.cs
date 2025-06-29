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

    [GeneratedRegex("^[a-fA-F0-9]{64}$")]
    private static partial Regex Sha256Regex();

    [GeneratedRegex(@"^[\P{C}\s]*$")]
    private static partial Regex ValidNameRegex();

    [GeneratedRegex(@"\s")]
    private static partial Regex WhitespaceRegex();

    public void ValidCheck()
    {
        if (File is null || File.Length == 0)
        {
            throw new ValidationException("文件不能为空");
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ValidationException("名称不能为空");
        }

        if (ValidNameRegex().IsMatch(Name) && !WhitespaceRegex().IsMatch(Name.Replace(" ", string.Empty)))
        {
            throw new ValidationException("名称不能包含空格或不可见字符");
        }

        if (string.IsNullOrWhiteSpace(NormalizedName))
        {
            throw new ValidationException("规范名称不能为空");
        }
        if (!NormalizedName.All(char.IsAsciiLetterLower))
        {
            throw new ValidationException("规范名称只能包含小写字母");
        }

        if (string.IsNullOrWhiteSpace(Version))
        {
            throw new ValidationException("版本不能为空");
        }

        if (!System.Version.TryParse(Version, out _))
        {
            throw new ValidationException("版本格式不正确，应为 x.y(.z(.e))");
        }

        if (string.IsNullOrWhiteSpace(Sha256))
        {
            throw new ValidationException("SHA256 不能为空");
        }

        if (Sha256Regex().IsMatch(Sha256))
        {
            throw new ValidationException("SHA256 格式不正确");
        }

        if (
            Dependencies
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Any(dependent => dependent.Any(char.IsAsciiLetterUpper))
        )
        {
            throw new ValidationException("依赖项名称错误");
        }
    }
}
