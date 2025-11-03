using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using ConcertJournal.Data;
using ConcertJournal.Models;
using Microsoft.Maui.Controls.Shapes;

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


        // Performers by Count
        var performerCounts = concerts
            .Where(c => !string.IsNullOrWhiteSpace(c.Performers))
            .SelectMany(c => c.Performers.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p))
            .GroupBy(p => p)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToList();

        if (performerCounts.Any())
        {
            MostFrequentPerformerLabel.Text = "Performers by concert count:\n" +
                string.Join("\n", performerCounts.Select(p => $"{p.Name}: {p.Count}"));
        }
        else
        {
            MostFrequentPerformerLabel.Text = "No performer data available.";
        }

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

    // Toggle section visibility methods
    private void OnTotalConcertsTapped(object sender, EventArgs e)
    {
        //Toogle visibility
        TotalConcertsContent.IsVisible = !TotalConcertsContent.IsVisible;

        //Update header text based on state
        if (TotalConcertsContent.IsVisible)
        {
            TotalConcertsHeader.Text = "Total Concerts ↓";
        }
        else
        {
            TotalConcertsHeader.Text = "Total Concerts →";
        }
    }

    // Average Rating Toggle
    private void OnAverageRatingTapped(object sender, EventArgs e)
    {
        //Toggle visibility
        AverageRatingContent.IsVisible = !AverageRatingContent.IsVisible;

        //Update header text based on state
        if (AverageRatingContent.IsVisible)
        {
            AverageRatingHeader.Text = "Average Rating ↓";
        }
        else
        {
            AverageRatingHeader.Text = "Average Rating →";
        }
    }

    // Most Frequent Performer Toggle
    private void OnPerformerTapped(object sender, EventArgs e)
    {
        //Toggle visibility
        MostFrequentPerformerContent.IsVisible = !MostFrequentPerformerContent.IsVisible;

        //Update header text based on state
        if (MostFrequentPerformerContent.IsVisible)
        {
            MostFrequentPerformerHeader.Text = "Performers by Count ↓";
        }
        else
        {
            MostFrequentPerformerHeader.Text = "Performers by Count →";
        }
    }

    // Concerts By Country Toggle
    private void OnConcertsByCountryTapped(object sender, EventArgs e)
    {
        //Toogle visibility
        ConcertsByCountryContent.IsVisible = !ConcertsByCountryContent.IsVisible;

        //Update header text based on state
        if (ConcertsByCountryContent.IsVisible)
        {
            ConcertsByCountryHeader.Text = "Concerts By Country ↓";
        }
        else
        {
            ConcertsByCountryHeader.Text = "Concerts By Country →";
        }
    }

    // Latest Concert Toggle
    private void OnLatestConcertTapped(object sender, EventArgs e)
    {
        // Toggle visibility
        LatestConcertContent.IsVisible = !LatestConcertContent.IsVisible;

        // Update header text based on state
        if (LatestConcertContent.IsVisible)
        {
            LatestConcertHeader.Text = "Latest Concert ↓";
        }
        else
        {
            LatestConcertHeader.Text = "Latest Concert →";
        }
    }
}
