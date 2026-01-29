using ConcertJournal.Models;
using ConcertJournal.ViewModels;

namespace ConcertJournal.Views;

public partial class ConcertListPage : UraniumUI.Pages.UraniumContentPage
{
    private readonly ConcertListViewModel _viewModel;

    public ConcertListPage(ConcertListViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ConcertListViewModel vm)
        {
            if (vm.Concerts.Count == 0)
            {
                await vm.RefreshAllAsync();
            }
        }
    }

    protected override bool OnBackButtonPressed()
    {
        var vm = BindingContext as ConcertListViewModel;
        if (vm?.HasSelection == true)
        {
            vm.ClearSelectionCommand.Execute(null);
            return true; // Stop the app from navigating back
        }
        return base.OnBackButtonPressed();
    }

    private async void OnCardTapped(object sender, TappedEventArgs e)
    {
        // 1. Get the visual element that was tapped
        var layout = sender as VisualElement;

        // 2. Extract the Concert object from that specific row's BindingContext
        if (layout?.BindingContext is Concert concert)
        {
            // 3. Access the ViewModel from the Page's BindingContext
            if (BindingContext is ConcertListViewModel vm)
            {
                // 4. Call the public method in your ViewModel
                await vm.CardClicked(concert);
            }
        }
    }

    private async void OnShowOptionsClicked(object sender, EventArgs e)
    {
        // 1. Identify the button that was clicked
        var button = (ImageButton)sender;

        // 2. Extract the Concert model from the row's BindingContext
        if (button.BindingContext is Concert concert)
        {
            // 3. Access your ViewModel and call the method directly
            if (BindingContext is ConcertListViewModel vm)
            {
                await vm.ShowOptions(concert);
            }
        }
    }

}