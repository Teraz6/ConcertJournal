using ConcertJournal.Models;
using ConcertJournal.Services;
using System.Collections.ObjectModel;

namespace ConcertJournal.Views;

public partial class AddConcertPage : ContentPage
{
    private Concert _existingConcert;
    private ObservableCollection<string> Performers = new ObservableCollection<string>();
    private ObservableCollection<string> MediaFiles = new ObservableCollection<string>();
    private const string DefaultCountryKey = "DefaultCountry";
    private const string DefaultCityKey = "DefaultCity";

    public AddConcertPage(Concert existingConcert = null)
    {
        InitializeComponent();

        _existingConcert = existingConcert;
        BindingContext = this;
        //Performer and media list
        PerformersList.ItemsSource = Performers;
        MediaCollectionView.ItemsSource = MediaFiles;

        // Load default country/city from settings
        string defaultCountry = Preferences.Get(DefaultCountryKey, string.Empty);
        string defaultCityy = Preferences.Get(DefaultCityKey, string.Empty);

        if (_existingConcert != null)
        {
            // Prefill fields
            EventTitleEntry.Text = _existingConcert.EventTitle;
            VenueEntry.Text = _existingConcert.Venue;
            CountryEntry.Text = _existingConcert.Country;
            CityEntry.Text = _existingConcert.City;
            NotesEditor.Text = _existingConcert.Notes;
            DatePicker.Date = _existingConcert.Date ?? DateTime.Today;
            ConcertRating.Rating = _existingConcert.Rating;

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
                foreach (var path in _existingConcert.MediaPaths.Split(';'))
                {
                    var trimmedPath = path.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedPath) && File.Exists(trimmedPath))
                        MediaFiles.Add(trimmedPath);
                }
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
            CountryEntry.Text = Preferences.Get(DefaultCountryKey, string.Empty);
            CityEntry.Text = Preferences.Get(DefaultCityKey, string.Empty);
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

    private async void OnAddImageClicked(object sender, EventArgs e)
    {
        try
        {
#if WINDOWS
            var results = await FilePicker.PickMultipleAsync(new PickOptions
            {
                PickerTitle = "Select images",
                FileTypes = FilePickerFileType.Images
            });
#else
            var results = await FilePicker.PickMultipleAsync(new PickOptions
            {
                PickerTitle = "Select images",
                FileTypes = FilePickerFileType.Images
            });
#endif

            if (results != null)
            {
                foreach (var file in results)
                {
                    if (File.Exists(file.FullPath))
                    {
                        MediaFiles.Add(file.FullPath);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to pick image: {ex.Message}", "OK");
        }
    }

    private void OnRemoveMediaClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is string mediaPath)
        {
            MediaFiles.Remove(mediaPath);
        }
    }

    private async void SaveButton_Clicked(object sender, EventArgs e)
    {
        // Update existing concert
        if (_existingConcert != null)
        {
            _existingConcert.EventTitle = EventTitleEntry?.Text.Trim();
            _existingConcert.Venue = VenueEntry?.Text.Trim();
            _existingConcert.Country = CountryEntry?.Text.Trim();
            _existingConcert.City = CityEntry?.Text.Trim();
            _existingConcert.Notes = NotesEditor?.Text.Trim();
            _existingConcert.Date = DatePicker?.Date ?? DateTime.Today;
            _existingConcert.Rating = ConcertRating.Rating;
            _existingConcert.Performers = string.Join(", ", Performers);
            _existingConcert.MediaPaths = string.Join(";", MediaFiles);

            await App.Database.SaveConcertAsync(_existingConcert);
            await DisplayAlert("Success", "Concert updated!", "OK");

            EventBus.OnConcertUpdated();
            await Navigation.PushAsync(new ConcertDetailsPage(_existingConcert));
        }
        else
        {
            // Create new concert
            var newConcert = new Concert
            {
                EventTitle = EventTitleEntry?.Text.Trim(),
                Venue = VenueEntry?.Text.Trim(),
                Country = CountryEntry?.Text.Trim(),
                City = CityEntry?.Text.Trim(),
                Notes = NotesEditor?.Text.Trim(),
                Date = DatePicker?.Date ?? DateTime.Today,
                Rating = ConcertRating.Rating,
                Performers = string.Join(", ", Performers),
                MediaPaths = string.Join(";", MediaFiles)
            };

            await App.Database.SaveConcertAsync(newConcert);
            await DisplayAlert("Success", "Concert created!", "OK");

            // Clear all fields for next entry
            if (EventTitleEntry != null) EventTitleEntry.Text = string.Empty;
            if (VenueEntry != null) VenueEntry.Text = string.Empty;
            if (CountryEntry != null) CountryEntry.Text = Preferences.Get(DefaultCountryKey, string.Empty);
            if (CityEntry != null) CityEntry.Text = Preferences.Get(DefaultCityKey, string.Empty);
            if (NotesEditor != null) NotesEditor.Text = string.Empty;
            PerformerEntry.Text = string.Empty;
            Performers.Clear();
            MediaFiles.Clear();
            if (DatePicker != null) DatePicker.Date = DateTime.Today;
            if (ConcertRating != null) ConcertRating.Rating = 0;

            EventBus.OnConcertCreated();
        }
    }
}