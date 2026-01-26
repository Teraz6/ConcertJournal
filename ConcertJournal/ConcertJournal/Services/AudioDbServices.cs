using System.Diagnostics;
using System.Net.Http.Json;
using static ConcertJournal.Models.AudioDbInfo;

namespace ConcertJournal.Services
{
    public class AudioDbServices
    {
        private readonly HttpClient _httpClient = new();
        private const string BaseUrl = "https://www.theaudiodb.com/api/v1/json/123/search.php?s=";

        public async Task<ArtistInfo?> GetArtistDetailsAsync(string artistName)
        {
            try
            {
                var encodedName = Uri.EscapeDataString(artistName);
                var response = await _httpClient.GetFromJsonAsync<AudioDbResponse>(BaseUrl + encodedName);

                return response?.Artists?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error fetching artist: {ex.Message}");
                return null;
            }
        }
    }
}
