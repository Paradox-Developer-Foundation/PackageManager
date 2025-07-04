export interface ApiResponse<T> {
    /**
     * 状态码
     */
    code: number;
    /**
     * 消息
     */
    message: string;
    /**
     * 数据
     */
    data: T;
    [property: string]: any;
}