using System;
using System.Windows;

namespace MetaParser.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        if (e.Args != null && e.Args.Length > 0)
        {
            Properties["InitialFile"] = e.Args[0];
        }

        base.OnStartup(e);
    }
}
