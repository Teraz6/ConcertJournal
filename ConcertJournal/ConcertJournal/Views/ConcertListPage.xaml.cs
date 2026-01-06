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

        await _viewModel.RefreshAllAsync();
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
}