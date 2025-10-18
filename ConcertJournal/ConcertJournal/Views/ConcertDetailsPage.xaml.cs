using ConcertJournal.Models;
using System.Collections.ObjectModel;
using MauiMap = Microsoft.Maui.Controls.Maps.Map;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Linq;

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
            var files = concert.MediaPaths.Split("; ", StringSplitOptions.RemoveEmptyEntries);
            foreach (var file in files)
            {
                MediaFiles.Add(file.Trim());
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
        await Shell.Current.GoToAsync("..", animate: true);
    }
}