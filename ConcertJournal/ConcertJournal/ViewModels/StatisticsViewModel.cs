using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConcertJournal.Data;
using ConcertJournal.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ConcertJournal.ViewModels;

public partial class StatisticsViewModel : BaseViewModel
{
    public DatabaseContext Database { get; }

    // --- Overview Properties ---
    [ObservableProperty] public partial string TotalConcertsText { get; set; } = "Loading...";
    [ObservableProperty] public partial double AverageRating { get; set; }
    [ObservableProperty] public partial string LatestConcertText { get; set; } = "N/A";
    [ObservableProperty] public partial string MostConcertsYearText { get; set; } = "N/A";
    [ObservableProperty] public partial string MostPerformersYearText { get; set; } = "N/A";
    [ObservableProperty] public partial FormattedString ConcertsByYearFormatted { get; set; } = new();

    // --- Performers Tab Properties ---
    private List<PerformerViewModel> _allPerformers = [];
    private List<PerformerViewModel> _filteredPerformers = [];
    public ObservableCollection<PerformerViewModel> DisplayedPerformers { get; } = new();

    [ObservableProperty] public partial string SearchQuery { get; set; } = string.Empty;

    private const int PageSize = 20;
    private int _currentPage = 0;
    private bool _isAddingItems;

    // --- Chart Property ---
    [ObservableProperty] public partial CountryChartViewModel? ChartVM { get; set; }

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
        // 1. Guard against multiple simultaneous taps
        if (IsLoading) return;

        IsLoading = true;
        try
        {
            var concerts = await Database.GetAllConcertsAsync();

            if (concerts == null || !concerts.Any())
            {
                TotalConcertsText = "No concert data available.";
                ResetPerformers(new List<PerformerViewModel>());
                ChartVM = null;
                return;
            }

            // 2. Data Processing (Offload heavy LINQ to a background thread if list is huge)
            await Task.Run(() =>
            {
                ProcessOverview(concerts);
                ProcessYearlyStats(concerts);

                var performers = concerts
                    .Where(c => !string.IsNullOrWhiteSpace(c.Performers))
                    .SelectMany(c => c.Performers!.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    .Select(p => p.Trim())
                    .GroupBy(p => p)
                    .Select(g => new PerformerViewModel { Name = g.Key, Count = g.Count() })
                    .OrderByDescending(p => p.Count)
                    .ToList();

                // Use MainThread to update the collection if ResetPerformers touches the UI
                MainThread.BeginInvokeOnMainThread(() => ResetPerformers(performers));

                var countryGroups = concerts
                    .GroupBy(c => string.IsNullOrWhiteSpace(c.Country) ? "Undefined" : c.Country.Trim())
                    .Select(g => (
                        Country: g.Key,
                        ConcertCount: g.Count(),
                        PerformerCount: g.SelectMany(c => (c.Performers ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim())).Distinct().Count()
                    ))
                    .ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ChartVM = new CountryChartViewModel(countryGroups);
                });
            });
        }
        catch (Exception ex)
        {
            // Always good to log errors when dealing with Databases
            Debug.WriteLine($"Error loading stats: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
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
            : _allPerformers.Where(p => p.Name!.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
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