using ConcertJournal.Resources.Themes;
using ConcertJournal.Services;

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

    //Database Export
    private async void OnExportDatabaseClicked(object sender, EventArgs e)
    {
        try
        {
            var dbPath = DatabaseHelper.GetDatabasePath();

            if (!File.Exists(dbPath))
            {
                await DisplayAlert("Error", "No database file found to export.", "OK");
                return;
            }

            // Let user choose where to save
            var fileName = $"ConcertJournalBackup_{DateTime.Now:yyyyMMdd_HHmm}.db3";
            var destPath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);

#if ANDROID
            // Android: copy to Downloads folder
            var downloads = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
            if (downloads != null)
            {
                destPath = Path.Combine(downloads, fileName);
            }
#endif

            File.Copy(dbPath, destPath, overwrite: true);
            await DisplayAlert("Success", $"Database exported to:\n{destPath}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to export database: {ex.Message}", "OK");
        }
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
                { DevicePlatform.Android, new[] { ".xlsx" } },
                { DevicePlatform.WinUI, new[] { ".xlsx" } },
                { DevicePlatform.iOS, new[] { ".xlsx" } }
            })
            });

            if (result == null)
                return;

            await ImportServices.ImportConcertsFromExcelAsync(result.FullPath);
            
            await DisplayAlert("Success", "Concerts imported successfully!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to import Excel file: {ex.Message}", "OK");
        }

    }
}
