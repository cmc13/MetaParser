using Microsoft.Win32;
using System;
using System.IO;

namespace MetaParser.WPF.Services;

public sealed class DialogService
{
    private readonly FileSystemService fileSystemService;
    private static readonly string VTANK_DIRECTORY = @"C:\Games\VirindiPlugins\VirindiTank\";

    public DialogService(FileSystemService fileSystemService)
    {
        this.fileSystemService = fileSystemService;
    }

    public bool? ShowOpenFileDialog(string title, out string outFileName, string fileName = null, string filter = null)
    {
        var ofd = new OpenFileDialog()
        {
            Filter = filter ?? "Meta Files (*.met)|*.met",
            Multiselect = false,
            Title = title
        };

        SetInitialDirectory(fileName, ofd);

        var result = ofd.ShowDialog();
        if (result.HasValue && result.Value == true)
        {
            outFileName = ofd.FileName;
        }
        else
            outFileName = null;

        return result;
    }

    public bool? ShowSaveFileDialog(string title, out string outFileName, string fileName = null, string filter = null)
    {
        var sfd = new SaveFileDialog()
        {
            OverwritePrompt = true,
            Filter = "Meta Files (*.met)|*.met",
            Title = title
        };

        SetInitialDirectory(fileName, sfd);

        var result = sfd.ShowDialog();

        if (result.HasValue && result.Value == true)
        {
            outFileName = sfd.FileName;
        }
        else
            outFileName = null;

        return result;
    }

    private void SetInitialDirectory(string fileName, FileDialog fd)
    {
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            fd.FileName = Path.GetFileName(fileName);
            fd.InitialDirectory = Path.GetDirectoryName(fileName);
        }
        else if (fileSystemService.DirectoryExists(VTANK_DIRECTORY))
            fd.InitialDirectory = VTANK_DIRECTORY;
        else
            fd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }
}
