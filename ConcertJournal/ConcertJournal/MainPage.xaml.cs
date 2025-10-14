using System;
using System.Collections.ObjectModel;
using System.Linq;                      // <= needed for LINQ
using ConcertJournal.Models;            // for Concert
using ConcertJournal.Views;             // for navigation pages

namespace ConcertJournal;

public partial class MainPage : ContentPage
{
    // ----- UI bindings -----
    // Add button visibility (if you still bind to it in XAML)
    bool _isAddButtonVisible = true;
    public bool IsAddButtonVisible
    {
        get => _isAddButtonVisible;
        set { _isAddButtonVisible = value; OnPropertyChanged(); }
    }

    // Show/hide stats frame
    bool _hasConcerts;
    public bool HasConcerts
    {
        get => _hasConcerts;
        set { _hasConcerts = value; OnPropertyChanged(); }
    }

    // Status counts
    int _happenedCount, _missedCount, _cancelledCount, _totalCount;
    public int HappenedCount { get => _happenedCount; set { _happenedCount = value; OnPropertyChanged(); } }
    public int MissedCount { get => _missedCount; set { _missedCount = value; OnPropertyChanged(); } }
    public int CancelledCount { get => _cancelledCount; set { _cancelledCount = value; OnPropertyChanged(); } }
    public int TotalCount { get => _totalCount; set { _totalCount = value; OnPropertyChanged(); } }

    // Top lists for stats panel (your XAML binds to these)
    public ObservableCollection<StatRow> PerformerStats { get; } = new();
    public ObservableCollection<StatRow> LocationStats { get; } = new();
    public ObservableCollection<StatRow> DateStats { get; } = new();   // <= FIXED

    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    // Load data each time the page appears
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var concerts = await App.Database.GetConcertsAsync();

        ConcertsView.ItemsSource = concerts;          // binds the list
        HasConcerts = concerts?.Count > 0;            // show stats when there are items
        IsAddButtonVisible = !HasConcerts;            // if you still bind the Add button

        BuildStatusCounts(concerts);                  // fills Happened/Missed/Cancelled/Total
        BuildTopLists(concerts);                      // fills Performer/Location/Date lists
    }

    // ---- Bottom bar navigation ----
    private async void OnStartPageClicked(object sender, EventArgs e)
        => await Navigation.PopToRootAsync();

    private async void OnAddConcertClicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new AddConcertPage(), false);

    private async void OnConcertListClicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new ConcertListPage(), false);

    // ===== Stats helpers =====
    private void BuildStatusCounts(IList<Concert> concerts)
    {
        HappenedCount = MissedCount = CancelledCount = 0;
        TotalCount = concerts?.Count ?? 0;
        if (concerts == null || concerts.Count == 0) return;

        foreach (var c in concerts)
        {
            var status = GetStatus(c); // "happened" | "missed" | "cancelled" | ""
            switch (status)
            {
                case "cancelled": CancelledCount++; break;
                case "missed": MissedCount++; break;
                case "happened": HappenedCount++; break;
            }
        }
    }

    // No Status property in your model, so use date-only fallback:
    // past/ today => "happened"; future => "" (ignored in counts).
    // Extend here if you add flags like IsCancelled/Attended later.
    private static string GetStatus(Concert c)
    {
        //if (c == null) return "";
        //if (c.IsCancelled) return "cancelled";
        //if (c.Attended == true) return "happened";
        //if (c.Attended == false && c.Date <= DateTime.Today) return "missed";
        if (c.Date <= DateTime.Today) return "happened"; // fallback
        return "";

        if (c?.Date is DateTime d && d.Date <= DateTime.Today)
            return "happened";

        return "";
    }

    // Build the three “Top” lists used by your XAML
    private void BuildTopLists(IList<Concert> concerts)
    {
        PerformerStats.Clear();
        LocationStats.Clear();
        DateStats.Clear();

        if (concerts == null || concerts.Count == 0) return;

        // Performers (split by comma)
        var performerCounts = concerts
            .SelectMany(c => (c.Performers ?? "")
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            .GroupBy(p => p)
            .Select(g => new StatRow(g.Key, g.Count()))
            .OrderByDescending(r => r.Count).ThenBy(r => r.Name).Take(5);
        foreach (var r in performerCounts) PerformerStats.Add(r);

        // Locations (Country / City)
        var locationCounts = concerts
            .Select(c => $"{(c.Country ?? "").Trim()}{(string.IsNullOrWhiteSpace(c.City) ? "" : " / " + c.City.Trim())}")
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .GroupBy(s => s)
            .Select(g => new StatRow(g.Key, g.Count()))
            .OrderByDescending(r => r.Count).ThenBy(r => r.Name).Take(5);
        foreach (var r in locationCounts) LocationStats.Add(r);

        // Dates (Month-Year)
        var dateCounts = concerts
            .Where(c => c.Date.HasValue)
            .GroupBy(c => new { c.Date!.Value.Year, c.Date!.Value.Month })
            .Select(g => new StatRow(new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"), g.Count()))
            .OrderByDescending(r => r.Count).ThenBy(r => r.Name).Take(5);
        foreach (var r in dateCounts) DateStats.Add(r);
    }
}

// Row model for the small stat lists
public record StatRow(string Name, int Count);