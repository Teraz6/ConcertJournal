namespace ConcertJournal.Services
{
    class DatabaseHelper
    {
        public static string GetDatabasePath()
        {
            return Path.Combine(FileSystem.AppDataDirectory, "ConcertJournal.db3");
        }
    }
}
