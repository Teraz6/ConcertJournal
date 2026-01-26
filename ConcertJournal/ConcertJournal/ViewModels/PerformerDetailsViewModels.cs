using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConcertJournal.Models;
using ConcertJournal.ServiceInterface;
using ConcertJournal.Services;
using System.Collections.ObjectModel;

namespace ConcertJournal.ViewModels;

[QueryProperty(nameof(PerformerName), "PerformerName")]
public partial class PerformerDetailsViewModel : ObservableObject
{
    private readonly IConcertService _concertService;
    private readonly AudioDbServices _audioDbServices;

    [ObservableProperty] private string _performerName;
    [ObservableProperty] private string _timesSeen = "-";
    [ObservableProperty] private string _countriesSeen = "-";
    [ObservableProperty] private string _firstSeen = "-";
    [ObservableProperty] private string _recentSeen = "-";
    [ObservableProperty] private string _backgroundImageUrl = "";

    public ObservableCollection<Concert> Concerts { get; } = new();

    public PerformerDetailsViewModel(IConcertService concertService, AudioDbServices audioDbServices)
    {
        _concertService = concertService;
        _audioDbServices = audioDbServices;
    }

    // This triggers automatically when PerformerName is passed via navigation
    partial void OnPerformerNameChanged(string value)
    {
        _ = LoadStatsAsync();
        _ = LoadBackgroundImageAsync();
    }

    [RelayCommand]
    public async Task LoadStatsAsync()
    {
        if (string.IsNullOrEmpty(PerformerName)) return;

        // 1. Get all concerts from the Service
        var allConcerts = await _concertService.GetConcertsPagedAsync(0, 5000, "Default", "");

        // 2. Filter logic (moving it out of the code-behind)
        var filtered = allConcerts
            .Where(c => !string.IsNullOrEmpty(c.Performers) &&
                        c.Performers.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim())
                        .Contains(PerformerName, StringComparer.OrdinalIgnoreCase))
            .OrderByDescending(c => c.Date)
            .ToList();

        // 3. Update UI Collections
        Concerts.Clear();
        foreach (var concert in filtered) Concerts.Add(concert);

        // 4. Calculate Stats
        TimesSeen = filtered.Count.ToString();

        CountriesSeen = filtered
            .Select(c => c.Country) // Using Country property for accuracy
            .Where(loc => !string.IsNullOrEmpty(loc))
            .Distinct().Count().ToString();

        var first = filtered.OrderBy(c => c.Date).FirstOrDefault();
        FirstSeen = first?.Date?.ToString("dd MMM yyyy") ?? "-";

        var recent = filtered.OrderByDescending(c => c.Date).FirstOrDefault();
        RecentSeen = recent?.Date?.ToString("dd MMM yyyy") ?? "-";
    }

    private async Task LoadBackgroundImageAsync()
    {
        if (string.IsNullOrEmpty(PerformerName)) return;

        var artist = await _audioDbServices.GetArtistDetailsAsync(PerformerName);

        // Use fanart for background, fallback to banner if fanart is missing
        if (!string.IsNullOrWhiteSpace(artist?.StrArtistFanart))
        {
            BackgroundImageUrl = artist.StrArtistFanart;
        }
        else
        {
            BackgroundImageUrl = artist?.StrArtistBanner!;
        }
    }

    [RelayCommand]
    private async Task GoToDetailsAsync(Concert concert)
    {
        await Shell.Current.GoToAsync(nameof(Views.ConcertDetailsPage), new Dictionary<string, object>
        {
            { "Concert", concert }
        });
    }

    [RelayCommand]
    private async Task BackAsync() => await Shell.Current.GoToAsync("..");
}