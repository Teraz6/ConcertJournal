using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConcertJournal.Data;
using ConcertJournal.Models;
using System.Collections.ObjectModel;

namespace ConcertJournal.ViewModels;

public partial class StatisticsViewModel : ObservableObject
{
    public DatabaseContext Database { get; }

    // --- Overview Properties ---
    [ObservableProperty] private string totalConcertsText = "Loading...";
    [ObservableProperty] private double averageRating;
    [ObservableProperty] private string latestConcertText = "N/A";
    [ObservableProperty] private string mostConcertsYearText = "N/A";
    [ObservableProperty] private string mostPerformersYearText = "N/A";
    [ObservableProperty] private FormattedString concertsByYearFormatted = new();

    // --- Performers Tab Properties ---
    private List<PerformerViewModel> _allPerformers = new();
    private List<PerformerViewModel> _filteredPerformers = new();
    public ObservableCollection<PerformerViewModel> DisplayedPerformers { get; } = new();

    [ObservableProperty] private string searchQuery = string.Empty;

    private const int PageSize = 20;
    private int _currentPage = 0;
    private bool _isAddingItems;

    // --- Chart Property ---
    [ObservableProperty] private CountryChartViewModel? chartVM;

    public StatisticsViewModel(DatabaseContext database)
    {
        Database = database;
    }

    /// <summary>
    /// The master refresh method called by the code-behind or pull-to-refresh.
    /// </summary>
    [RelayCommand]
    public async Task RefreshAllAsync()
    {

        var concerts = await Database.GetAllConcertsAsync();

        if (concerts == null || !concerts.Any())
        {
            TotalConcertsText = "No concert data available.";
            ResetPerformers(new List<PerformerViewModel>());
            ChartVM = null;
            return;
        }

        // 1. Process Overview & Years
        ProcessOverview(concerts);
        ProcessYearlyStats(concerts);

        // 2. Process Performers
        var performers = concerts
            .Where(c => !string.IsNullOrWhiteSpace(c.Performers))
            .SelectMany(c => c.Performers!.Split(',', StringSplitOptions.RemoveEmptyEntries))
            .Select(p => p.Trim())
            .GroupBy(p => p)
            .Select(g => new PerformerViewModel { Name = g.Key, Count = g.Count() })
            .OrderByDescending(p => p.Count)
            .ToList();

        ResetPerformers(performers);

        // 3. Process Chart
        var countryGroups = concerts
            .GroupBy(c => string.IsNullOrWhiteSpace(c.Country) ? "Undefined" : c.Country.Trim())
            .Select(g => (
                Country: g.Key,
                ConcertCount: g.Count(),
                PerformerCount: g.SelectMany(c => (c.Performers ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim())).Distinct().Count()
            ))
            .ToList();

        ChartVM = new CountryChartViewModel(countryGroups);
    }

    private void ProcessOverview(List<Concert> concerts)
    {
        TotalConcertsText = $"Total concerts: {concerts.Count}";

        var ratedConcerts = concerts.Where(c => c.Rating > 0).ToList();
        AverageRating = ratedConcerts.Any() ? ratedConcerts.Average(c => c.Rating) : 0;

        var latest = concerts.Where(c => c.Date.HasValue).OrderByDescending(c => c.Date).FirstOrDefault();
        LatestConcertText = latest != null ? $"{latest.EventTitle} on {latest.Date:dd MMM yyyy}" : "N/A";
    }

    private void ProcessYearlyStats(List<Concert> concerts)
    {
        var yearlyData = concerts
            .Where(c => c.Date.HasValue)
            .GroupBy(c => c.Date!.Value.Year)
            .Select(g => new
            {
                Year = g.Key,
                Count = g.Count(),
                Performers = g.Where(c => !string.IsNullOrWhiteSpace(c.Performers))
                              .SelectMany(c => c.Performers!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                              .Select(p => p.Trim()).Distinct().Count()
            })
            .OrderByDescending(x => x.Year)
            .ToList();

        var fs = new FormattedString();
        var textColor = (Color)Application.Current!.Resources["TextColor"];

        foreach (var item in yearlyData)
        {
            fs.Spans.Add(new Span { Text = item.Year.ToString(), FontAttributes = FontAttributes.Bold, TextColor = textColor });
            fs.Spans.Add(new Span { Text = $": {item.Count} concerts  |  {item.Performers} performers\n", TextColor = textColor });
        }
        ConcertsByYearFormatted = fs;

        var topConcerts = yearlyData.OrderByDescending(x => x.Count).FirstOrDefault();
        MostConcertsYearText = topConcerts != null ? $"{topConcerts.Year} ({topConcerts.Count} concerts)" : "N/A";

        var topPerfs = yearlyData.OrderByDescending(x => x.Performers).FirstOrDefault();
        MostPerformersYearText = topPerfs != null ? $"{topPerfs.Year} ({topPerfs.Performers} performers)" : "N/A";
    }

    // --- Performer Logic ---

    private void ResetPerformers(List<PerformerViewModel> performers)
    {
        _allPerformers = performers;
        _filteredPerformers = _allPerformers;
        SearchQuery = string.Empty;
        ResetAndLoadFirstPage();
    }

    [RelayCommand]
    private void SearchPerformers()
    {
        _filteredPerformers = string.IsNullOrWhiteSpace(SearchQuery)
            ? _allPerformers
            : _allPerformers.Where(p => p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
        ResetAndLoadFirstPage();
    }

    [RelayCommand]
    private void SortMost()
    {
        _filteredPerformers = _filteredPerformers.OrderByDescending(p => p.Count).ToList();
        ResetAndLoadFirstPage();
    }

    [RelayCommand]
    private void SortLeast()
    {
        _filteredPerformers = _filteredPerformers.OrderBy(p => p.Count).ToList();
        ResetAndLoadFirstPage();
    }

    [RelayCommand]
    public void LoadNextPerformersPage()
    {
        // Prevent adding if we are already in the middle of a reset/load
        if (_isAddingItems) return;

        try
        {
            _isAddingItems = true;

            var nextBatch = _filteredPerformers
                .Skip(_currentPage * PageSize)
                .Take(PageSize)
                .ToList();

            if (nextBatch.Count == 0) return;

            foreach (var p in nextBatch)
            {
                DisplayedPerformers.Add(p);
            }
            _currentPage++;
        }
        finally
        {
            _isAddingItems = false;
        }
    }

    private void ResetAndLoadFirstPage()
    {
        _currentPage = 0;
        DisplayedPerformers.Clear();
        LoadNextPerformersPage();
    }

    partial void OnSearchQueryChanged(string value)
    {
        SearchPerformersCommand.Execute(null);
    }
}