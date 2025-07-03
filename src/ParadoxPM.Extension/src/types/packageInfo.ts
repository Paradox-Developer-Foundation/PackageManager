/**
 * 包
 */
export interface PackageInfo {
    /**
     * 游戏类型
     */
    arch: Arch;
    /**
     * 作者
     */
    author: string;
    /**
     * 描述
     */
    description?: string;
    /**
     * 主页地址
     */
    homepage?: string;
    /**
     * 唯一序号
     */
    id: number;
    /**
     * 是否启用
     */
    isActive: boolean;
    /**
     * 许可证
     */
    license?: string;
    /**
     * 名称
     */
    name: string;
    /**
     * 规范化名称
     */
    normalizedName: string;
    /**
     * 仓库地址
     */
    repository?: string;
    /**
     * 版本表
     */
    versions: Version[];
    [property: string]: any;
}

/**
 * 游戏类型
 */
export enum Arch {
    Ck3 = "ck3",
    Csl = "csl",
    Csl2 = "csl2",
    Eu4 = "eu4",
    Eu5 = "eu5",
    Hoi4 = "hoi4",
    St = "st",
    Vic2 = "vic2",
    Vic3 = "vic3",
}

export interface Version {
    /**
     * 依赖项
     */
    dependencies: Dependency[];
    /**
     * 下载次数
     */
    downloadCount: number;
    /**
     * 验证数据
     */
    integrity: string;
    /**
     * 下载地址
     */
    tarball: string;
    /**
     * 上传时间
     */
    uploadTime: Date;
    /**
     * 版本号
     */
    version: string;
    [property: string]: any;
}

export interface Dependency {
    /**
     * 唯一序号
     */
    id: number;
    /**
     * 最小版本号
     */
    minVersion: string;
    /**
     * 规范化名称
     */
    normalizedName: string;
    [property: string]: any;
}