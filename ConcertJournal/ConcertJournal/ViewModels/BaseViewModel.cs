using CommunityToolkit.Mvvm.ComponentModel;

namespace ConcertJournal.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string title = string.Empty;
    }
}
