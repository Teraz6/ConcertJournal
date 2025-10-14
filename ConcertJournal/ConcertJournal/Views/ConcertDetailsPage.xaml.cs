using ConcertJournal.Models;

namespace ConcertJournal.Views;

public partial class ConcertDetailsPage : ContentPage
{
	public ConcertDetailsPage(Concert concert)
	{
		InitializeComponent();

		BindingContext = concert;
	}

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        if (BindingContext is Concert concert)
        {
            // Navigate to AddConcertPage with the selected concert to edit
            await Navigation.PushAsync(new AddConcertPage(concert));
        }
    }
}