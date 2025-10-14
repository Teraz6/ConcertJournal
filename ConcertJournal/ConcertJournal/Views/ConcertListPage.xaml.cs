using ConcertJournal.Models;

namespace ConcertJournal.Views;

public partial class ConcertListPage : ContentPage
{
	public ConcertListPage()
	{
		InitializeComponent();
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var concerts = await App.Database.GetConcertsAsync();
        ConcertListView.ItemsSource = concerts;
    }

    private async Task LoadConcerts()
    {
        var concerts = await App.Database.GetConcertsAsync();
        ConcertListView.ItemsSource = concerts;
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Concert selectedConcert)
        {
            // Navigate to AddConcertPage with the existing concert
            await Navigation.PushAsync(new AddConcertPage(selectedConcert));
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Concert concert)
        {
            bool confirm = await DisplayAlert(
                "Delete Concert",
                $"Are you sure you want to delete '{concert.EventTitle}'?",
                "Yes",
                "No");

            if (confirm)
            {
                await App.Database.DeleteConcertAsync(concert);
                await LoadConcerts(); // Refresh the list
            }
        }
    }

    //When clicked on Details button it will take you to Details page
    private async void OnDetailsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Concert concert)
        {
            await Navigation.PushAsync(new ConcertDetailsPage(concert));
        }
    }

    //NavigationBar code 
    private async void OnStartPageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage(),false);
    }

    private async void OnAddConcertClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddConcertPage(),false);
    }

    private async void OnConcertListClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ConcertListPage(), false);
    }
}