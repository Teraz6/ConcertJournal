using System.Collections.ObjectModel;
using ConcertJournal.Models;
using ConcertJournal.Views;

namespace ConcertJournal.Views;

public partial class AddConcertPage : ContentPage
{
    // Collection used for the performers list in XAML
    public ObservableCollection<string> Performers { get; set; } = new();

    public AddConcertPage()
    {
        InitializeComponent();
        BindingContext = this; // simple binding for Performers, etc.
    }

    // ---- Bottom bar navigation ----
    private async void OnStartPageClicked(object sender, EventArgs e)
    {
        // Go back to the first page instead of stacking another MainPage
        await Navigation.PopToRootAsync();
    }

    private async void OnAddConcertClicked(object sender, EventArgs e)
    {
        // Already on Add page; avoid pushing another
        await DisplayAlert("Info", "You're already on the Add Event page.", "OK");
    }

    private async void OnConcertListClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ConcertListPage(), false);
    }

    // ---- Performers add/remove ----
    private void AddNewPerformer(object sender, EventArgs e)
    {
        var performerName = PerformerEntry?.Text?.Trim();

        if (string.IsNullOrEmpty(performerName))
        {
            DisplayAlert("Missing Name", "Please enter a performer's name.", "OK");
            return;
        }

        if (Performers.Contains(performerName))
        {
            DisplayAlert("Duplicate", "Performer already added.", "OK");
            return;
        }

        Performers.Add(performerName);
        PerformerEntry.Text = string.Empty;
    }

    private void RemovePerformer(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is string performer)
        {
            Performers.Remove(performer);
        }
    }

    // ---- Create/save concert ----
    private async void CreateButton_Clicked(object sender, EventArgs e)
    {
        // QUICK: basic validation (optional, but helpful)
        if (string.IsNullOrWhiteSpace(EventTitleEntry?.Text))
        {
            await DisplayAlert("Missing Title", "Please enter an event title.", "OK");
            return;
        }

        // Build the model from the fields
        var concert = new Concert
        {
            EventTitle = EventTitleEntry?.Text,
            Performers = string.Join(",", Performers),
            Venue = VenueEntry?.Text,
            Country = CountryEntry?.Text,
            City = CityEntry?.Text,
            Date = EventDatePicker?.Date,   // <-- IMPORTANT: use EventDatePicker (rename in XAML)
            Notes = NotesEditor?.Text
        };

        // Save to DB
        await App.Database.SaveConcertAsync(concert);
        await DisplayAlert("Success", "Concert saved!", "OK");

        // Go back so MainPage.OnAppearing() can refresh/hide the Add button
        await Navigation.PopAsync();

        // (No need to clear fields after PopAsync because this page is going away.
        // If you ever want to stay on this page after saving, move the PopAsync below
        // and uncomment the clears.)

        
        EventTitleEntry.Text = string.Empty;
        VenueEntry.Text = string.Empty;
        CountryEntry.Text = string.Empty;
        CityEntry.Text = string.Empty;
        NotesEditor.Text = string.Empty;
        EventDatePicker.Date = DateTime.Today;
        Performers.Clear();
        
    }
}
