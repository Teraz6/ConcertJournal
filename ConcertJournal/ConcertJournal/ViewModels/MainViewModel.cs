using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConcertJournal.Services;

namespace ConcertJournal.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly UpdateServices _updateService;

    [ObservableProperty] private string _versionText = $"Current version: {AppInfo.VersionString}";
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
    public async Task CheckForUpdatesAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            // The service handles the JSON, Version check, and Toast
            await _updateService.CheckForUpdateAsync();
        }
        finally
        {
            IsBusy = false;
        }
    }
}