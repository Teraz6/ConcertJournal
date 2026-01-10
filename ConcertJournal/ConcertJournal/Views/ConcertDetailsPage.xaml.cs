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
}