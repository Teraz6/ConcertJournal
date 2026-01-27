using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ConcertJournal.Messages;
using ConcertJournal.Models;
using ConcertJournal.ServiceInterface;
using System.Collections.ObjectModel;

namespace ConcertJournal.ViewModels;

[QueryProperty(nameof(Concert), "SelectedConcert")]
public partial class AddConcertViewModel : ObservableObject
{
    private readonly IConcertService _concertServices;
    private readonly IImageService _imageServices;

    // The backing property for the QueryProperty
    [ObservableProperty]
    public partial Concert? Concert { get; set; }

    // --- UI State Properties ---
    [ObservableProperty] public partial string PageTitle { get; set; } = "New Event";
    [ObservableProperty] public partial string SaveButtonText { get; set; } = "Create";

    // --- Form Properties ---
    [ObservableProperty] public partial string EventTitle { get; set; } = string.Empty;
    [ObservableProperty] public partial string Venue { get; set; } = string.Empty;
    [ObservableProperty] public partial string Country { get; set; } = string.Empty;
    [ObservableProperty] public partial string City { get; set; } = string.Empty;
    [ObservableProperty] public partial string Notes { get; set; } = string.Empty;
    [ObservableProperty] public partial DateTime Date { get; set; } = DateTime.Today;
    [ObservableProperty] public partial double Rating { get; set; }
    [ObservableProperty] public partial string PerformerInput { get; set; } = string.Empty;

    public ObservableCollection<string> Performers { get; } = new();
    public ObservableCollection<string> MediaFiles { get; } = new();

    // 1. Constructor Injection: The service is provided by MAUI's Dependency Injection container
    public AddConcertViewModel(IConcertService concertServices, IImageService imageServices)
    {
        _concertServices = concertServices;
        _imageServices = imageServices;

        // Load defaults for new entries
        Country = Preferences.Get("DefaultCountry", string.Empty);
        City = Preferences.Get("DefaultCity", string.Empty);
    }

    // 2. Automated Logic: This runs as soon as 'SelectedConcert' is passed via Shell Navigation
    partial void OnConcertChanged(Concert? value)
    {
        if (value == null) return;

        PageTitle = "Edit Event";
        SaveButtonText = "Save";

        // Map values from the model to the VM properties
        EventTitle = value.EventTitle ?? "";
        Venue = value.Venue ?? "";
        Country = value.Country ?? "";
        City = value.City ?? "";
        Notes = value.Notes ?? "";
        Date = (DateTime)value.Date!;
        Rating = value.Rating;

        // Load collections
        Performers.Clear();
        var performerList = _concertServices.ConvertStringToList(value.Performers, ',');
        foreach (var p in performerList) Performers.Add(p);

        MediaFiles.Clear();
        var mediaList = _concertServices.ConvertStringToList(value.MediaPaths, ';');
        foreach (var m in mediaList) MediaFiles.Add(m);
    }

    // --- Commands ---

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(EventTitle))
        {
            await Shell.Current.DisplayAlertAsync("Error", "Event Title is required.", "OK");
            return;
        }

        // Use the existing concert object if editing, or create a new one
        var concertToSave = Concert ?? new Concert();

        concertToSave.EventTitle = EventTitle;
        concertToSave.Venue = Venue;
        concertToSave.Country = Country;
        concertToSave.City = City;
        concertToSave.Notes = Notes;
        concertToSave.Date = Date;
        concertToSave.Rating = Rating;

        concertToSave.Performers = _concertServices.ConvertListToString(Performers, ", ");
        concertToSave.MediaPaths = _concertServices.ConvertListToString(MediaFiles, ";");

        await _concertServices.SaveConcertAsync(concertToSave);

        WeakReferenceMessenger.Default.Send(new ConcertUpdatedMessage());

        bool isNew = Concert == null;

        //Reset the form for new entry
        if (isNew)
        {
            EventTitle = string.Empty;
            Venue = string.Empty;
            Country = Preferences.Get("DefaultCountry", string.Empty);
            City = Preferences.Get("DefaultCity", string.Empty);
            Notes = string.Empty;
            Date = DateTime.Today;
            Rating = 0;
            Performers.Clear();
            MediaFiles.Clear();
        }

        await Shell.Current.DisplayAlertAsync("Success", isNew ? "Concert created!" : "Concert updated!", "OK");
  
        // Go back to the previous page
        if (!isNew)
        {
            await Shell.Current.GoToAsync("..");
        }
            
    }

    [RelayCommand]
    private static async Task CancelAsync() => await Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task AddPerformerAsync()
    {
        var name = PerformerInput?.Trim();
        if (string.IsNullOrEmpty(name))
        { 
            await Shell.Current.DisplayAlertAsync("Empty", "Please enter a performer name.", "OK");
            return;
        }

        if (Performers.Contains(name))
        {
            await Shell.Current.DisplayAlertAsync("Duplicate", "Performer already added.", "OK");
            return;
        }

        Performers.Add(name);
        PerformerInput = string.Empty;
    }

    [RelayCommand]
    private void RemovePerformer(string name) => Performers.Remove(name);

    [RelayCommand]
    private async Task AddImageAsync()
    {
        // 1. Ask the service to handle the "Work" of picking files
        var paths = await _imageServices.PickImagesAsync("Select Concert Photos");

        // 2. The ViewModel only handles the "Result"
        foreach (var path in paths)
        {
            if (!MediaFiles.Contains(path))
            {
                MediaFiles.Add(path);
            }
        }
    }

    [RelayCommand]
    private void RemoveMedia(string path) => MediaFiles.Remove(path);
}