using ConcertJournal.Data;
using ConcertJournal.Services;
using Microsoft.Extensions.Logging;

namespace ConcertJournal;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });


#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<DatabaseContext>(s =>
            new DatabaseContext(DatabaseHelper.GetDatabasePath()));
        return builder.Build();
	}
}
