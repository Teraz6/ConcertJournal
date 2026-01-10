namespace ConcertJournal.Services
{
    public class DatabaseServices
    {
        public static string GetDatabasePath()
        {
            return Path.Combine(FileSystem.AppDataDirectory, "concerts.db3");
        }
    }
}
