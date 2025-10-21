using ConcertJournal.Resources.Themes;
using ConcertJournal.Services;
using Microsoft.Maui.Storage;

namespace ConcertJournal.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();

        // Load last theme state from Preferences
        bool isDevil = Preferences.Get("AppTheme", "Angel") == "Devil";
        ThemeSwitch.IsToggled = isDevil;
        ApplyTheme(isDevil);
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

        bool isExcel = await DisplayAlert("Export Format", "Choose export format:", "Excel (.xlsx)", "CSV (.csv)");

        if (isExcel)
            await ExportServices.ExportConcertsToExcelAsync(concerts, this);
        else
            await ExportServices.ExportConcertsToCsvAsync(concerts, this);
    }
}
