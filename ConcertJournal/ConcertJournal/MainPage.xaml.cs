using System.Text.Json;

namespace ConcertJournal;

public partial class MainPage : ContentPage
{
    private string? _downloadUrl;

    public MainPage()
    {
        InitializeComponent();
        CheckForUpdate();
    }

    private async void CheckForUpdate()
    {
        VersionLabel.Text = $"Current version: {AppInfo.VersionString}";

        try
        {
            using var client = new HttpClient();
            string json = await client.GetStringAsync("https://raw.githubusercontent.com/Teraz6/ConcertJournal/main/update.json");

            var info = JsonSerializer.Deserialize<UpdateInfo>(json) ?? throw new Exception("Invalid update.json format");
            _downloadUrl = info.DownloadUrl;

            var currentVersion = Version.Parse(AppInfo.VersionString);
            var latestVersion = Version.Parse(info.Version);

            if (currentVersion >= latestVersion)
            {
                // Latest version
                UpdateCard.Stroke = Colors.Green;
                UpdateMessageLabel.Text = "You are using the latest version!";
                UpdateButton.IsVisible = false;
            }
            else
            {
                // Needs update
                UpdateCard.Stroke = Colors.Yellow;
                UpdateMessageLabel.Text = $"A new version {latestVersion} is available!";
                UpdateButton.IsVisible = true;
            }
        }
        catch (Exception)
        {
            UpdateCard.Stroke = Colors.Gray;
            UpdateMessageLabel.Text = "Unable to check for updates.";
            UpdateButton.IsVisible = false;
        }
    }

    private async void UpdateButton_Clicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Update", "This will download the latest version.", "Yes", "No");

        if (!confirm)
            return;

        if (!string.IsNullOrEmpty(_downloadUrl))
        {
            await Launcher.OpenAsync(_downloadUrl);
        }
    }

    //Json data structure
    private class UpdateInfo
    {
        public required string Version { get; set; }
        public required string DownloadUrl { get; set; }
    }
}

