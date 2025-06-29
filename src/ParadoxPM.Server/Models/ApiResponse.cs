namespace ParadoxPM.Server.Models;

public sealed class ApiResponse<T>(int code, string message, T data)
{
    public int Code { get; set; } = code;
    public string Message { get; set; } = message;
    public T Data { get; set; } = data;
}
