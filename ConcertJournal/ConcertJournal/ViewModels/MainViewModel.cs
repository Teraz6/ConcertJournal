using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConcertJournal.Services;

namespace ConcertJournal.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly UpdateServices _updateService;

    [ObservableProperty] private string _versionText = $"Current version: {AppInfo.VersionString}";

    [ObservableProperty] private Color _statusColor = Colors.Gray;

    [ObservableProperty] private string _updateMessage = string.Empty;

    [ObservableProperty] private bool _canUpdate;

    private string? _downloadUrl;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    // This property provides the inverted value to the UI directly
    public bool IsNotBusy => !IsBusy;

    // Inject the service here
    public MainViewModel(UpdateServices updateService)
    {
        _updateService = updateService;
    }

    [RelayCommand]
    private async Task DownloadUpdateAsync()
    {
        if (!string.IsNullOrEmpty(_downloadUrl))
        {
            await Launcher.OpenAsync(_downloadUrl);
        }
    }

    [RelayCommand]
    public async Task CheckForUpdatesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var info = await _updateService.GetUpdateInfoAsync();

            if (info == null)
            {
                StatusColor = Colors.Gray;
                UpdateMessage = "Unable to check for updates.";
                CanUpdate = false;
                return;
            }

            // Standardize version parsing
            if (Version.TryParse(AppInfo.VersionString, out var current) &&
                Version.TryParse(info.Version, out var latest))
            {
                if (current >= latest)
                {
                    StatusColor = Colors.Green;
                    UpdateMessage = "You are using the latest version!";
                    CanUpdate = false;
                }
                else
                {
                    StatusColor = Colors.Yellow;
                    UpdateMessage = $"New version {info.Version} available!";
                    CanUpdate = true;
                    _downloadUrl = info.DownloadUrl;
                }
            }
        }
        catch (Exception ex)
        {
            StatusColor = Colors.Gray;
            UpdateMessage = "Error comparing versions.";
            System.Diagnostics.Debug.WriteLine(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}