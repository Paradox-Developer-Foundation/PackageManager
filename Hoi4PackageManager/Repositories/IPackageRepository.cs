using Hoi4PackageManager.Models;

namespace Hoi4PackageManager.Repositories;

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
    /// <param name="packageNormalizedName">包的规范化名称</param>
    /// <param name="isActiveOnly">是否只获取启用的包</param>
    /// <exception cref="KeyNotFoundException">未找到符合要求的包时抛出</exception>
    /// <returns>包</returns>
    Task<Package> GetPackageAsync(
        int packageId,
        string packageNormalizedName,
        bool isActiveOnly = true
    );

    Task CheckDependenciesAsync(
        IEnumerable<string> dependencies
    );
    
    /// <summary>
    /// 增加包的下载计数
    /// </summary>
    /// <param name="packageId">包的序号</param>
    /// <param name="packageNormalizedName">包的规范化名称</param>
    /// <exception cref="KeyNotFoundException">未找到符合要求的包时抛出</exception>
    Task AddPackageDownloadCountAsync(int packageId, string packageNormalizedName);

    /// <summary>
    /// 添加新的包
    /// </summary>
    /// <param name="package">包</param>
    /// <exception cref="ArgumentNullException">当参数为空时抛出</exception>
    Task AddPackageAsync(Package package);

    /// <summary>
    /// 更新包
    /// </summary>
    /// <param name="updatePackage">包</param>
    /// <exception cref="ArgumentNullException">当参数为空时抛出</exception>
    /// <exception cref="KeyNotFoundException">未找到符合要求的包时抛出</exception>
    Task UpdatePackageAsync(Package updatePackage);

    /// <summary>
    /// 删除包
    /// </summary>
    /// <param name="packageId">包的序号</param>
    /// <param name="packageNormalizedName">包的规范化名称</param>
    /// <exception cref="KeyNotFoundException">未找到符合要求的包时抛出</exception>
    Task DeletePackageAsync(int packageId, string packageNormalizedName);
}
