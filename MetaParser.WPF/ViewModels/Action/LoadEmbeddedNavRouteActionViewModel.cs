﻿using MetaParser.Formatting;
using MetaParser.Models;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using System;
using System.IO;

namespace MetaParser.WPF.ViewModels
{
    public class LoadEmbeddedNavRouteActionViewModel : ActionViewModel
    {
        private NavRouteViewModel nav;
        private string name;
        private readonly INavReader navReader = Formatters.NavReader;

        public LoadEmbeddedNavRouteActionViewModel(EmbeddedNavRouteMetaAction action, MetaViewModel meta) : base(action, meta)
        {
            name = action.Data.name;

            if (action.Data.nav == null)
                action.Data = (action.Data.name, new NavRoute() { Type = NavType.Circular });
            nav = new NavRouteViewModel(action.Data.nav);
            nav.PropertyChanged += Nav_PropertyChanged;

            LoadNavCommand = new(async () =>
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
                    var nav = new NavRoute();
                    using var fs = File.OpenRead(ofd.FileName);
                    using var reader = new StreamReader(fs);
                    if (Path.GetExtension(ofd.FileName).ToLower() == ".af")
                        await new MetafNavReader().ReadNavAsync(reader, nav);
                    else
                        await navReader.ReadNavAsync(reader, nav).ConfigureAwait(false);

                    ((EmbeddedNavRouteMetaAction)Action).Data = (Path.GetFileName(ofd.FileName), nav);
                    Nav = new NavRouteViewModel(nav);
                }
            });
        }

        public override string Display => $"{base.Display} ({Nav.Indicator})";

        public AsyncRelayCommand LoadNavCommand { get; }

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

        private void Nav_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NavRouteViewModel.IsDirty))
            {
                OnPropertyChanged(nameof(IsDirty));
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
    }
}
