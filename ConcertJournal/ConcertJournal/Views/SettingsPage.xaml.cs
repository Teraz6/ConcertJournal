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
        //Uncomment below to add csv option 

        //bool isExcel = await DisplayAlert("Export Format", "Choose export format:", "Excel (.xlsx)", "CSV (.csv)");

        //if (isExcel)
            await ExportServices.ExportConcertsToExcelAsync(concerts, this);
        //else
        //    await ExportServices.ExportConcertsToCsvAsync(concerts, this);
    }

    //Database Export
//    private async void OnExportDatabaseClicked(object sender, EventArgs e)
//    {
//        try
//        {
//            var dbPath = DatabaseHelper.GetDatabasePath();

//            if (!File.Exists(dbPath))
//            {
//                await DisplayAlert("Error", "No database file found to export.", "OK");
//                return;
//            }

//            // Let user choose where to save
//            var fileName = $"ConcertJournalBackup_{DateTime.Now:yyyyMMdd_HHmm}.db3";
//            var destPath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);

//#if ANDROID
//            // Android: copy to Downloads folder
//            var downloads = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads)?.AbsolutePath;
//            if (downloads != null)
//            {
//                destPath = Path.Combine(downloads, fileName);
//            }
//#endif

//            File.Copy(dbPath, destPath, overwrite: true);
//            await DisplayAlert("Success", $"Database exported to:\n{destPath}", "OK");
//        }
//        catch (Exception ex)
//        {
//            await DisplayAlert("Error", $"Failed to export database: {ex.Message}", "OK");
//        }
//    }

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

    //private async void OnImportDatabaseClicked(object sender, EventArgs e)
    //{
    //    try
    //    {
    //        // Pick a .db3 file
    //        var result = await FilePicker.PickAsync(new PickOptions
    //        {
    //            PickerTitle = "Select a Concert Journal database file",
    //            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
    //        {
    //            { DevicePlatform.Android, new[] { ".db3" } },
    //            { DevicePlatform.WinUI, new[] { ".db3" } },
    //            { DevicePlatform.iOS, new[] { ".db3" } },
    //        })
    //        });

    //        if (result == null)
    //            return; // user canceled

    //        // Destination path (your app's database location)
    //        var dbPath = DatabaseHelper.GetDatabasePath();

    //        // Copy the picked file to your app's database path
    //        using var stream = await result.OpenReadAsync(); // works for cloud files
    //        using var destStream = File.Create(dbPath);
    //        await stream.CopyToAsync(destStream);

    //        await DisplayAlert("Success", "Database imported successfully! Restart the app to see changes.", "OK");

    //        // Optional: reload data immediately
    //        EventBus.OnConcertCreated();
    //    }
    //    catch (Exception ex)
    //    {
    //        await DisplayAlert("Error", $"Failed to import database: {ex.Message}", "OK");
    //    }
    //}

}
