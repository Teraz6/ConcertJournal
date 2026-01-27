using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConcertJournal.Resources.Themes;
using ConcertJournal.ServiceInterface;
using ConcertJournal.Services;

namespace ConcertJournal.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IConcertService _concertService;
    private readonly ImportServices _importService; // The "instance" of the service

    private const string DefaultCountryKey = "DefaultCountry";
    private const string DefaultCityKey = "DefaultCity";
    private const string AppThemeKey = "AppTheme";

    [ObservableProperty] public partial bool IsDevilTheme { get; set; }
    [ObservableProperty] public partial string DefaultCountry { get; set; }
    [ObservableProperty] public partial string DefaultCity {  get; set; }
    public SettingsViewModel(IConcertService concertService, ImportServices importService)
    {
        _concertService = concertService;

        // Load Initial States
        IsDevilTheme = Preferences.Get(AppThemeKey, "Angel") == "Devil";
        DefaultCountry = Preferences.Get(DefaultCountryKey, string.Empty);
        DefaultCity = Preferences.Get(DefaultCityKey, string.Empty);
        _importService = importService;
    }

    // Automatically reacts when the Switch is toggled in UI
    partial void OnIsDevilThemeChanged(bool value)
    {
        Preferences.Set(AppThemeKey, value ? "Devil" : "Angel");
        ApplyTheme(value);
    }

    private void ApplyTheme(bool isDevil)
    {
        App.Current?.Resources.MergedDictionaries.Clear();
        if (isDevil)
            App.Current?.Resources.MergedDictionaries.Add(new DevilTheme());
        else
            App.Current?.Resources.MergedDictionaries.Add(new AngelTheme());
    }

    [RelayCommand]
    private async Task SaveDefaultsAsync()
    {
        Preferences.Set(DefaultCountryKey, DefaultCountry?.Trim() ?? string.Empty);
        Preferences.Set(DefaultCityKey, DefaultCity?.Trim() ?? string.Empty);
        await Shell.Current.DisplayAlertAsync("Success", "Default values saved!", "OK");
    }

    [RelayCommand]
    private async Task ExportDataAsync()
    {
        var concerts = await _concertService.GetConcertsPagedAsync(0, 10000, "Default", "");
        if (!concerts.Any())
        {
            await Shell.Current.DisplayAlertAsync("No Data", "Nothing to export.", "OK");
            return;
        }
        await ExportServices.ExportConcertsToExcelAsync(concerts.ToList(), Shell.Current.CurrentPage);
    }

    [RelayCommand]
    private async Task ImportDataAsync()
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Select Excel file" });
            if (result == null) return;

            using var stream = await result.OpenReadAsync();

            // FIX: Use the private field (_importService) instead of the Class name (ImportServices)
            await _importService.ImportConcertsFromExcelAsync(stream);

            await Shell.Current.DisplayAlertAsync("Success", "Concerts imported!", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", ex.Message, "OK");
        }
    }
}