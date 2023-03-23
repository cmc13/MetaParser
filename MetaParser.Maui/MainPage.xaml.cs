using MetaParser.Maui.ViewModels;

namespace MetaParser.Maui;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}