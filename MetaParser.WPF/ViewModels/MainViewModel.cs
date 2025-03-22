using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GongSolutions.Wpf.DragDrop;
using MetaParser.WPF.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace MetaParser.WPF.ViewModels;

public partial class MainViewModel
    : ObservableRecipient, IDropTarget
{
    private sealed class SimpleDisposable : IDisposable
    {
        private readonly Action dispAction;

        public SimpleDisposable(Action dispAction)
        {
            this.dispAction = dispAction;
        }

        public void Dispose() => dispAction();
    }

    private static readonly string RECENT_FILE_NAME = Path.Combine(FileSystemService.AppDataDirectory, "RecentFiles.json");
    private const int RECENT_FILE_COUNT = 10;

    private readonly FileSystemService fileSystemService;
    private readonly DialogService dialogService;
    private readonly ConditionViewModelFactory conditionViewModelFactory;
    private readonly ActionViewModelFactory actionViewModelFactory;
    private readonly ClipboardService clipboardService;
    private FileSystemWatcher fw = null;
    private Timer t = null;

    [ObservableProperty]
    private string fileName;

    partial void OnFileNameChanging(string oldValue, string newValue)
    {
        if (fw != null)
        {
            fw.Changed -= Fw_Changed;
            fw.Dispose();
            fw = null;
        }
    }

    partial void OnFileNameChanged(string oldValue, string newValue)
    {
        OnPropertyChanged(nameof(FileNameDisplay));

        if (!string.IsNullOrEmpty(newValue))
        {
            RecentFiles.RemoveAll(f => f.Equals(newValue));
            RecentFiles.Insert(0, newValue);

            while (RecentFiles.Count > RECENT_FILE_COUNT)
                RecentFiles.RemoveAt(RecentFiles.Count - 1);

            OnPropertyChanged(nameof(RecentFiles));

            Task.Run(async () =>
            {
                fileSystemService.TryCreateDirectory(Path.GetDirectoryName(RECENT_FILE_NAME));
                using var fs = fileSystemService.OpenFileForWriteAccess(RECENT_FILE_NAME);
                await JsonSerializer.SerializeAsync(fs, RecentFiles).ConfigureAwait(false);
            });

            fw = new(Path.GetDirectoryName(newValue), Path.GetFileName(newValue));
            fw.Changed += Fw_Changed;
            fw.EnableRaisingEvents = true;
        }
    }

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string busyStatus;

    public string FileNameDisplay => !string.IsNullOrEmpty(FileName) ? Path.GetFileName(FileName) : "[New File]";

    [ObservableProperty]
    private MetaViewModel metaViewModel;

    [RelayCommand]
    async Task NewFile()
    {
        if (await DirtyCheck().ConfigureAwait(false))
            return;

        // Initialize new meta
        MetaViewModel = new(conditionViewModelFactory, actionViewModelFactory, clipboardService);
        FileName = null;
    }

    [RelayCommand]
    async Task OpenFile()
    {
        if (await DirtyCheck().ConfigureAwait(false))
            return;

        var result = dialogService.ShowOpenFileDialog("Open Meta File…", out var fileName, FileName, "Meta Files (*.met)|*.met|MiMB Files (*.xml)|*.xml|Metaf files (*.af)|*.af");
        if (result.HasValue && result.Value == true)
        {
            await OpenFile(fileName).ConfigureAwait(false);
        }
    }

    [RelayCommand]
    async Task ImportFile()
    {
        var result = dialogService.ShowOpenFileDialog("Import Meta File…", out var fileName, FileName, "Meta Files (*.met)|*.met");
        if (result.HasValue && result == true)
        {
            await ImportFile(fileName).ConfigureAwait(false);
        }
    }

    [RelayCommand]
    async Task SaveFile()
    {
        if (string.IsNullOrEmpty(FileName) || Path.GetExtension(FileName).ToLower() != ".met")
        {
            await SaveFileAs().ConfigureAwait(false);
        }
        else
        {
            try
            {
                if (fw != null)
                    fw.EnableRaisingEvents = false;

                using (GetBusy("Saving file..."))
                {
                    try
                    {
                        using var ms = new MemoryStream();
                        await Formatters.MetaWriter.WriteMetaAsync(ms, MetaViewModel.Meta).ConfigureAwait(false);
                        MetaViewModel.Clean();

                        await File.WriteAllBytesAsync(FileName, ms.ToArray());
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"There was an error saving the meta file. The message returned was: {ex.Message}", "Error Saving File", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            finally
            {
                if (fw != null)
                    fw.EnableRaisingEvents = true;
            }
        }
    }

    [RelayCommand]
    async Task SaveFileAs()
    {
        var result = dialogService.ShowSaveFileDialog("Save Meta File…", out var fileName, FileName, "Meta Files (*.met)|*.met");
        if (result.HasValue && result.Value)
        {
            FileName = fileName;
            await SaveFile().ConfigureAwait(false);
        }
    }

    [RelayCommand]
    async Task Closing(CancelEventArgs e)
    {
        if (await DirtyCheck().ConfigureAwait(false))
            e.Cancel = true;
    }

    [RelayCommand]
    async Task OpenRecentFile(string f)
    {
        if (await DirtyCheck().ConfigureAwait(false))
            return;

        if (fileSystemService.FileExists(f))
        {
            await OpenFile(f).ConfigureAwait(false);
        }
        else
        {
            RecentFiles.RemoveAll(f => FileName.Equals(f, System.StringComparison.OrdinalIgnoreCase));
            OnPropertyChanged(nameof(RecentFiles));
            MessageBox.Show($"The file {f} is no longer on disk. Removing from recent files list.",
                "File Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public List<string> RecentFiles { get; } = [];

    public MainViewModel(FileSystemService fileSystemService, DialogService dialogService, ConditionViewModelFactory conditionViewModelFactory, ActionViewModelFactory actionViewModelFactory, ClipboardService clipboardService)
    {
        this.fileSystemService = fileSystemService;
        this.dialogService = dialogService;
        this.conditionViewModelFactory = conditionViewModelFactory;
        this.actionViewModelFactory = actionViewModelFactory;
        this.clipboardService = clipboardService;
        if (fileSystemService.FileExists(RECENT_FILE_NAME))
        {
            Task.Run(async () =>
            {
                using var fs = fileSystemService.OpenFileForReadAccess(RECENT_FILE_NAME);
                await foreach (var file in JsonSerializer.DeserializeAsyncEnumerable<string>(fs).ConfigureAwait(false))
                {
                    RecentFiles.Add(file);
                }
                while (RecentFiles.Count > RECENT_FILE_COUNT)
                    RecentFiles.RemoveAt(RecentFiles.Count - 1);
                OnPropertyChanged(nameof(RecentFiles));
            });
        }

        if (Application.Current.Properties["InitialFile"] != null)
        {
            if (fileSystemService.FileExists((string)Application.Current.Properties["InitialFile"]))
                OpenFile((string)Application.Current.Properties["InitialFile"]).GetAwaiter().GetResult();
        }
        else
            MetaViewModel = new(conditionViewModelFactory, actionViewModelFactory, clipboardService);
    }

    private async Task OpenFile(string fileName)
    {
        using var _ = GetBusy("Opening file...");
        try
        {
            using var fs = fileSystemService.OpenFileForReadAccess(fileName);
            var m = await (Path.GetExtension(fileName).ToLower() switch
            {
                ".xml" => Formatters.XMLMetaReader.ReadMetaAsync(fs),
                ".af" => Formatters.MetafReader.ReadMetaAsync(fs),
                _ => Formatters.DefaultMetaReader.ReadMetaAsync(fs)
            }).ConfigureAwait(false);

            FileName = fileName;
            MetaViewModel = new(m, conditionViewModelFactory, actionViewModelFactory, clipboardService);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"There was an error opening the meta file. The message returned was: {ex.Message}", "Error Opening File", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ImportFile(string fileName)
    {
        using var _ = GetBusy("Importing file...");
        try
        {
            using var fs = fileSystemService.OpenFileForReadAccess(fileName);
            var m = await Formatters.DefaultMetaReader.ReadMetaAsync(fs).ConfigureAwait(false);

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var rule in m.Rules)
                    MetaViewModel.Rules.Add(new(rule, MetaViewModel, conditionViewModelFactory, actionViewModelFactory, clipboardService));
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"There was an error importing the meta file. The message returned was: {ex.Message}", "Error Importing File", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task<bool> DirtyCheck()
    {
        if (MetaViewModel.IsDirty)
        {
            var response = MessageBox.Show($"{FileNameDisplay} has unsaved changes. Would you like to save before closing?", "Unsaved Changes", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (response == MessageBoxResult.Cancel)
                return true;
            else if (response == MessageBoxResult.Yes)
            {
                // save file
                await SaveFile().ConfigureAwait(false);
            }
        }

        return false;
    }

    private void Fw_Changed(object sender, FileSystemEventArgs e)
    {
        t?.Dispose();
        t = new(new TimerCallback(Reload), null, 2000, Timeout.Infinite);
    }

    private async void Reload(object _)
    {
        t.Dispose();
        t = null;

        var result = MessageBox.Show($"{FileNameDisplay} has changed on disk. Would you like the reload the file?", "File Changed", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            await OpenFile(FileName).ConfigureAwait(false);
        }
    }

    private IDisposable GetBusy(string busyStatus)
    {
        BusyStatus = busyStatus;
        IsBusy = true;
        return new SimpleDisposable(() =>
        {
            IsBusy = false;
        });
    }

    public void DragOver(IDropInfo dropInfo)
    {
        if (dropInfo.Data is DataObject obj && obj.GetDataPresent(DataFormats.FileDrop))
            dropInfo.Effects = DragDropEffects.Move;
        else
            dropInfo.NotHandled = true;
    }

    public async void Drop(IDropInfo dropInfo)
    {
        if (dropInfo.Data is DataObject obj && obj.GetDataPresent(DataFormats.FileDrop))
        {
            var files = (string[])obj.GetData(DataFormats.FileDrop);
            var file = files?.FirstOrDefault(f => Regex.IsMatch(Path.GetExtension(f), @"^(\.xml|\.met|\.af)$"));
            if (file != null)
            {
                if (await DirtyCheck().ConfigureAwait(false))
                    return;
                await OpenFile(file).ConfigureAwait(false);
            }
        }
        else
            dropInfo.NotHandled = true;
    }
}
