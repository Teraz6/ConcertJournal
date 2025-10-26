using ClosedXML.Excel;
using ConcertJournal.Models;

namespace ConcertJournal.Services
{
    public static class ImportServices
    {
        public static event Action? ConcertsImported;
        public static async Task ImportConcertsFromExcelAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Excel file not found", filePath);

            using var workbook = new XLWorkbook(filePath);
            var worksheet = workbook.Worksheet(1); // Assuming first sheet

            var concerts = new List<Concert>();

            // Skip header row (start at row 2)
            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                try
                {
                    var concert = new Concert
                    {
                        EventTitle = row.Cell(1).GetString(),
                        Performers = row.Cell(2).GetString(),
                        Venue = row.Cell(3).GetString(),
                        Country = row.Cell(4).GetString(),
                        City = row.Cell(5).GetString(),
                        Date = DateTime.TryParse(row.Cell(6).GetString(), out var date) ? date : DateTime.Today,
                        Rating = row.Cell(7).GetDouble(),
                        Notes = row.Cell(8).GetString(),
                        MediaPaths = row.Cell(9).GetString()
                    };

                    concerts.Add(concert);
                }
                catch
                {
                    // Ignore malformed rows
                }
            }

            // Save to SQLite database
            foreach (var concert in concerts)
            {
                await App.Database.SaveConcertAsync(concert);
            }

            ConcertsImported?.Invoke();
        }
    }
}
