using ConcertJournal.ViewModels;

namespace ConcertJournal.Views;

public partial class AddConcertPage : ContentPage
{
    public AddConcertPage(AddConcertViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}