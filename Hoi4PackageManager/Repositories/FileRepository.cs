using System.Security.Cryptography;

namespace Hoi4PackageManager.Repositories;

public sealed class FileRepository : IFileRepository
{
    private readonly string _basePath;

    public FileRepository(string basePath)
    {
        _basePath = basePath;
    }

    public async Task<Stream> GetFileAsync(string path)
    {
        var fullPath = Path.Combine(_basePath, path);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException();
        }
        return await Task.FromResult(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
    }

    public async Task<Stream> DeleteFileAsync(string path)
    {
        var fullPath = Path.Combine(_basePath, path);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException();
        }
        File.Delete(fullPath);
        return await Task.FromResult(new MemoryStream());
    }

    public async Task<Stream> UpdateFileAsync(string path, Stream fileStream)
    {
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }
        var fullPath = Path.Combine(_basePath, path);
        await using (var file = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            await fileStream.CopyToAsync(file);
        }
        return await Task.FromResult(new MemoryStream());
    }

    public async Task<string> GetFileSha256Async(Stream fileStream)
    {
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        byte[] hashBytes = await SHA3_256.HashDataAsync(fileStream);
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public async Task SaveFileAsync(string path, Stream fileStream)
    {
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }
        var fullPath = Path.Combine(_basePath, path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? string.Empty);
        await using var file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await fileStream.CopyToAsync(file);
    }
}
