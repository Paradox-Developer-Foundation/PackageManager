using ParadoxPM.Server.Models;
using ParadoxPM.Server.ViewModels;

namespace ParadoxPM.Server.Repositories;

public interface IPackageRepository
{
    /// <summary>
    /// 获取所有的包
    /// </summary>
    /// <param name="isActiveOnly">是否只获取启用的包</param>
    /// <returns>包的枚举器</returns>
    Task<IEnumerable<Package>> GetPackagesAsync(bool isActiveOnly = true);

    /// <summary>
    /// 获取指定的包
    /// </summary>
    /// <param name="packageId">包的序号</param>
    /// <exception cref="KeyNotFoundException">未找到符合要求的包时抛出</exception>
    /// <returns>包</returns>
    Task<Package> GetPackageAsync(int packageId);

    /// <summary>
    /// 检查所有依赖项是否有效
    /// </summary>
    /// <param name="dependencies">依赖项</param>
    /// <returns>如果任意一个依赖项无效, 返回<c>false</c>, 否则返回<c>true</c></returns>
    Task<bool> IsValidDependenciesAsync(IEnumerable<UploadDependency> dependencies);

    /// <summary>
    /// 添加新的包
    /// </summary>
    /// <param name="package">包</param>
    /// <exception cref="ArgumentNullException">当参数为空时抛出</exception>
    Task AddPackageAsync(Package package);
    Task<int?> GetNextIdAsync();
}
