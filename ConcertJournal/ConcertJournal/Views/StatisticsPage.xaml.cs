using ConcertJournal.Data;
using ConcertJournal.Models;

namespace ConcertJournal.Views;

public partial class StatisticsPage : ContentPage
{
    private readonly DatabaseContext _database;

    public StatisticsPage(DatabaseContext database)
    {
        InitializeComponent();
        _database = database;
    }

    // This method runs every time the page appears
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadStatistics();
    }

    private async void LoadStatistics()
    {
        var concerts = await App.Database.GetConcertsAsync();

        if (concerts == null || concerts.Count == 0)
        {
            TotalConcertsLabel.Text = "No concert data available.";
            return;
        }

        // Total concerts
        TotalConcertsLabel.Text = $"Total concerts: {concerts.Count}";

        // Average rating
        var ratedConcerts = concerts.Where(c => c.Rating > 0).ToList();
        var avgRating = ratedConcerts.Any() ? ratedConcerts.Average(c => c.Rating) : 0;
        AverageRatingLabel.Text = $"Average rating: {avgRating:F2}";


        // Most frequent performer
        var performer = concerts
            .Where(c => !string.IsNullOrWhiteSpace(c.Performers))
            .SelectMany(c => c.Performers.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .GroupBy(p => p)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? "N/A";

        MostFrequentPerformerLabel.Text = $"Most frequent performer: {performer}";

        // Concerts by country
        var concertsByCountry = concerts
            .Where(c => !string.IsNullOrWhiteSpace(c.Country))
            .GroupBy(c => c.Country.Trim())
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToList();

        ConcertsByCountryLabel.Text = concertsByCountry.Any()
            ? "Concerts by country:\n" + string.Join("\n", concertsByCountry)
            : "No country data available.";

        // Latest concert date
        var latestConcert = concerts
            .Where(c => c.Date.HasValue)
            .OrderByDescending(c => c.Date)
            .FirstOrDefault();

        LatestConcertLabel.Text = latestConcert != null
            ? $"Latest concert: {latestConcert.EventTitle} on {latestConcert.Date:dd MMM yyyy}"
            : "Latest concert: N/A";
    }
}
