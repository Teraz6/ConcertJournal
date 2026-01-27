using CommunityToolkit.Mvvm.ComponentModel;

namespace ConcertJournal.ViewModels
{
    public partial class BaseViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial bool IsLoading { get; set; }

        [ObservableProperty]
        public partial string Title { get; set; } = string.Empty;
    }
}
