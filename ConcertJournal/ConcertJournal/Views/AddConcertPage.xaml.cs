using System.Collections.ObjectModel;
using ConcertJournal.Models;
using ConcertJournal.Views;

namespace ConcertJournal.Views;

[QueryProperty(nameof(ExistingConcert), "existingConcert")]
public partial class AddConcertPage : ContentPage
{
    private Concert _existingConcert;
    public Concert ExistingConcert
    {
        get => _existingConcert;
        set
        {
            _existingConcert = value;
            LoadConcertData();
        }
    }

    public ObservableCollection<string> Performers { get; set; } = new();

    public AddConcertPage(Concert existingConcert = null)
    {
        InitializeComponent();
        BindingContext = this;

        //_concert = concert ?? new Concert();

        ////Fill the entrys if editing an existing concert
        //if (concert != null)
        //{
        //    EventTitleEntry.Text = _concert.EventTitle;
        //    VenueEntry.Text = _concert.Venue;
        //    CountryEntry.Text = _concert.Country;
        //    CityEntry.Text = _concert.City;
        //    DatePicker.Date = (DateTime)_concert.Date;
        //    NotesEditor.Text = _concert.Notes;

        //    if (!string.IsNullOrWhiteSpace(_concert.Performers))
        //    {
        //        foreach (var name in _concert.Performers.Split(", "))
        //        {
        //            Performers.Add(name.Trim());
        //        }
        //    }
        //}
        //else
        //{
        //    DatePicker.Date = DateTime.Today;
        //}
    }

    private void LoadConcertData()
    {
        if(_existingConcert != null)
        {
            //Fill entrys with existing concert data
            EventTitleEntry.Text = _existingConcert.EventTitle;
            VenueEntry.Text = _existingConcert.Venue;
            CountryEntry.Text = _existingConcert.Country;
            CityEntry.Text = _existingConcert.City;
            NotesEditor.Text = _existingConcert.Notes;
            DatePicker.Date = _existingConcert.Date ?? DateTime.Now;

            SaveButton.Text = "Save";
        }
        else
        {
            SaveButton.Text = "Create";
            DatePicker.Date = DateTime.Now;
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

                //Optional: Update a ListView or Label showing performers
                //You can bind performers to a collectionView or listview
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

    //private async void CreateButton_Clicked(object sender, EventArgs e)
    //{
    //    //Gather all input data and save concert 
    //    var concert = new Concert
    //    {
    //        EventTitle = EventTitleEntry?.Text,
    //        Performers = string.Join(", ", Performers), // Assuming Performers is a list
    //        Venue = VenueEntry?.Text,
    //        Country = CountryEntry?.Text,
    //        City = CityEntry?.Text,
    //        Date = DatePicker?.Date,
    //        Notes = NotesEditor?.Text
    //    };

    //    // This calls the Save method on the shared database instance
    //    await App.Database.SaveConcertAsync(concert);

    //    await DisplayAlert("Success", "Concert saved!", "OK");

    //    // Clear all input fields safely
    //    if (EventTitleEntry != null) EventTitleEntry.Text = string.Empty;
    //    if (VenueEntry != null) VenueEntry.Text = string.Empty;
    //    if (CountryEntry != null) CountryEntry.Text = string.Empty;
    //    if (CityEntry != null) CityEntry.Text = string.Empty;
    //    if (NotesEditor != null) NotesEditor.Text = string.Empty;
    //    if (DatePicker != null) DatePicker.Date = DateTime.Today;

    //    // Clear performers list
    //    Performers.Clear();

    //}

    private async void SaveButton_Clicked(object sender, EventArgs e)
    {
        if (_existingConcert != null)
        {
            _existingConcert.EventTitle = EventTitleEntry?.Text;
            _existingConcert.Performers = string.Join(", ", Performers);
            _existingConcert.Venue = VenueEntry?.Text;
            _existingConcert.Country = CountryEntry?.Text;
            _existingConcert.City = CityEntry?.Text;
            _existingConcert.Date = DatePicker?.Date;
            _existingConcert.Notes = NotesEditor?.Text;

            await App.Database.SaveConcertAsync(_existingConcert);
            await DisplayAlert("Success", "Concert saved!", "OK");
        }
        else
        {
            var newConcert = new Concert
            {
                EventTitle = EventTitleEntry?.Text,
                Performers = string.Join(", ", Performers), // Assuming Performers is a list
                Venue = VenueEntry?.Text,
                Country = CountryEntry?.Text,
                City = CityEntry?.Text,
                Date = DatePicker?.Date,
                Notes = NotesEditor?.Text
            };

            await App.Database.SaveConcertAsync(newConcert);
            await DisplayAlert("Created", "Concert created successfully", "OK");
        }
            await Navigation.PopAsync();

        // Clear all input fields safely
        if (EventTitleEntry != null) EventTitleEntry.Text = string.Empty;
        if (VenueEntry != null) VenueEntry.Text = string.Empty;
        if (CountryEntry != null) CountryEntry.Text = string.Empty;
        if (CityEntry != null) CityEntry.Text = string.Empty;
        if (NotesEditor != null) NotesEditor.Text = string.Empty;
        if (DatePicker != null) DatePicker.Date = DateTime.Today;

        // Clear performers list
        Performers.Clear();
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
