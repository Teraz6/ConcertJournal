using SQLite;
using ConcertJournal.Models;

namespace ConcertJournal.Data
{
    public class DatabaseContext
    {
        private readonly SQLiteAsyncConnection _database;

        public DatabaseContext(string dbPath)
        {
            // Professional practice: Use 'ReadWrite' and 'Create' flags to ensure the DB file behaves correctly
            var options = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;

            _database = new SQLiteAsyncConnection(dbPath, options);

            // We use .Wait() here in the constructor just to ensure the table exists before any calls happen.
            // In a very large app, we would move this to an 'InitializeAsync' method.
            _database.CreateTableAsync<Concert>().Wait();
        }

        // --- CRUD OPERATIONS ---

        public Task<List<Concert>> GetAllConcertsAsync()
        {
            return _database.Table<Concert>().ToListAsync();
        }

        public Task<Concert> GetConcertByIdAsync(int id)
        {
            return _database.Table<Concert>().Where(i => i.Id == id).FirstOrDefaultAsync();
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

        // --- ADVANCED FILTERING & PAGING ---

        public Task<List<Concert>> GetConcertsPagedAsync(int skip, int take, string sortBy = "Default", bool ascending = true, string searchText = "")
        {
            AsyncTableQuery<Concert> query = _database.Table<Concert>();

            // 1. Apply search filter (Case-insensitive)
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                // Note: SQLite's 'Contains' is already case-insensitive for standard characters
                query = query.Where(c =>
                    c.EventTitle.Contains(searchText) ||
                    c.Performers.Contains(searchText) ||
                    c.Country.Contains(searchText) ||
                    c.City.Contains(searchText));
            }

            // 2. Sorting Logic (Fixed the logic here)
            query = sortBy switch
            {
                "Date" => ascending
                    ? query.OrderBy(c => c.Date)
                    : query.OrderByDescending(c => c.Date),

                "Title" => ascending
                    ? query.OrderBy(c => c.EventTitle)
                    : query.OrderByDescending(c => c.EventTitle),

                _ => query.OrderByDescending(c => c.Id) // Default: Most recently added
            };

            // 3. Apply paging
            return query.Skip(skip).Take(take).ToListAsync();
        }
    }
}
