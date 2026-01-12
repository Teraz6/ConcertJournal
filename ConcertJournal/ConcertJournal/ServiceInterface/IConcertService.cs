using ConcertJournal.Models;

namespace ConcertJournal.ServiceInterface
{
    public interface IConcertService
    {
        // --- CREATE & UPDATE ---
        Task<int> SaveConcertAsync(Concert concert);

        // --- DELETE ---
        Task<int> DeleteConcertAsync(Concert concert);

        // --- READ (Multiple) ---
        Task<List<Concert>> GetConcertsPagedAsync(
            int skip,
            int take,
            string sortBy = "Default",
            string searchText = "");

        // --- READ (Single) ---
        Task<Concert> GetConcertByIdAsync(int id);

        Task<int> GetConcertCountAsync(string? searchTerm = null);

        List<string> ConvertStringToList(string? input, char separator);
        string ConvertListToString(IEnumerable<string> items, string separator);
    }
}
