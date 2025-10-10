using System.Collections.ObjectModel;
using ConcertJournal.Models;
using ConcertJournal.Views;

namespace ConcertJournal.Views;

public partial class AddConcertPage : ContentPage
{
    
    public AddConcertPage()
    {
        InitializeComponent();
        BindingContext = this;
    }
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

    public ObservableCollection<string> Performers { get; set; } = new ObservableCollection<string>();

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

    private async void CreateButton_Clicked(object sender, EventArgs e)
    {
        //Gather all input data and save concert 
        var concert = new Concert
        {
            EventTitle = EventTitleEntry?.Text,
            Performers = string.Join(",", Performers), // Assuming Performers is a list
            Venue = VenueEntry?.Text,
            Country = CountryEntry?.Text,
            City = CityEntry?.Text,
            Date = DatePicker?.Date,
            Notes = NotesEditor?.Text
        };

        // This calls the Save method on the shared database instance
        await App.Database.SaveConcertAsync(concert);

        await DisplayAlert("Success", "Concert saved!", "OK");

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


}
