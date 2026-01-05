using ConcertJournal.Models;
using System.Text.Json;

namespace ConcertJournal.Services;

public class UpdateServices
{
    private const string UpdateUrl = "https://raw.githubusercontent.com/Teraz6/ConcertJournal/refs/heads/feature-update-notification/update.json";

    public async Task CheckForUpdateAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5); // Don't hang the app if GitHub is slow
            var json = await client.GetStringAsync(UpdateUrl);

            var info = JsonSerializer.Deserialize<UpdateInfo>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (info == null) return;

            var current = AppInfo.VersionString;

            // Use Version.Parse safely
            if (Version.Parse(info.Version) > Version.Parse(current))
            {
                // Professional way: Ask before jumping to browser
                await ShowUpdateDialog(info.DownloadUrl, info.Version);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Update check failed: {ex.Message}");
        }
    }

    private async Task ShowUpdateDialog(string url, string newVersion)
    {
        // Toasts are small; for a version update, a Dialog is more "Professional"
        bool download = await Shell.Current.DisplayAlert(
            "Update Available",
            $"Version {newVersion} is ready! Would you like to download it now?",
            "Download",
            "Later");

        if (download)
        {
            await Launcher.OpenAsync(url);
        }
    }
}