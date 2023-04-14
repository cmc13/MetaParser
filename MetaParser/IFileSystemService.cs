using System.IO;

namespace MetaParser;

public interface IFileSystemService
{
    bool FileExists(string path);
    Stream OpenFileForReadAccess(string path);
}