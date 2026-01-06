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

        public async Task<List<Concert>> GetConcertsPagedAsync(int skip, int take, string sortBy = "Default", bool ascending = true, string searchText = "")
        {
            await InitAsync();

            AsyncTableQuery<Concert> query = _database.Table<Concert>();

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = query.Where(c =>
                    c.EventTitle.Contains(searchText) ||
                    c.Performers.Contains(searchText) ||
                    c.Country.Contains(searchText) ||
                    c.City.Contains(searchText));
            }

            query = sortBy switch
            {
                "Date" => ascending ? query.OrderBy(c => c.Date) : query.OrderByDescending(c => c.Date),
                "Title" => ascending ? query.OrderBy(c => c.EventTitle) : query.OrderByDescending(c => c.EventTitle),
                _ => query.OrderByDescending(c => c.Id)
            };

            return await query.Skip(skip).Take(take).ToListAsync();
        }
    }
}
