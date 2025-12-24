using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using System.Text.Json;

namespace ConcertJournal.Services
{
    public class UpdateServices
    {
        private const string UpdateUrl = "https://raw.githubusercontent.com/Teraz6/ConcertJournal/refs/heads/feature-update-notification/update.json";

        public async Task CheckForUpdateAsync()
        {
            try
            {
                using var client = new HttpClient();
                var json = await client.GetStringAsync(UpdateUrl);

                var info = JsonSerializer.Deserialize<UpdateInfo>(json);
                var current = AppInfo.VersionString;

                if (Version.Parse(info!.Version) > Version.Parse(current))
                {
                    await UpdateServices.ShowUpdateNotification(info.DownloadUrl);
                }
            }
            catch
            {
                // optionally log error or ignore if offline
            }
        }

        private static async Task ShowUpdateNotification(string url)
        {
            var toast = Toast.Make("A new version is available! Tap to download.", ToastDuration.Long, 14);
            await toast.Show();
            // You can open browser directly (optional)
            await Launcher.OpenAsync(url);
        }
    }

    public class UpdateInfo
    {
        public required string Version { get; set; }
        public required string DownloadUrl { get; set; }
    }
}

