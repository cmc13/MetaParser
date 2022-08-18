using GongSolutions.Wpf.DragDrop;
using MetaParser.WPF.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
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

namespace MetaParser.WPF.ViewModels
{
    public class MainViewModel
        : ObservableRecipient, IDropTarget
    {
        private class SimpleDisposable : IDisposable
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

        private MetaViewModel metaViewModel;
        private string fileName;
        private bool isBusy;
        private string busyStatus;
        private readonly FileSystemService fileSystemService = new();
        private FileSystemWatcher fw = null;
        private Timer t = null;

        public string FileName
        {
            get => fileName;
            set
            {
                if (fileName != value)
                {
                    if (fw != null)
                    {
                        fw.Changed -= Fw_Changed;
                        fw.Dispose();
                        fw = null;
                    }    

                    fileName = value;
                    OnPropertyChanged(nameof(FileName));
                    OnPropertyChanged(nameof(FileNameDisplay));

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        RecentFiles.RemoveAll(f => f.Equals(fileName));
                        RecentFiles.Insert(0, fileName);

                        while (RecentFiles.Count > RECENT_FILE_COUNT)
                            RecentFiles.RemoveAt(RecentFiles.Count - 1);

                        OnPropertyChanged(nameof(RecentFiles));

                        Task.Run(async () =>
                        {
                            fileSystemService.TryCreateDirectory(Path.GetDirectoryName(RECENT_FILE_NAME));
                            using var fs = fileSystemService.OpenFileForWriteAccess(RECENT_FILE_NAME);
                            await JsonSerializer.SerializeAsync(fs, RecentFiles).ConfigureAwait(false);
                        });

                        fw = new(Path.GetDirectoryName(fileName), Path.GetFileName(fileName));
                        fw.Changed += Fw_Changed;
                        fw.EnableRaisingEvents = true;
                    }
                }
            }
        }

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (IsBusy != value)
                {
                    isBusy = value;
                    OnPropertyChanged(nameof(IsBusy));
                }
            }
        }

        public string BusyStatus
        {
            get => busyStatus;
            set
            {
                if (busyStatus != value)
                {
                    busyStatus = value;
                    OnPropertyChanged(nameof(BusyStatus));
                }
            }
        }

        public string FileNameDisplay => !string.IsNullOrEmpty(FileName) ? Path.GetFileName(FileName) : "[New File]";

        public MetaViewModel MetaViewModel
        {
            get => metaViewModel;
            set
            {
                if (metaViewModel != value)
                {
                    metaViewModel = value;
                    OnPropertyChanged(nameof(MetaViewModel));
                }
            }
        }

        public AsyncRelayCommand NewFileCommand { get; }

        public AsyncRelayCommand OpenFileCommand { get; }

        public AsyncRelayCommand ImportFileCommand { get; }

        public AsyncRelayCommand SaveFileCommand { get; }

        public AsyncRelayCommand SaveFileAsCommand { get; }

        public AsyncRelayCommand<CancelEventArgs> ClosingCommand { get; }

        public AsyncRelayCommand<string> OpenRecentFileCommand { get; }

        public List<string> RecentFiles { get; } = new();

        public MainViewModel()
        {
            if (fileSystemService.FileExists(RECENT_FILE_NAME))
            {
                Task.Run(async () =>
                {
                    using var fs = fileSystemService.OpenFileForReadAccess(RECENT_FILE_NAME);
                    var files = await JsonSerializer.DeserializeAsync<IEnumerable<string>>(fs).ConfigureAwait(false);
                    RecentFiles.AddRange(files);
                    while (RecentFiles.Count > RECENT_FILE_COUNT)
                        RecentFiles.RemoveAt(RECENT_FILE_COUNT - 1);
                    OnPropertyChanged(nameof(RecentFiles));
                });
            }

            NewFileCommand = new(async () =>
            {
                if (await DirtyCheck().ConfigureAwait(false))
                    return;

                // Initialize new meta
                MetaViewModel = new();
                FileName = null;
            });

            OpenFileCommand = new(async () =>
            {
                if (await DirtyCheck().ConfigureAwait(false))
                    return;

                var ofd = new OpenFileDialog()
                {
                    Filter = "Meta Files (*.met)|*.met|MiMB Files (*.xml)|*.xml|Metaf files (*.af)|*.af",
                    Multiselect = false,
                    Title = "Open Meta File..."
                };

                if (!string.IsNullOrWhiteSpace(FileName))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(FileName);
                    ofd.FileName = Path.GetFileName(FileName);
                }
                else if (fileSystemService.DirectoryExists(@"C:\Games\VirindiPlugins\VirindiTank\"))
                    ofd.InitialDirectory = @"C:\Games\VirindiPlugins\VirindiTank\";
                else
                    ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                var result = ofd.ShowDialog();
                if (result.HasValue && result.Value == true)
                {
                    await OpenFile(ofd.FileName).ConfigureAwait(false);
                }
            });

            ImportFileCommand = new(async () =>
            {
                var ofd = new OpenFileDialog()
                {
                    Filter = "Meta Files (*.met)|*.met",
                    Multiselect = false,
                    Title = "Import Meta File"
                };

                if (!string.IsNullOrWhiteSpace(FileName))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(FileName);
                    ofd.FileName = Path.GetFileName(FileName);
                }
                else if (fileSystemService.DirectoryExists(@"C:\Games\VirindiPlugins\VirindiTank\"))
                    ofd.InitialDirectory = @"C:\Games\VirindiPlugins\VirindiTank\";
                else
                    ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                var result = ofd.ShowDialog();
                if (result.HasValue && result == true)
                {
                    await ImportFile(ofd.FileName).ConfigureAwait(false);
                }
            });

            SaveFileCommand = new(async () =>
            {
                if (string.IsNullOrEmpty(FileName) || Path.GetExtension(FileName).ToLower() != ".met")          
                {
                    await SaveFileAsCommand.ExecuteAsync(null).ConfigureAwait(false);
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
                                using var fs = fileSystemService.OpenFileForWriteAccess(FileName);
                                await Formatters.MetaWriter.WriteMetaAsync(fs, MetaViewModel.Meta).ConfigureAwait(false);
                                MetaViewModel.Clean();
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
            });

            SaveFileAsCommand = new(async () =>
            {
                var sfd = new SaveFileDialog()
                {
                    OverwritePrompt = true,
                    Filter = "Meta Files (*.met)|*.met",
                    Title = "Save Meta File..."
                };

                if (!string.IsNullOrWhiteSpace(FileName))
                {
                    sfd.FileName = Path.GetFileName(FileName);
                    sfd.InitialDirectory = Path.GetDirectoryName(FileName);
                }

                var result = sfd.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    FileName = sfd.FileName;
                    await SaveFileCommand.ExecuteAsync(null).ConfigureAwait(false);
                }
            });

            ClosingCommand = new(async e =>
            {
                if (await DirtyCheck().ConfigureAwait(false))
                    e.Cancel = true;
            });

            OpenRecentFileCommand = new(async f =>
            {
                if (await DirtyCheck().ConfigureAwait(false))
                    return;

                if (fileSystemService.FileExists(f))
                {
                    await OpenFile(f).ConfigureAwait(false);
                }
                else
                {
                    RecentFiles.RemoveAll(f => fileName.Equals(f, System.StringComparison.OrdinalIgnoreCase));
                    OnPropertyChanged(nameof(RecentFiles));
                    MessageBox.Show($"The file {f} is no longer on disk. Removing from recent files list.",
                        "File Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });

            MetaViewModel = new();
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
                MetaViewModel = new(m);
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
                        MetaViewModel.Rules.Add(new(rule, MetaViewModel));
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
                    await SaveFileCommand.ExecuteAsync(null).ConfigureAwait(false);
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
}
