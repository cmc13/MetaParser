using System;
using System.Collections.Generic;
using System.IO;

namespace MetaParser.WPF.Services
{
    public class FileSystemService
    {
        public static string AppDataDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MetaParser");

        public Stream OpenFileForReadAccess(string fileName) => File.OpenRead(fileName);
        public Stream OpenFileForWriteAccess(string fileName) => new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
        public IEnumerable<string> GetFilesInDirectory(string directory, string pattern) => Directory.EnumerateFiles(directory, pattern);
        public void MoveFile(string fileName, string newFileName) => File.Move(fileName, newFileName);
        public bool FileExists(string fileName) => File.Exists(fileName);
        public void DeleteFile(string fileName) => File.Delete(fileName);
        public bool DirectoryExists(string directory) => Directory.Exists(directory);
        public bool TryCreateDirectory(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                return true;
            }

            return false;
        }
    }
}
