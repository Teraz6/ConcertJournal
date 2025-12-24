using ConcertJournal.Data;
using ConcertJournal.Models;
using ConcertJournal.Models.ViewModels;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp.Views.Maui;
using System.Collections.ObjectModel;

namespace ConcertJournal.Views;

public partial class StatisticsPage : ContentPage
{
    //For performer listing
    public class PerformerViewModel
    {
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; }
        public string CountText => $"{Count} concerts";
    }

    private readonly DatabaseContext _database;
    

    //Optimized performers loading
    private const int PageSize = 20;
    private int currentPage = 0;
    private readonly ObservableCollection<PerformerViewModel> displayedPerformers = [];
    private List<PerformerViewModel> allPerformers = default!;
    private List<PerformerViewModel>? currentPerformers;

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
        var avgRating = ratedConcerts.Count != 0 ? ratedConcerts.Average(c => c.Rating) : 0;

        // Set stars
        AverageRatingStars.Rating = avgRating;

        // Concerts by country
        var concertsByCountry = concerts
            .Where(c => !string.IsNullOrWhiteSpace(c.Country))
            .GroupBy(c => c.Country!.Trim())
            .Select(g => $"{g.Key}: {g.Count()}")
            .ToList();

        // Latest concert date
        var latestConcert = concerts
            .Where(c => c.Date!.HasValue)
            .OrderByDescending(c => c.Date)
            .FirstOrDefault();

        LatestConcertLabel.Text = latestConcert != null
            ? $"{latestConcert.EventTitle} on {latestConcert.Date:dd MMM yyyy}"
            : "Latest concert: N/A";

        // Concerts per year WITH performer count
        var concertsByYear = concerts
            .Where(c => c.Date.HasValue)
            .GroupBy(c => c.Date!.Value.Year)
            .OrderByDescending(g => g.Key)
            .Select(g =>
            {
                int year = g.Key;
                int concertCount = g.Count();

                // Collect performers in that year (split by comma)
                var performers = g
                    .Where(c => !string.IsNullOrWhiteSpace(c.Performers))
                    .SelectMany(c => c.Performers!
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim()))
                    .Distinct()
                    .Count();

                return new
                {
                    Year = year,
                    Concerts = concertCount,
                    Performers = performers
                };
            })
            .ToList();

        var formatted = new FormattedString();

        foreach (var item in concertsByYear)
        {
            formatted.Spans.Add(new Span
            {
                Text = item.Year.ToString(),
                FontAttributes = FontAttributes.Bold
            });

            formatted.Spans.Add(new Span
            {
                Text = $": {item.Concerts} concerts  |  {item.Performers} performers\n"
            });
        }

        ConcertsByYearLabel.FormattedText = formatted;

        // Find the year with most concerts
        var yearWithMostConcerts = concerts
            .Where(c => c.Date.HasValue)
            .GroupBy(c => c.Date!.Value.Year)
            .Select(g => new
            {
                Year = g.Key,
                ConcertCount = g.Count(),
                PerformerCount = g
                    .Where(c => !string.IsNullOrWhiteSpace(c.Performers))
                    .SelectMany(c => c.Performers!
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim()))
                    .Distinct()
                    .Count()
            })
            .OrderByDescending(x => x.ConcertCount)
            .FirstOrDefault();

        // Find the year with most performers
        var yearWithMostPerformers = concerts
            .Where(c => c.Date.HasValue)
            .GroupBy(c => c.Date!.Value.Year)
            .Select(g => new
            {
                Year = g.Key,
                ConcertCount = g.Count(),
                PerformerCount = g
                    .Where(c => !string.IsNullOrWhiteSpace(c.Performers))
                    .SelectMany(c => c.Performers!
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim()))
                    .Distinct()
                    .Count()
            })
            .OrderByDescending(x => x.PerformerCount)
            .FirstOrDefault();

        if (yearWithMostConcerts != null)
        {
            MostConcertsByYearLabel.Text =
                $"{yearWithMostConcerts.Year} ({yearWithMostConcerts.ConcertCount} concerts)";
        }

        if (yearWithMostPerformers != null)
        {
            MostPerformersByYearLabel.Text =
                $"{yearWithMostPerformers.Year} ({yearWithMostPerformers.PerformerCount} performers)";
        }

        //For country chart, dont delete
        LoadCountryChart(concerts);
    }


    // Initialize or reset the performers collection for pagination
    private void InitializePerformersCollection(bool resetList = false)
    {
        currentPage = 0;

        // Only clear if explicitly resetting (like first load)
        if (resetList)
            displayedPerformers.Clear();

        currentPerformers = allPerformers;
        LoadNextPage();
    }

    // Load performers for performers tab
    private async Task LoadPerformers()
    {
        var concerts = await App.Database.GetConcertsAsync();

        allPerformers = [.. concerts
            .Where(c => !string.IsNullOrWhiteSpace(c.Performers))
            .SelectMany(c => c.Performers!.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(p => p.Trim())
            .GroupBy(p => p)
            .Select(g => new PerformerViewModel { Name = g.Key, Count = g.Count() })
            .OrderByDescending(p => p.Count)];

        // Initialize the collection only for the first load
        InitializePerformersCollection(resetList: true);
    }

    private void LoadNextPage()
    {
        if (currentPerformers == null || currentPerformers.Count == 0)
            return;

        var nextBatch = currentPerformers
            .Skip(currentPage * PageSize)
            .Take(PageSize)
            .ToList();

        if (nextBatch.Count == 0)
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
            : [.. allPerformers.Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))];

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
            .Select(g =>
            {
                var concertCount = g.Count();

                // Split each Performer string into individual performers and get distinct count
                var performerCount = g
                    .SelectMany(c => (c.Performers ?? "")
                        .Split([','], StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim())) // remove extra spaces
                    .Distinct()
                    .Count();

                return (Country: g.Key, ConcertCount: concertCount, PerformerCount: performerCount);
            })
            .OrderByDescending(x => x.ConcertCount)
            .ToList();

        var vm = new CountryChartViewModel(countryGroups);
        CountriesChart.BindingContext = vm;
        CountriesChart.Series = vm.CountrySeries;
        CountriesChart.XAxes = vm.XAxes;
        CountriesChart.YAxes = vm.YAxes;

        // Dynamic legend color
        var legendTextColor = (Color)Application.Current!.Resources["TextColor"];
        CountriesChart.LegendTextPaint = new SolidColorPaint(legendTextColor.ToSKColor());
    }

    private async void OnPerformerButtonClicked(object sender, EventArgs e)
    {
        if (sender is not Button btn)
            return;

        if (btn.BindingContext is not PerformerViewModel performer)
            return;

        await Navigation.PushAsync(new PerformerDetailsPage(performer.Name, _database));
    }
}
