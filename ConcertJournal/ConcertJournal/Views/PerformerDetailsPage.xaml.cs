using ConcertJournal.Data;
using ConcertJournal.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ConcertJournal.Views;

public partial class PerformerDetailsPage : ContentPage
{
    private readonly DatabaseContext _database;

    public string PerformerName { get; }
    public ObservableCollection<Concert> Concerts { get; set; } = new();

    public PerformerDetailsPage(string performerName, DatabaseContext database)
    {
        InitializeComponent();
        PerformerName = performerName;
        _database = database;

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Get concerts from database
        var concerts = await App.Database.GetConcertsAsync();

        // Filter concerts where Performers contains this name
        var filtered = concerts
            .Where(c => !string.IsNullOrEmpty(c.Performers) &&
                        c.Performers
                        .Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim())
                        .Contains(PerformerName))
            .OrderByDescending(c => c.Date)
            .ToList(); // Ensure filtered is a List<Concert>

        Concerts.Clear();

        // Add the filtered concerts to the observable collection
        foreach (var concert in filtered)
            Concerts.Add(concert);

        // Calculate how many times the performer has been seen
        TimesSeenLabel.Text = filtered.Count.ToString();  // Count is valid for List<Concert>

        // Calculate different countries (distinct locations)
        var uniqueCountries = filtered
            .Select(c => c.Location)
            .Where(location => !string.IsNullOrEmpty(location))  // Avoid null or empty locations
            .Distinct()
            .ToList();
        CountriesSeenLabel.Text = uniqueCountries.Count.ToString();

        // First time seen
        var firstSeenConcert = filtered.OrderBy(c => c.Date).FirstOrDefault();
        FirstSeenLabel.Text = firstSeenConcert?.Date?.ToString("dd MMM yyyy") ?? "-";  // Handle nullable DateTime

        // Most recent time seen
        var recentSeenConcert = filtered.OrderByDescending(c => c.Date).FirstOrDefault();
        RecentSeenLabel.Text = recentSeenConcert?.Date?.ToString("dd MMM yyyy") ?? "-";  // Handle nullable DateTime
    }

    private async void OnDetailsClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is Concert concert)
        {
            await Navigation.PushAsync(new ConcertDetailsPage(concert));
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        try
        {
            // Try standard navigation pop first
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync(true);
            }
            else
            {
                // Fallback to Shell navigation to root
                await Shell.Current.GoToAsync("//ConcertListPage");
            }
        }
        catch (Exception ex)
        {
            // Last resort - go to root
            await Shell.Current.GoToAsync("//ConcertListPage");
        }
    }
}
