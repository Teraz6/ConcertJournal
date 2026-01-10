using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConcertJournal.Views;

namespace ConcertJournal.ViewModels
{
    public partial class PerformerViewModel : ObservableObject
    {
        public string Name { get; set; }
        public int Count { get; set; }
        public string CountText => $"{Count} {(Count == 1 ? "Concert" : "Concerts")}";

        [RelayCommand]
        private async Task SelectPerformer()
        {
            // Example: Navigate to a filtered list or show a popup
            await Shell.Current.GoToAsync($"{nameof(PerformerDetailsPage)}?PerformerName={Name}");
        }
    }
}
