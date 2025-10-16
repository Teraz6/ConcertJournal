using ConcertJournal.Models;
using System;
using System.Collections.ObjectModel;

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

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        if (BindingContext is Concert concert)
        {
            // Navigate to AddConcertPage with the selected concert to edit
            await Navigation.PushAsync(new AddConcertPage(concert));
        }
    }
}