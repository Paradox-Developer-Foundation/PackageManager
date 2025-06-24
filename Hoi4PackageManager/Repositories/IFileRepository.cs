using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Hoi4PackageManager.Repositories;

public interface IFileRepository
{
    Task<Stream> GetFileAsync(string path);
    Task<Stream> DeleteFileAsync(string path);
    Task<Stream> UpdateFileAsync(string path, Stream fileStream);
    Task SaveFileAsync(string path, Stream fileStream);
    // ReSharper disable once InconsistentNaming
    Task<string> GetFileSHA256Async(Stream fileStream);
}
