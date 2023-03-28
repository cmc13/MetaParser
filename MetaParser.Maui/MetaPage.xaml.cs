using MetaParser.Maui.ViewModels;

namespace MetaParser.Maui;

public partial class MetaPage : ContentPage
{
	public MetaPage(MetaViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}