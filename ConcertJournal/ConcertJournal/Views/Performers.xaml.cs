using System.Collections.ObjectModel;
using ConcertJournal.Models;
using ConcertJournal.Services;

namespace ConcertJournal.Views;

public partial class Performers : ContentPage
{
    public ObservableCollection<PerformerCount> PerformerStats { get; } = new();

    public Performers()
    {
        InitializeComponent();
        BindingContext = this;

        // refresh when data changes (same pattern as your list page)
        EventBus.ConcertCreated += async () => await LoadStats();
        EventBus.ConcertUpdated += async () => await LoadStats();
        ImportServices.ConcertsImported += async () => await LoadStats();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadStats();
    }

    private async Task LoadStats()
    {
        // 1) get all concerts from your SQLite DB
        var concerts = await App.Database.GetConcertsAsync();

        // 2) count occurrences for each performer name (every typed occurrence)
        var counts = new Dictionary<string, (string Display, int Count)>(StringComparer.OrdinalIgnoreCase);

        foreach (var c in concerts)
        {
            foreach (var name in EnumeratePerformers(c, false))
            {
                if (counts.TryGetValue(name, out var cur))
                    counts[name] = (cur.Display, cur.Count + 1);
                else
                    counts[name] = (name, 1); // keep the first-seen casing as Display
            }
        }

        // 3) order: most occurrences first, then A→Z
        var ordered = counts.Values
            .Select(x => new PerformerCount { Name = x.Display, Occurrences = x.Count })
            .OrderByDescending(x => x.Occurrences)
            .ThenBy(x => x.Name)
            .ToList();

        // 4) update the UI once
        PerformerStats.Clear();
        foreach (var row in ordered)
            PerformerStats.Add(row);
    }

    private IEnumerable<string> EnumeratePerformers(Concert concert, bool distinctWithinConcert = false)
    {
        if (string.IsNullOrWhiteSpace(concert.Performers))
            yield break;

        var separators = new[] { ',', ';', '/', '&', '+' };

        if (distinctWithinConcert)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var raw in concert.Performers.Split(separators, StringSplitOptions.RemoveEmptyEntries))
            {
                var name = raw.Trim();
                if (name.Length == 0) continue;
                if (seen.Add(name)) yield return name;
            }
        }
        else
        {
            foreach (var raw in concert.Performers.Split(separators, StringSplitOptions.RemoveEmptyEntries))
            {
                var name = raw.Trim();
                if (name.Length > 0) yield return name;
            }
        }
    }
}