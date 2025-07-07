using System.Security.Cryptography;

namespace ParadoxPM.Server.Repositories;

public sealed class FileRepository : IFileRepository
{
    private readonly string _basePath;

    public FileRepository(string basePath)
    {
        _basePath = basePath;
    }

    public Task<Stream> GetFileAsync(string path)
    {
        string fullPath = Path.Combine(_basePath, path);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("文件不存在");
        }

        return Task.FromResult<Stream>(new FileStream(fullPath, FileMode.Open, FileAccess.Read));
    }

    public Task<Stream> DeleteFileAsync(string path)
    {
        string fullPath = Path.Combine(_basePath, path);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException();
        }
        File.Delete(fullPath);

        return Task.FromResult<Stream>(new MemoryStream());
    }

    public async Task<Stream> UpdateFileAsync(string path, Stream fileStream)
    {
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }
        string fullPath = Path.Combine(_basePath, path);
        await using (var file = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            await fileStream.CopyToAsync(file);
        }

        return new MemoryStream();
    }

    public async Task<string> GetFileSha256Async(Stream fileStream)
    {
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        byte[] hashBytes = await SHA256.HashDataAsync(fileStream);

        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    public async Task<bool> CheckFileIntegrityAsync(Stream fileStream, string expectedSha256)
    {
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }
        string fileSha256 = "sha256-" + await GetFileSha256Async(fileStream);
        return fileSha256.Equals(expectedSha256, StringComparison.InvariantCultureIgnoreCase);
    }

    public async Task SaveFileAsync(string path, Stream fileStream)
    {
        if (fileStream.CanSeek)
        {
            fileStream.Position = 0;
        }

        string fullPath = Path.Combine(_basePath, path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? string.Empty);
        await using var file = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await fileStream.CopyToAsync(file);
    }
}
