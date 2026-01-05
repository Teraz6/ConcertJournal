using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConcertJournal.Models;
using ConcertJournal.ServiceInterface;
using ConcertJournal.Views;
using System.Collections.ObjectModel;

namespace ConcertJournal.ViewModels;

[QueryProperty(nameof(Concert), "Concert")]
public partial class ConcertDetailsViewModel : ObservableObject
{
    private readonly IConcertService _concertService;

    [ObservableProperty]
    private Concert _concert;

    // These collections will hold the parsed data for the UI
    public ObservableCollection<string> Performers { get; } = new();
    public ObservableCollection<string> MediaFiles { get; } = new();

    public ConcertDetailsViewModel(IConcertService concertService)
    {
        _concertService = concertService;
    }

    // This runs automatically when the "Concert" property is set via navigation
    partial void OnConcertChanged(Concert value)
    {
        if (value == null) return;

        // Use our Service helpers to turn database strings into UI lists
        Performers.Clear();
        var performerList = _concertService.ConvertStringToList(value.Performers, ',');
        foreach (var p in performerList) Performers.Add(p);

        MediaFiles.Clear();
        var mediaList = _concertService.ConvertStringToList(value.MediaPaths, ';');
        foreach (var m in mediaList) MediaFiles.Add(m);
    }

    [RelayCommand]
    private async Task EditConcertAsync()
    {
        // Navigate to the Add/Edit page, passing the current concert
        await Shell.Current.GoToAsync(nameof(AddConcertPage), new Dictionary<string, object>
        {
            { "SelectedConcert", Concert }
        });
    }

    [RelayCommand]
    private async Task DeleteConcertAsync()
    {
        var confirm = await Shell.Current.DisplayAlert("Delete", "Are you sure you want to delete this memory?", "Yes", "No");

        if (confirm)
        {
            await _concertService.DeleteConcertAsync(Concert);
            await Shell.Current.GoToAsync(".."); // Go back to the list
        }
    }
}