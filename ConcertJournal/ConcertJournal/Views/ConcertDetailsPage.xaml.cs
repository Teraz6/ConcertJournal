using ConcertJournal.Models;
using System.Collections.ObjectModel;
using ConcertJournal.Services;

namespace ConcertJournal.Views;

public partial class ConcertDetailsPage : ContentPage
{
        public ObservableCollection<string> MediaFiles { get; set; } = new();
        private Concert _concert;

	public ConcertDetailsPage(Concert concert)
	{
		InitializeComponent();
        _concert = concert;
        BindingContext = concert;

        // Load images
        if (!string.IsNullOrWhiteSpace(concert.MediaPaths))
        {
            foreach (var path in concert.MediaPaths.Split(';'))
            {
                var trimmedPath = path.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedPath) && File.Exists(trimmedPath))
                    MediaFiles.Add(trimmedPath);
            }
        }

        MediaCollectionView.ItemsSource = MediaFiles;
    }

    private void OnImageTapped(object sender, TappedEventArgs e)
    {
        if (sender is Image image && image.Source is FileImageSource source)
        {
            PopupImage.Source = source.File;
            ImageOverlay.IsVisible = true;
        }
        else if (sender is Image img)
        {
            PopupImage.Source = img.Source;
            ImageOverlay.IsVisible = true;
        }
    }

    private void OnCloseImageTapped(object sender, EventArgs e)
    {
        ImageOverlay.IsVisible = false;
    }

    //Update button 
    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        if (BindingContext is Concert concert)
        {
            // Navigate to AddConcertPage with the selected concert to edit
            await Navigation.PushAsync(new AddConcertPage(concert));
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//ConcertListPage", animate: true);
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is Concert concert)
        {
            bool confirm = await DisplayAlert(
                "Delete Concert",
                $"Are you sure you want to delete '{concert.EventTitle}'?",
                "Yes",
                "No");

            if (confirm)
            {
                await App.Database.DeleteConcertAsync(concert);
                await Shell.Current.GoToAsync($"//ConcertListPage", animate: true);
            }
        }
    }

    private async void OnPerformerTapped(object sender, TappedEventArgs e)
    {
        if (_concert != null && !string.IsNullOrWhiteSpace(_concert.Performers))
        {
            // If there are multiple performers, show an action sheet to choose
            var performers = _concert.Performers
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();

            if (performers.Length == 0)
                return;

            string selectedPerformer;
        
            if (performers.Length == 1)
            {
                selectedPerformer = performers[0];
            }
            else
            {
                // Show action sheet if multiple performers
                selectedPerformer = await DisplayActionSheet(
                    "Select Performer", 
                    "Cancel", 
                    null, 
                    performers);
            
                if (selectedPerformer == "Cancel" || string.IsNullOrWhiteSpace(selectedPerformer))
                    return;
            }

            // Navigate to PerformerDetailsPage with database context
            await Navigation.PushAsync(new PerformerDetailsPage(selectedPerformer, App.Database));
        }
    }
}