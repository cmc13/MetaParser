using CommunityToolkit.Mvvm.ComponentModel;

namespace MetaParser.Maui.ViewModels;

[QueryProperty(nameof(FileName), nameof(FileName))]
public partial class MetaViewModel
    : ObservableObject
{
    public bool IsDirty { get; }

    [ObservableProperty]
    private string fileName;
}
