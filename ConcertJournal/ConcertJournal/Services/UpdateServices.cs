using ConcertJournal.Models;
using System.Text.Json;

namespace ConcertJournal.Services;

public class UpdateServices
{
    private const string UpdateUrl = "https://raw.githubusercontent.com/Teraz6/ConcertJournal/refs/heads/feature-update-notification/update.json";

    // Update the return type from Task to Task<UpdateInfo?>
    public async Task<UpdateInfo?> GetUpdateInfoAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var json = await client.GetStringAsync(UpdateUrl);

            var info = JsonSerializer.Deserialize<UpdateInfo>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return info; // Return the object to the ViewModel
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Update check failed: {ex.Message}");
            return null; // Return null so the ViewModel knows it failed
        }
    }
}