using CommunityToolkit.Mvvm.Input;
using MetaParser.Formatting;
using MetaParser.Models;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MetaParser.WPF.ViewModels
{
    public partial class LoadEmbeddedNavRouteActionViewModel : ActionViewModel
    {
        private NavRouteViewModel nav;
        private string name;
        private readonly INavReader navReader = Formatters.NavReader;
        private readonly INavReader metafNavReader = Formatters.MetafNavReader;

        public LoadEmbeddedNavRouteActionViewModel(EmbeddedNavRouteMetaAction action, MetaViewModel meta) : base(action, meta)
        {
            name = action.Data.name;

            if (action.Data.nav == null)
                action.Data = (action.Data.name, new NavRoute() { Type = NavType.Circular });
            nav = new NavRouteViewModel(action.Data.nav);
            nav.PropertyChanged += Nav_PropertyChanged;
        }

        [RelayCommand]
        async Task LoadNav()
        {
            var ofd = new OpenFileDialog()
            {
                InitialDirectory = Directory.Exists(@"C:\Games\VirindiPlugins\VirindiTank\") ? @"C:\Games\VirindiPlugins\VirindiTank\" : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false,
                Filter = "Nav Route Files (*.nav)|*.nav|Metaf Nav Files (*.af)|*.af"
            };

            var result = ofd.ShowDialog();
            if (result.HasValue && result.Value == true)
            {
                using var fs = File.OpenRead(ofd.FileName);
                using var reader = new StreamReader(fs);
                var nav = await (Path.GetExtension(ofd.FileName).ToLower() switch
                {
                    ".af" => metafNavReader.ReadNavAsync(reader).ConfigureAwait(false),
                    _ => navReader.ReadNavAsync(reader).ConfigureAwait(false)
                });

                ((EmbeddedNavRouteMetaAction)Action).Data = (Path.GetFileName(ofd.FileName), nav);
                Nav = new NavRouteViewModel(nav);
            }
        }

        public override string Display => $"{base.Display} ({Nav.Indicator})";

        public NavRouteViewModel Nav
        {
            get => nav;
            set
            {
                if (nav != value)
                {
                    if (nav != null)
                        nav.PropertyChanged -= Nav_PropertyChanged;
                    nav = value;
                    OnPropertyChanged(nameof(Nav));
                    OnPropertyChanged(nameof(Display));
                    IsDirty = true;
                    nav.PropertyChanged += Nav_PropertyChanged;
                }
            }
        }

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                    IsDirty = true;
                }
            }
        }

        public override bool IsDirty
        {
            get => base.IsDirty || Nav.IsDirty;
            set => base.IsDirty = value;
        }

        public override void Clean()
        {
            base.Clean();
            if (Nav.IsDirty)
                Nav.Clean();
        }

        private void Nav_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NavRouteViewModel.IsDirty))
            {
                OnPropertyChanged(nameof(IsDirty));
            }
        }
    }
}
