using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ParadoxPM.Server.Models;

public sealed partial class PackageUploadDependencyInfo
{
    [Required]
    public required int Id { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    [StringLength(100)]
    public required string NormalizedName { get; set; }

    [Required]
    [StringLength(20)]
    public required string MinVersion { get; set; }

    [GeneratedRegex(@"^[\P{C}\s]*$")]
    private static partial Regex ValidNameRegex();

    [GeneratedRegex(@"^[a-z0-9]+$")]
    private static partial Regex ValidNormalizedNameRegex();

    [GeneratedRegex(@"\s")]
    private static partial Regex WhitespaceRegex();

    public bool IsValid(out IEnumerable<string> errorMessages)
    {
        List<string> errorList = [];
        if (Id <= 0)
        {
            errorList.Add("ID 必须大于 0");
        }
        if (string.IsNullOrWhiteSpace(Name))
        {
            errorList.Add("名称不能为空");
        }
        if (string.IsNullOrWhiteSpace(NormalizedName))
        {
            errorList.Add("规范化名称不能为空");
        }
        if (string.IsNullOrWhiteSpace(MinVersion))
        {
            errorList.Add("最小版本不能为空");
        }
        if (!ValidNameRegex().IsMatch(Name) || WhitespaceRegex().IsMatch(Name.Replace(" ", string.Empty)))
        {
            errorList.Add("名称不能包含空格或不可见字符");
        }

        if (!ValidNormalizedNameRegex().IsMatch(NormalizedName))
        {
            errorList.Add("规范名称只能包含小写字母");
        }

        if (!System.Version.TryParse(MinVersion, out _))
        {
            errorList.Add("最小版本格式不正确，应为 x.y(.z(.e))");
        }
        errorMessages = errorList;
        return errorList.Count == 0;
    }
}
