using ConcertJournal.ViewModels;

namespace ConcertJournal.Views;

public partial class ConcertDetailsPage : ContentPage
{
    private readonly ConcertDetailsViewModel _viewModel;

    public ConcertDetailsPage(ConcertDetailsViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // If you need to refresh data when coming back from the Edit page, 
        // you could call a method on the ViewModel here.
    }

    private async void OnImageTapped(object sender, TappedEventArgs e)
    {
        // The 'sender' is the visual element (Image), not the gesture
        if (sender is Image image && image.BindingContext is string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                await Navigation.PushModalAsync(new Views.ImageZoomPage(imagePath));
            }
        }
    }
}