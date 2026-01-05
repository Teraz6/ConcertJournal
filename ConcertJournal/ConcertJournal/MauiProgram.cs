using CommunityToolkit.Maui;
using ConcertJournal.Data;
using ConcertJournal.ServiceInterface;
using ConcertJournal.Services;
using ConcertJournal.ViewModels;
using ConcertJournal.Views;
using CustomShellMaui;
using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;
using UraniumUI;

namespace ConcertJournal;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseUraniumUI()
            .UseUraniumUIMaterial()
            .UseMauiCommunityToolkit()
            .UseCustomShellMaui()
            .UseLiveCharts()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("OpenSans-Italic.ttf", "OpenSansItalic");
            });

        builder.ConfigureMauiHandlers(handlers => {});

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<DatabaseContext>(s =>
            new DatabaseContext(DatabaseServices.GetDatabasePath()));

        //Register Services
        builder.Services.AddSingleton<IConcertService, ConcertServices>();
        builder.Services.AddSingleton<IImageService, ImageServices>();
        builder.Services.AddSingleton<UpdateServices>();

        //Register viewmodels
        // Transient = Create a fresh new one every time the page is opened
        builder.Services.AddTransient<AddConcertViewModel>();
        builder.Services.AddTransient<ConcertListViewModel>();
        builder.Services.AddTransient<ConcertDetailsViewModel>();
        builder.Services.AddTransient<PerformerDetailsViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<StatisticsViewModel>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<PerformerViewModel>();

        //Register pages
        builder.Services.AddTransient<AddConcertPage>();
        builder.Services.AddTransient<ConcertListPage>();
        builder.Services.AddTransient<ConcertDetailsPage>();
        builder.Services.AddTransient<PerformerDetailsPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<StatisticsPage>();
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
	}
}
