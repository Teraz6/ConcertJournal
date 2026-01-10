using ClosedXML.Excel;
using ConcertJournal.Models;
using ConcertJournal.Data;

namespace ConcertJournal.Services
{
    public class ImportServices
    {
        private readonly DatabaseContext _database;

        // Inject the database context via constructor
        public ImportServices(DatabaseContext database)
        {
            _database = database;
        }

        public async Task ImportConcertsFromExcelAsync(Stream excelStream)
        {
            // Use Task.Run for Excel processing so the UI doesn't freeze
            await Task.Run(async () =>
            {
                using var workbook = new XLWorkbook(excelStream);
                var worksheet = workbook.Worksheet(1);
                var concerts = new List<Concert>();

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
                    catch { /* Ignore malformed rows */ }
                }

                foreach (var concert in concerts)
                {
                    // Use the injected _database instead of App.Database
                    await _database.SaveConcertAsync(concert);
                }
            });
        }
    }
}