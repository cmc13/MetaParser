using System.IO;

namespace MetaParser;

class FileSystemService : IFileSystemService
{
    public bool FileExists(string path) => File.Exists(path);
    public Stream OpenFileForReadAccess(string path) => File.OpenRead(path);
}
