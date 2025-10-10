namespace ConcertJournal.Services
{
    class DatabaseHelper
    {
        public static string GetDatabasePath()
        {
            var basePath = FileSystem.AppDataDirectory;  // This ensures it works on all platforms
            return Path.Combine(basePath, "ConcertJournal.db3");
        }
    }
}
