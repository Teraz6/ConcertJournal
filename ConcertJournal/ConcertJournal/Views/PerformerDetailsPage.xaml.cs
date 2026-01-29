using ConcertJournal.Models;
using ConcertJournal.ViewModels;

namespace ConcertJournal.Views;

[QueryProperty(nameof(PerformerName), "PerformerName")]
public partial class PerformerDetailsPage : ContentPage
{
    public string? PerformerName { get; set; } // Set by Shell

    public PerformerDetailsPage(PerformerDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnDetailsClicked(object sender, EventArgs e)
    {
        // 1. Identify the button that was clicked
        var button = (ImageButton)sender;

        // 2. Extract the 'Concert' object from the button's BindingContext
        if (button.BindingContext is Concert selectedConcert)
        {
            // 3. Perform the navigation directly from the View
            await Shell.Current.GoToAsync(nameof(ConcertDetailsPage), new Dictionary<string, object>
        {
            { "Concert", selectedConcert }
        });
        }
    }
}