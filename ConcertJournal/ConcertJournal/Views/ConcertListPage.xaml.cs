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
}