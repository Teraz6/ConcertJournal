using ConcertJournal.Data;
using SkiaSharp;
using ConcertJournal.Models;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.Measure;
using SkiaSharp;
using ConcertJournal.Models.ViewModels;


namespace ConcertJournal.Views;

public partial class StatisticsPage : ContentPage
{
    //For performer listing
    public class PerformerViewModel
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public string CountText => $"{Count} concerts";
    }

    private readonly DatabaseContext _database;
    

    //Optimized performers loading
    private const int PageSize = 15;
    private int currentPage = 0;
    private ObservableCollection<PerformerViewModel> displayedPerformers = new();
    private List<PerformerViewModel> allPerformers;
    private List<PerformerViewModel> currentPerformers;

    public StatisticsPage(DatabaseContext database)
    {
        InitializeComponent();
        _database = database;

        PerformersCollection.ItemsSource = displayedPerformers;
    }

    // This method runs every time the page appears
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        LoadStatistics();
        await LoadPerformers();
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

        // Set stars
        AverageRatingStars.Rating = avgRating;

        // Concerts by country
        var concertsByCountry = concerts
            .Where(c => !string.IsNullOrWhiteSpace(c.Country))
            .GroupBy(c => c.Country.Trim())
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToList();

        // Latest concert date
        var latestConcert = concerts
            .Where(c => c.Date.HasValue)
            .OrderByDescending(c => c.Date)
            .FirstOrDefault();

        LatestConcertLabel.Text = latestConcert != null
            ? $"Latest concert: {latestConcert.EventTitle} on {latestConcert.Date:dd MMM yyyy}"
            : "Latest concert: N/A";
        
        // Concerts per year
        var concertsByYear = concerts
            .Where(c => c.Date.HasValue)
            .GroupBy(c => c.Date.Value.Year)
            .OrderByDescending(g => g.Key)
            .Select(g => $"{g.Key}: {g.Count()} concerts")
            .ToList();

        ConcertsByYearLabel.Text = concertsByYear.Any()
            ? "Concerts per year:\n" + string.Join("\n", concertsByYear)
            : "No concert date data available.";

        //For country chart, dont delete
        LoadCountryChart(concerts);
    }


    // Initialize or reset the performers collection for pagination
    private void InitializePerformersCollection()
    {
        currentPage = 0;
        displayedPerformers.Clear();

        currentPerformers ??= allPerformers;
        LoadNextPage();
    }

    // Load performers for performers tab
    private async Task LoadPerformers()
    {
        var concerts = await App.Database.GetConcertsAsync();

        allPerformers = concerts
            .Where(c => !string.IsNullOrWhiteSpace(c.Performers))
            .SelectMany(c => c.Performers.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(p => p.Trim())
            .GroupBy(p => p)
            .Select(g => new PerformerViewModel { Name = g.Key, Count = g.Count() })
            .OrderByDescending(p => p.Count)
            .ToList();

        InitializePerformersCollection();
    }

    private void LoadNextPage()
    {
        if (currentPerformers == null || currentPerformers.Count == 0)
            return;

        var nextBatch = currentPerformers
            .Skip(currentPage * PageSize)
            .Take(PageSize)
            .ToList();

        if (!nextBatch.Any())
            return; // nothing left to load

        foreach (var performer in nextBatch)
            displayedPerformers.Add(performer);

        currentPage++;
    }

    private void OnRemainingItemsThresholdReached(object sender, EventArgs e)
    {
        LoadNextPage();
    }

    // Search filter for performers tab
    private void OnPerformerSearchChanged(object sender, TextChangedEventArgs e)
    {
        string query = e.NewTextValue?.Trim() ?? "";

        currentPerformers = string.IsNullOrWhiteSpace(query)
            ? allPerformers
            : allPerformers
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

        // Reset paging and reload
        currentPage = 0;
        displayedPerformers.Clear();
        LoadNextPage();
    }

    // Sort buttons
    private void OnSortMostClicked(object sender, EventArgs e)
    {
        if (currentPerformers == null) return;

        currentPerformers = currentPerformers.OrderByDescending(p => p.Count).ToList();
        currentPage = 0;
        displayedPerformers.Clear();
        LoadNextPage();
    }

    private void OnSortLeastClicked(object sender, EventArgs e)
    {
        if (currentPerformers == null) return;

        currentPerformers = currentPerformers.OrderBy(p => p.Count).ToList();
        currentPage = 0;
        displayedPerformers.Clear();
        LoadNextPage();
    }

    //Countries Tab
    private void LoadCountryChart(List<Concert> concerts)
    {
        var countryGroups = concerts
        .GroupBy(c => string.IsNullOrWhiteSpace(c.Country) ? "Undefined" : c.Country.Trim())
        .Select(g => (Country: g.Key, Count: g.Count()))
        .OrderByDescending(x => x.Count)
        .ToList();

        var vm = new CountryChartViewModel(countryGroups);
        CountriesChart.BindingContext = vm;
        CountriesChart.Series = vm.CountrySeries;
        CountriesChart.XAxes = vm.XAxes;
        CountriesChart.YAxes = vm.YAxes;
    }
}
