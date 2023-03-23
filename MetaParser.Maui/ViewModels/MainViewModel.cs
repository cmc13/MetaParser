using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MetaParser.Maui.ViewModels;

public partial class MainViewModel
    : ObservableObject
{
    private string fileName = null;
    private readonly IFileSaver fileSaver;

    public MainViewModel(IFileSaver fileSaver)
    {
        this.fileSaver = fileSaver;
    }

    [RelayCommand]
    public void Exit() => Application.Current.Quit();

    [RelayCommand]
    public async Task NewFile()
    {
        if (MetaViewModel.IsDirty)
        {
            var result = await Shell.Current.CurrentPage.DisplayAlert("Unsaved Changes", $"{FileNameDisplay} has unsaved changes. Would you like to save before closing?", "Yes", "No");
            if (!result)
            {
                return;
            }
        }

        MetaViewModel = new();
        FileName = null;
    }

    [RelayCommand]
    public async Task OpenFile()
    {
        if (MetaViewModel.IsDirty)
        {
            var confirm = await Shell.Current.CurrentPage.DisplayAlert("Unsaved Changes", $"{FileNameDisplay} has unsaved changes. Would you like to save before closing?", "Yes", "No");
            if (!confirm)
            {
                return;
            }
        }

        var result = await FilePicker.Default.PickAsync(new()
        {
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>()
            {
                { DevicePlatform.WinUI, new[] { ".met", ".xml", ".af"} }
            }),
            PickerTitle = "Open Meta File…"
        });

        if (result != null)
        {
            FileName = result.FullPath;
        }
    }

    [RelayCommand]
    public async Task SaveFile()
    {
        if (string.IsNullOrEmpty(FileName) || Path.GetExtension(FileName).ToLower() != ".met")
        {
            await SaveFileAs();
        }
        else
        {
        }
    }

    [RelayCommand]
    public async Task SaveFileAs()
    {
        var fileName = FileName ?? @"C:\test.met";
        using var ms = new MemoryStream();
        var result = await fileSaver.SaveAsync(Path.GetDirectoryName(fileName), Path.GetFileName(fileName), ms, CancellationToken.None);

    }

    public string FileName
    {
        get => fileName;
        set
        {
            if (fileName != value)
            {
                fileName = value;
                OnPropertyChanged(nameof(FileName));
                OnPropertyChanged(nameof(FileNameDisplay));
            }
        }
    }

    public string FileNameDisplay => !string.IsNullOrEmpty(FileName) ? Path.GetFileName(FileName) : "[New File]";

    public MetaViewModel MetaViewModel { get; set; } = new();
}
