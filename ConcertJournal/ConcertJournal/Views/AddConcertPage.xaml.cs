using ConcertJournal.Models;
using ConcertJournal.Services;
using ConcertJournal.Views;
using Microsoft.Maui.Storage;
using System;
using System.Collections.ObjectModel;


namespace ConcertJournal.Views;

public partial class AddConcertPage : ContentPage
{
<<<<<<< HEAD
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
=======
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
>>>>>>> 6446c03978ca5118e74c99d43fe711054a317a4d

        if (_existingConcert != null)
        {
            // Prefill fields
            EventTitleEntry.Text = _existingConcert.EventTitle;
            VenueEntry.Text = _existingConcert.Venue;
            CountryEntry.Text = _existingConcert.Country;
            CityEntry.Text = _existingConcert.City;
            NotesEditor.Text = _existingConcert.Notes;
            DatePicker.Date = _existingConcert.Date ?? DateTime.Today;

<<<<<<< HEAD
    // ---- Performers add/remove ----
=======
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

>>>>>>> 6446c03978ca5118e74c99d43fe711054a317a4d
    private void AddNewPerformer(object sender, EventArgs e)
    {
        var performerName = PerformerEntry?.Text?.Trim();

        if (string.IsNullOrEmpty(performerName))
        {
<<<<<<< HEAD
            DisplayAlert("Missing Name", "Please enter a performer's name.", "OK");
            return;
        }

        if (Performers.Contains(performerName))
=======
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
>>>>>>> 6446c03978ca5118e74c99d43fe711054a317a4d
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

<<<<<<< HEAD
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
        await Navigation.PopAsync();   // <- This returns to MainPage


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
=======
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

            EventBus.OnConcertCreated();
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

    private async void OnAddVideoClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Alert", "Uploading function not implemented", "OK");
    }

    private void OnRemoveMediaClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is string mediaPath)
        {
            MediaFiles.Remove(mediaPath);
        }
    }
>>>>>>> 6446c03978ca5118e74c99d43fe711054a317a4d
}
