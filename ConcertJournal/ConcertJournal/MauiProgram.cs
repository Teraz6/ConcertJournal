using Microsoft.Extensions.Logging;
using ConcertJournal.Data;
using CommunityToolkit.Maui;

namespace ConcertJournal;

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


#if DEBUG
        builder.Logging.AddDebug();
#endif

		builder.Services.AddSingleton<DatabaseContext>(s => new DatabaseContext(Path.Combine(FileSystem.AppDataDirectory, "ConcertJournalDb1")));
        return builder.Build();
	}
}
