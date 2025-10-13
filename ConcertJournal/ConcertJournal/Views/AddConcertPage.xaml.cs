using System.Collections.ObjectModel;
using ConcertJournal.Models;
using ConcertJournal.Views;

namespace ConcertJournal.Views;

public partial class AddConcertPage : ContentPage
{
    private Concert _existingConcert;
    private ObservableCollection<string> Performers = new ObservableCollection<string>();

    public AddConcertPage(Concert existingConcert = null)
    {
        InitializeComponent();

        _existingConcert = existingConcert;

        // Bind performers list
        PerformersList.ItemsSource = Performers;

        if (_existingConcert != null)
        {
            // Prefill fields
            EventTitleEntry.Text = _existingConcert.EventTitle;
            VenueEntry.Text = _existingConcert.Venue;
            CountryEntry.Text = _existingConcert.Country;
            CityEntry.Text = _existingConcert.City;
            NotesEditor.Text = _existingConcert.Notes;
            DatePicker.Date = _existingConcert.Date ?? DateTime.Today;

            // Load performers
            if (!string.IsNullOrWhiteSpace(_existingConcert.Performers))
            {
                foreach (var name in _existingConcert.Performers.Split(","))
                {
                    Performers.Add(name.Trim());
                }
            }

            // Change button text to "Save"
            SaveButton.Text = "Save";
        }
        else
        {
            // New concert
            SaveButton.Text = "Create";
            DatePicker.Date = DateTime.Today;
        }
    }

    private void AddNewPerformer(object sender, EventArgs e)
    {
        var performerName = PerformerEntry?.Text?.Trim();

        if (!string.IsNullOrEmpty(performerName))
        {
            if (!Performers.Contains(performerName))
            {
                Performers.Add(performerName);
                PerformerEntry.Text = string.Empty;
            }
            else
            {
                DisplayAlert("Duplicate", "Performer already added.", "OK");
            }
        }
        else
        {
            DisplayAlert("Missing Name", "Please enter a performer's name", "OK");
        }
    }

    private void RemovePerformer(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is string performer)
        {
            Performers.Remove(performer);
        }
    }

    private async void SaveButton_Clicked(object sender, EventArgs e)
    {
        // Update existing concert
        if (_existingConcert != null)
        {
            _existingConcert.EventTitle = EventTitleEntry?.Text;
            _existingConcert.Venue = VenueEntry?.Text;
            _existingConcert.Country = CountryEntry?.Text;
            _existingConcert.City = CityEntry?.Text;
            _existingConcert.Notes = NotesEditor?.Text;
            _existingConcert.Date = DatePicker?.Date ?? DateTime.Today;
            _existingConcert.Performers = string.Join(", ", Performers);

            await App.Database.SaveConcertAsync(_existingConcert);
            await DisplayAlert("Success", "Concert updated!", "OK");

            await Navigation.PopAsync();
        }
        else
        {
            // Create new concert
            var newConcert = new Concert
            {
                EventTitle = EventTitleEntry?.Text,
                Venue = VenueEntry?.Text,
                Country = CountryEntry?.Text,
                City = CityEntry?.Text,
                Notes = NotesEditor?.Text,
                Date = DatePicker?.Date ?? DateTime.Today,
                Performers = string.Join(", ", Performers)
            };

            await App.Database.SaveConcertAsync(newConcert);
            await DisplayAlert("Success", "Concert created!", "OK");

            // Clear all fields for next entry
            EventTitleEntry.Text = string.Empty;
            VenueEntry.Text = string.Empty;
            CountryEntry.Text = string.Empty;
            CityEntry.Text = string.Empty;
            NotesEditor.Text = string.Empty;
            PerformerEntry.Text = string.Empty;
            Performers.Clear();
            DatePicker.Date = DateTime.Today;
        }
    }


    //Navigation code
    private async void OnStartPageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage(), false);
    }

    private async void OnAddConcertClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddConcertPage(), false);
    }

    private async void OnConcertListClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ConcertListPage(), false);
    }

}
