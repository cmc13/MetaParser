using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace MetaParser.Maui.ViewModels;

public partial class MainViewModel
    : ObservableObject
{
    [RelayCommand]
    async Task NewFile()
    {
        await Task.Delay(0);
    }

    [RelayCommand(CanExecute = nameof(CanOpenFile))]
    async Task OpenFile()
    {
        await Shell.Current.GoToAsync(nameof(MetaPage), new Dictionary<string, object>()
        {
            { nameof(MetaViewModel.FileName), SelectedFile.FullName }
        });
    }

    bool CanOpenFile() => SelectedFile != null;

    [RelayCommand]
    async Task FileDoubleClicked()
    {
        await Task.Delay(0);
    }

    [RelayCommand]
    async Task ChooseLocation()
    {
        var result = await FolderPicker.Default.PickAsync(CurrentLocation, CancellationToken.None);
        if (result.IsSuccessful)
        {
            CurrentLocation = result.Folder.Path;
        }
    }

    [RelayCommand]
    void Appearing()
    {
        OnCurrentLocationChanged(CurrentLocation);
    }

    [ObservableProperty]
    private string currentLocation = @"C:\Games\VirindiPlugins\VirindiTank";

    [ObservableProperty]
    private ObservableCollection<FileInfo> files = new();

    [ObservableProperty]
    private FileInfo selectedFile;

    partial void OnCurrentLocationChanged(string value)
    {
        Files.Clear();
        foreach (var file in new[] { "*.met", "*.af", "*.xml" }.SelectMany(pat => Directory.EnumerateFiles(value, pat)).OrderBy(f => f))
            Files.Add(new FileInfo(file));
    }

    partial void OnSelectedFileChanged(FileInfo value)
    {
        OpenFileCommand.NotifyCanExecuteChanged();
    }
}
