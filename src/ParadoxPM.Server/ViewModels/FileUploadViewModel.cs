using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ParadoxPM.Server.ViewModels;

public sealed class FileUploadViewModel
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

    public string? Dependencies { get; set; }

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
        var str = Name;
        var flag = Regex.IsMatch(str, @"^[\P{C}\s]*$") && !Regex.IsMatch(str.Replace(" ", ""), @"[\s]");
        if (!flag)
        {
            throw new ValidationException("名称不能包含空格或不可见字符");
        }

        if (string.IsNullOrWhiteSpace(NormalizedName))
        {
            throw new ValidationException("规范名称不能为空");
        }
        str = NormalizedName;
        flag = Regex.IsMatch(str, @"^[a-z]*$");
        if (!flag)
        {
            throw new ValidationException("规范名称只能包含小写字母");
        }

        if (string.IsNullOrWhiteSpace(Version))
        {
            throw new ValidationException("版本不能为空");
        }

        str = Version;
        flag = Regex.IsMatch(str, @"^(0|[1-9]\d*)(\.(0|[1-9]\d*)){1,3}$");
        if (!flag)
        {
            throw new ValidationException("版本格式不正确，应为 x.y(.z(.e))");
        }

        if (string.IsNullOrWhiteSpace(Sha256))
        {
            throw new ValidationException("SHA256 不能为空");
        }
        str = Sha256;
        flag = Regex.IsMatch(str, @"^[a-fA-F0-9]{64}$");
        if (!flag)
        {
            throw new ValidationException("SHA256 格式不正确");
        }

        str = Dependencies ?? "";
        if (str.Split('|').Any(s => !Regex.IsMatch(s, @"^[a-z]*$")))
        {
            throw new ValidationException("依赖项名称错误");
        }
    }
}
