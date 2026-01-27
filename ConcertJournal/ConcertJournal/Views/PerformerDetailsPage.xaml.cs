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
}