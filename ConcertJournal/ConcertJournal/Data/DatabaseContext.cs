using SQLite;
using ConcertJournal.Models;

namespace ConcertJournal.Data
{
    public class DatabaseContext
    {
        readonly SQLiteAsyncConnection _database;
        public ConcertJournalDatabase(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Concert>().Wait();
        }

        public Task<List<Concert>> GetConcertsAsync() => _database.Table<Concert>().OrderByDescending(c => c.Date).ToListAsync();

        public Task<int>SaveConcertAsync(Concert concert) => _database.InsertOrReplaceAsync(concert);

        public Task<int> DeleteConcertAsync(Concert concert) => _database.DeleteAsync(concert);
    }
}
