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

        public Task<List<Concert>> GetConcertsPagedAsync(int skip, int take, string sortBy = "Default", bool ascending = true)
        {
            AsyncTableQuery<Concert> query = _database.Table<Concert>();

            // Sorting
            switch (sortBy)
            {
                case "NewestByDate":
                    query = ascending ? query.OrderBy(c => c.Date) : query.OrderByDescending(c => c.Date);
                    break;

                case "OldestByDate":
                    query = ascending ? query.OrderBy(c => c.Date) : query.OrderByDescending(c => c.Date);
                    break;

                default:
                    query = query.OrderByDescending(c => c.Id); // Default: newest inserted first
                    break;
            }

            // Apply paging
            query = query.Skip(skip).Take(take);

            return query.ToListAsync();
        }
    }
}
