using ConcertJournal.Resources.Themes;
using ConcertJournal.Services;

namespace ConcertJournal.Views;

public partial class SettingsPage : ContentPage
{

    private const string DefaultCountryKey = "DefaultCountry";
    private const string DefaultCityKey = "DefaultCity";
    public SettingsPage()
    {
        InitializeComponent();

        // Load last theme state from Preferences
        bool isDevil = Preferences.Get("AppTheme", "Angel") == "Devil";
        ThemeSwitch.IsToggled = isDevil;
        ApplyTheme(isDevil);

        string savedCountry = Preferences.Get(DefaultCountryKey, string.Empty);
        CountryEntry.Text = savedCountry;

        string savedCity = Preferences.Get(DefaultCityKey, string.Empty);
        CityEntry.Text = savedCity;
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        string country = CountryEntry.Text?.Trim() ?? string.Empty;
        string city = CityEntry.Text?.Trim() ?? string.Empty;

        // Save to preferences
        Preferences.Set(DefaultCountryKey, country);
        Preferences.Set(DefaultCityKey, city);

        DisplayAlert("Values set", $"Country: {country}\nCity: {city}", "OK");
    }

    private void OnThemeToggled(object sender, ToggledEventArgs e)
    {
        bool isDevil = e.Value;
        ApplyTheme(isDevil);
        Preferences.Set("AppTheme", isDevil ? "Devil" : "Angel");
    }

    private void ApplyTheme(bool isDevil)
    {
        App.Current.Resources.MergedDictionaries.Clear();

        if (isDevil)
            App.Current.Resources.MergedDictionaries.Add(new DevilTheme());
        else
            App.Current.Resources.MergedDictionaries.Add(new AngelTheme());
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        var concerts = await App.Database.GetConcertsAsync();

        if (concerts.Count == 0)
        {
            await DisplayAlert("No Data", "You have no concerts to export.", "OK");
            return;
        }
        
        await ExportServices.ExportConcertsToExcelAsync(concerts, this);
    }


    //Database Import
    private async void OnImportFromExcelClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Select Excel file to import",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
                { DevicePlatform.WinUI, new[] { ".xlsx" } },
                { DevicePlatform.iOS, new[] { "public.xlsx" } }
            })
            });

            if (result == null)
                return;

            // Create a temporary file in app's private storage
            using var stream = await result.OpenReadAsync();
            await ImportServices.ImportConcertsFromExcelAsync(stream);

            await DisplayAlert("Success", "Concerts imported successfully!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to import Excel file: {ex.Message}", "OK");
        }

    }

}
