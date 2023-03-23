using CommunityToolkit.Mvvm.ComponentModel;

namespace MetaParser.Maui.ViewModels;

public partial class MetaViewModel
    : ObservableObject
{
    public bool IsDirty { get; }
}
