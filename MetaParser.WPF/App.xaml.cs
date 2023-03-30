using MetaParser.WPF.Services;
using MetaParser.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace MetaParser.WPF;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly ServiceProvider serviceProvider;

    public App()
    {
        var sc = new ServiceCollection();

        ConfigureServices(sc);

        serviceProvider = sc.BuildServiceProvider();
    }

    private void ConfigureServices(ServiceCollection sc)
    {
        sc.AddSingleton<FileSystemService>();
        sc.AddSingleton<DialogService>();
        sc.AddSingleton<ClipboardService>();
        sc.AddSingleton<ActionViewModelFactory>();
        sc.AddSingleton<ConditionViewModelFactory>();
        sc.AddSingleton<MainViewModel>();
        sc.AddSingleton<MainWindow>();
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        if (e.Args != null && e.Args.Length > 0)
        {
            Properties["InitialFile"] = e.Args[0];
        }

        var window = serviceProvider.GetService<MainWindow>();
        window.Show();
    }
}
