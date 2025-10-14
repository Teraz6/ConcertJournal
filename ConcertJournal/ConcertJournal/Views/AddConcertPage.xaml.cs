using ConcertJournal.Models;
using ConcertJournal.Views;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;


namespace ConcertJournal.Views;

public partial class AddConcertPage : ContentPage
{
    private Concert _existingConcert;
    private ObservableCollection<string> Performers = new ObservableCollection<string>();
    private ObservableCollection<string> MediaFiles = new ObservableCollection<string>();

    public AddConcertPage(Concert existingConcert = null)
    {
        InitializeComponent();

        _existingConcert = existingConcert;
        //Performer and media list
        PerformersList.ItemsSource = Performers;
        MediaCollectionView.ItemsSource = MediaFiles;

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

            // Load media
            if (!string.IsNullOrWhiteSpace(_existingConcert.MediaPaths))
            {
                foreach (var path in _existingConcert.MediaPaths.Split(";"))
                    MediaFiles.Add(path);
            }

            // Change button and Title text
            SaveButton.Text = "Save";
            AddConcertPageTitle.Text = "Edit Event";
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
            _existingConcert.MediaPaths = string.Join("; ", MediaFiles);

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
                Performers = string.Join(", ", Performers),
                MediaPaths = string.Join("; ", MediaFiles)
            };

            await App.Database.SaveConcertAsync(newConcert);
            await DisplayAlert("Success", "Concert created!", "OK");

            // Clear all fields for next entry
            if (EventTitleEntry != null) EventTitleEntry.Text = string.Empty;
            if (VenueEntry != null) VenueEntry.Text = string.Empty;
            if (CountryEntry != null) CountryEntry.Text = string.Empty;
            if (CityEntry != null) CityEntry.Text = string.Empty;
            if (NotesEditor != null) NotesEditor.Text = string.Empty;
            PerformerEntry.Text = string.Empty;
            Performers.Clear();
            MediaFiles.Clear();
            if (DatePicker != null) DatePicker.Date = DateTime.Today;
        }
    }

    private async void OnAddImageClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Alert", "Uploading function not implemented", "OK");
    }

    private async void OnAddVideoClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Alert", "Uploading function not implemented", "OK");
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
