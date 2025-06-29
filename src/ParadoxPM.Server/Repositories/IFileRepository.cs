namespace ParadoxPM.Server.Repositories;

public interface IFileRepository
{
    Task<Stream> GetFileAsync(string path);
    Task<Stream> DeleteFileAsync(string path);
    Task<Stream> UpdateFileAsync(string path, Stream fileStream);
    Task SaveFileAsync(string path, Stream fileStream);
    Task<string> GetFileSha256Async(Stream fileStream);
}
