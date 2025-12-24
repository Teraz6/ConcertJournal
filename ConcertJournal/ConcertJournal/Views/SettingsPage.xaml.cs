using ConcertJournal.Resources.Themes;
using ConcertJournal.Services;

namespace ConcertJournal.Views;

public partial class SettingsPage : ContentPage
{

    private const string DefaultCountryKey = "DefaultCountry";
    private const string DefaultCityKey = "DefaultCity";

    // Bindable property for theme
    private bool _isDevilTheme;
    public bool IsDevilTheme
    {
        get => _isDevilTheme;
        set
        {
            if (_isDevilTheme != value)
            {
                _isDevilTheme = value;
                OnPropertyChanged(nameof(IsDevilTheme));
                ApplyTheme(_isDevilTheme);
                Preferences.Set("AppTheme", _isDevilTheme ? "Devil" : "Angel");
            }
        }
    }

    private string? _defaultCountry;
    public string? DefaultCountry
    {
        get => _defaultCountry;
        set
        {
            if (_defaultCountry != value)
            {
                _defaultCountry = value;
                OnPropertyChanged(nameof(DefaultCountry));
            }
        }
    }

    private string? _defaultCity;
    public string? DefaultCity
    {
        get => _defaultCity;
        set
        {
            if (_defaultCity != value)
            {
                _defaultCity = value;
                OnPropertyChanged(nameof(DefaultCity));
            }
        }
    }

    public SettingsPage()
    {
        InitializeComponent();

        BindingContext = this;
        // Load last theme state from Preferences
        IsDevilTheme = Preferences.Get("AppTheme", "Angel") == "Devil";

        string savedCountry = Preferences.Get(DefaultCountryKey, string.Empty);
        string savedCity = Preferences.Get(DefaultCityKey, string.Empty);
    }

    private void OnSaveClicked(object sender, EventArgs e)
    {
        Preferences.Set(DefaultCountryKey, DefaultCountry?.Trim() ?? string.Empty);
        Preferences.Set(DefaultCityKey, DefaultCity?.Trim() ?? string.Empty);

        DisplayAlert("Values set", $"Country: {DefaultCountry}\nCity: {DefaultCity}", "OK");
    }

    private void ApplyTheme(bool isDevil)
    {
        App.Current?.Resources.MergedDictionaries.Clear();

        if (isDevil)
            App.Current?.Resources.MergedDictionaries.Add(new DevilTheme());
        else
            App.Current?.Resources.MergedDictionaries.Add(new AngelTheme());
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
                { DevicePlatform.WinUI, new[] { ".xlsx", ".xlsm" } },
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
