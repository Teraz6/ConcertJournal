using ConcertJournal.ViewModels;

namespace ConcertJournal.Views;

public partial class StatisticsPage : ContentPage
{
    private readonly StatisticsViewModel _viewModel;

    public StatisticsPage(StatisticsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Call the refresh logic in the ViewModel
        await _viewModel.RefreshAllAsync();
    }

    private void OnRemainingItemsThresholdReached(object sender, EventArgs e)
    {
        _viewModel.LoadNextPerformersPageCommand.Execute(null);
    }

    private async void OnPerformerButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button { BindingContext: PerformerViewModel performer })
        {
            // Professional way: Use route names and dictionaries
            await Shell.Current.GoToAsync("PerformerDetailsPage", new Dictionary<string, object>
        {
            { "PerformerName", performer.Name! }
        });
        }
    }
}