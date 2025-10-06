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

		//builder.Services.AddSingleton<ConcertJournalDatabase>(s => new ConcertJournalDatabase(Path.Combine(FileSystem.AppDataDirectory, "ConcertJournalDb1");
		return builder.Build();
	}
}
