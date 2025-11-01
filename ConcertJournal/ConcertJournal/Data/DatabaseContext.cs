using SQLite;
using ConcertJournal.Models;

namespace ConcertJournal.Data
{
    public class DatabaseContext
    {
        readonly SQLiteAsyncConnection _database;
        public DatabaseContext(string dbPath)
        {
            _database = new SQLiteAsyncConnection(dbPath);
            _database.CreateTableAsync<Concert>().Wait();
        }

        public Task<List<Concert>> GetConcertsAsync()
        {
            return _database.Table<Concert>().ToListAsync();
        }

        public Task<int> SaveConcertAsync(Concert concert)
        {
            if (concert.Id != 0)
            {
                return _database.UpdateAsync(concert);
            }
            else
            {
                return _database.InsertAsync(concert);
            }
        }
        public Task<int> DeleteConcertAsync(Concert concert)
        {
            return _database.DeleteAsync(concert);
        }

        public async Task<List<Concert>> GetConcertsPagedAsync(int skip, int take)
        {
            try
            {
                // Always include OrderBy before Skip/Take
                return await _database.Table<Concert>()
                                      .OrderByDescending(c => c.Id) // or whatever sort order you use
                                      .Skip(skip)
                                      .Take(take)
                                      .ToListAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetConcertsPagedAsync: {ex.Message}");
                return new List<Concert>();
            }
        }
    }
}
