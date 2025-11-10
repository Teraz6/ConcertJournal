using CommunityToolkit.Maui;
using ConcertJournal.Data;
using ConcertJournal.Services;
using CustomShellMaui;
using Microsoft.Extensions.Logging;
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
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("OpenSans-Italic.ttf", "OpenSansItalic");
            });

        builder.ConfigureMauiHandlers(handlers => { });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<DatabaseContext>(s =>
            new DatabaseContext(DatabaseHelper.GetDatabasePath()));
        return builder.Build();
	}
}
