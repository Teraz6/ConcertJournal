using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ConcertJournal.Messages;
using ConcertJournal.Models;
using ConcertJournal.ServiceInterface;
using ConcertJournal.Views;
using System.Collections.ObjectModel;

namespace ConcertJournal.ViewModels;

[QueryProperty(nameof(Concert), "Concert")]
public partial class ConcertDetailsViewModel : ObservableObject, IRecipient<ConcertUpdatedMessage>
{
    private readonly IConcertService _concertService;

    [ObservableProperty]
    public partial Concert? Concert { get; set; } = null;

    // These collections will hold the parsed data for the UI
    public ObservableCollection<string> Performers { get; } = [];
    public ObservableCollection<string> MediaFiles { get; } = [];

    public ConcertDetailsViewModel(IConcertService concertService)
    {
        _concertService = concertService;
        WeakReferenceMessenger.Default.Register(this);
    }

    partial void OnConcertChanged(Concert? value)
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

    public async void Receive(ConcertUpdatedMessage message)
    {
        if (Concert != null)
        {
            // Fetch fresh data from DB
            var updated = await _concertService.GetConcertByIdAsync(Concert.Id);

            // Re-assigning this triggers OnConcertChanged automatically!
            Concert = updated;
        }
    }

    //Go back command
    [RelayCommand]
    private static async Task GoBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }

    //Edit concert command
    [RelayCommand]
    private async Task EditConcertAsync()
    {
        // Navigate to the Add/Edit page, passing the current concert
        await Shell.Current.GoToAsync(nameof(AddConcertPage), new Dictionary<string, object>
        {
            { "SelectedConcert", Concert!}
        });
    }

    //Delete concert command
    [RelayCommand]
    private async Task DeleteConcertAsync()
    {
        var confirm = await Shell.Current.DisplayAlertAsync("Delete", "Are you sure you want to delete this memory?", "Yes", "No");

        if (confirm)
        {
            await _concertService.DeleteConcertAsync(Concert!);
            await Shell.Current.GoToAsync(".."); // Go back to the list
        }
    }

    [RelayCommand]
    private async Task PerformerTappedAsync()
    {
        if (string.IsNullOrWhiteSpace(Concert?.Performers)) return;

        // 1. Split the string into a list (using comma as separator)
        var performerList = _concertService.ConvertStringToList(Concert.Performers, ',');

        if (performerList.Count == 0) return;

        string selectedPerformer;

        if (performerList.Count == 1)
        {
            // Only one performer, go straight to the page
            selectedPerformer = performerList[0];
        }
        else
        {
            // 2. Multiple performers: Let the user choose
            selectedPerformer = await Shell.Current.DisplayActionSheetAsync(
                "Select Performer",
                "Cancel",
                null,
                performerList.ToArray());

            // If they click 'Cancel' or outside the box, stop here
            if (selectedPerformer == "Cancel" || string.IsNullOrEmpty(selectedPerformer)) return;
        }

        // 3. Navigate to the Details Page
        await Shell.Current.GoToAsync(nameof(PerformerDetailsPage), new Dictionary<string, object>
    {
        { "PerformerName", selectedPerformer }
    });
    }
}