using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Storage;
using MetaParser.Maui.ViewModels;
using Microsoft.Extensions.Logging;

namespace MetaParser.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<MetaViewModel>();
            builder.Services.AddTransient<MetaPage>();
            builder.Services.AddSingleton(FileSaver.Default);

#if DEBUG
		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}