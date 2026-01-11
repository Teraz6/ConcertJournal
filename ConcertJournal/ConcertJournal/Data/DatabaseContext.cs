using SQLite;
using ConcertJournal.Models;

namespace ConcertJournal.Data
{
    public class DatabaseContext
    {
        private readonly SQLiteAsyncConnection _database;
        private bool _isInitialized;

        public DatabaseContext(string dbPath)
        {
            var options = SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache;
            _database = new SQLiteAsyncConnection(dbPath, options);
        }

        // This is the "Magic" method that ensures the table is ready
        private async Task InitAsync()
        {
            if (_isInitialized) return;

            await _database.CreateTableAsync<Concert>();
            _isInitialized = true;
        }

        // --- CRUD OPERATIONS (Now with safety checks) ---

        public async Task<List<Concert>> GetAllConcertsAsync()
        {
            await InitAsync(); // Ensure DB is ready before querying
            return await _database.Table<Concert>().ToListAsync();
        }

        public async Task<Concert> GetConcertByIdAsync(int id)
        {
            await InitAsync();
            return await _database.Table<Concert>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveConcertAsync(Concert concert)
        {
            await InitAsync();
            return concert.Id != 0
                ? await _database.UpdateAsync(concert)
                : await _database.InsertAsync(concert);
        }

        public async Task<int> DeleteConcertAsync(Concert concert)
        {
            await InitAsync();
            return await _database.DeleteAsync(concert);
        }

        // --- PAGING (Now with safety checks) ---

        public async Task<List<Concert>> GetConcertsPagedAsync(int skip, int take, string sortBy = "Default", string searchText = "")
        {
            await InitAsync();

            AsyncTableQuery<Concert> query = _database.Table<Concert>();

            // 1. Filter
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                // Convert the search term to lowercase once
                string lowerSearch = searchText.ToLower().Trim();

                // Use ToLower() on the fields to ensure a case-insensitive match
                query = query.Where(c =>
                    c.EventTitle!.ToLower().Contains(lowerSearch) ||
                    c.Performers!.ToLower().Contains(lowerSearch) ||
                    c.Country!.ToLower().Contains(lowerSearch) ||
                    c.City!.ToLower().Contains(lowerSearch));
            }

            // 2. Sort - Map the RadioButton "Value" to the SQL Logic
            query = sortBy switch
            {
                "LatestAdded" => query.OrderByDescending(c => c.Id),
                "NewestByDate" => query.OrderByDescending(c => c.Date),
                "OldestByDate" => query.OrderBy(c => c.Date),
                "Title" => query.OrderBy(c => c.EventTitle),
                _ => query.OrderByDescending(c => c.Id) //fallback
            };

            // 3. Paging
            return await query.Skip(skip).Take(take).ToListAsync();
        }
    }
}
