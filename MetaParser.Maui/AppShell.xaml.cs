namespace MetaParser.Maui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MetaPage), typeof(MetaPage));
        }
    }
}