using System.Text;
using ClosedXML.Excel;
using ConcertJournal.Models;

#if WINDOWS
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
#endif

namespace ConcertJournal.Services
{
    public class ExportServices
    {
        // ===================== CSV EXPORT =====================
        public static async Task ExportConcertsToCsvAsync(List<Concert> concerts, Page page, string defaultFileName = "Concerts.csv")
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("EventTitle,Performers,Venue,Country,City,Date,Notes");

            // Data
            foreach (var c in concerts)
            {
                var dateString = c.Date?.ToString("yyyy-MM-dd") ?? "";
                sb.AppendLine($"\"{c.EventTitle}\",\"{c.Performers}\",\"{c.Venue}\",\"{c.Country}\",\"{c.City}\",\"{dateString}\",\"{c.Notes}\"");
            }

            var csvBytes = Encoding.UTF8.GetBytes(sb.ToString());

#if WINDOWS
            // Ask user where to save file
            var picker = new FileSavePicker();
            var window = (Microsoft.Maui.Controls.Application.Current?.Windows?.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window);
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.SuggestedFileName = defaultFileName;
            picker.FileTypeChoices.Add("CSV File", new List<string> { ".csv" });

            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                await FileIO.WriteBytesAsync(file, csvBytes);
                await page.DisplayAlert("Export Complete", $"Saved to: {file.Path}", "OK");
            }
#else
            // Mobile: save to temp and share
            string path = Path.Combine(FileSystem.CacheDirectory, defaultFileName);
            await File.WriteAllBytesAsync(path, csvBytes);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Export Concerts (CSV)",
                File = new ShareFile(path)
            });
#endif
        }


        // ===================== EXCEL EXPORT =====================
        public static async Task ExportConcertsToExcelAsync(List<Concert> concerts, Page page, string defaultFileName = "Concerts.xlsx")
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Concerts");

            // Header
            ws.Cell(1, 1).Value = "EventTitle";
            ws.Cell(1, 2).Value = "Performers";
            ws.Cell(1, 3).Value = "Venue";
            ws.Cell(1, 4).Value = "Country";
            ws.Cell(1, 5).Value = "City";
            ws.Cell(1, 6).Value = "Date";
            ws.Cell(1, 7).Value = "Notes";
            ws.Cell(1, 8).Value = "MediaPaths";

            // Data
            for (int i = 0; i < concerts.Count; i++)
            {
                var c = concerts[i];
                ws.Cell(i + 2, 1).Value = c.EventTitle;
                ws.Cell(i + 2, 2).Value = c.Performers;
                ws.Cell(i + 2, 3).Value = c.Venue;
                ws.Cell(i + 2, 4).Value = c.Country;
                ws.Cell(i + 2, 5).Value = c.City;
                ws.Cell(i + 2, 6).Value = c.Date?.ToString("yyyy-MM-dd");
                ws.Cell(i + 2, 7).Value = c.Notes;
                ws.Cell(i + 2, 8).Value = c.MediaPaths;
            }

            // Auto-fit columns
            ws.Columns().AdjustToContents();

#if WINDOWS
            var picker = new FileSavePicker();
            var window = (Microsoft.Maui.Controls.Application.Current?.Windows?.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window);
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.SuggestedFileName = defaultFileName;
            picker.FileTypeChoices.Add("Excel Workbook", new List<string> { ".xlsx" });

            StorageFile file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                using var stream = await file.OpenStreamForWriteAsync();
                workbook.SaveAs(stream);
                await page.DisplayAlert("Export Complete", $"Saved to: {file.Path}", "OK");
            }
#else
            // Mobile: write to cache and share
            string path = Path.Combine(FileSystem.CacheDirectory, defaultFileName);
            workbook.SaveAs(path);

            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "Export Concerts (Excel)",
                File = new ShareFile(path)
            });
#endif
        }
    }
}
