using ConcertJournal.Models;
using ConcertJournal.ServiceInterface;
using CommunityToolkit.Mvvm.Messaging;
using ConcertJournal.Messages;
using ConcertJournal.Data;

namespace ConcertJournal.Services
{
    public class ConcertServices : IConcertService
    {
        private readonly DatabaseContext _db;

        // We inject the database through the constructor
        public ConcertServices(DatabaseContext db)
        {
            _db = db;
        }

        // CREATE and UPDATE (The "Save" part of CRUD)
        public async Task<int> SaveConcertAsync(Concert concert)
        {
            // Professional touch: Add business logic here
            if (string.IsNullOrWhiteSpace(concert.EventTitle))
                throw new Exception("A concert must have a title!");

            return await _db.SaveConcertAsync(concert);
        }

        // DELETE (The "Delete" part of CRUD)
        public async Task<int> DeleteConcertAsync(Concert concert)
        {
            return await _db.DeleteConcertAsync(concert);
        }

        // READ (The "Read" part of CRUD)
        public async Task<List<Concert>> GetConcertsPagedAsync(int skip, int take, string sortBy = "Default", string searchText = "")
        {
            string cleanSearch = (searchText ?? "").Trim();
            return await _db.GetConcertsPagedAsync(skip, take, sortBy, true, cleanSearch);
        }

        // READ SINGLE
        public async Task<Concert> GetConcertByIdAsync(int id)
        {
            // If you added this method to your DatabaseContext, call it here
            return await _db.GetConcertByIdAsync(id);
        }

        public List<string> ConvertStringToList(string? input, char separator)
        {
            if (string.IsNullOrWhiteSpace(input)) return new List<string>();

            return input.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim())
                        .ToList();
        }

        public string ConvertListToString(IEnumerable<string> items, string separator)
        {
            if (items == null || !items.Any()) return string.Empty;
            return string.Join(separator, items);
        }
    }
}