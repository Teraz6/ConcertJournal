using ConcertJournal.Models;
using ConcertJournal.Services;
using System.Collections.ObjectModel;

namespace ConcertJournal.Views;

public partial class ConcertListPage : ContentPage
{

    private List<Concert> allConcerts = new();
    private List<Concert> _selectedConcerts = new();
    private bool sortAscending = true;
    public ObservableCollection<Concert> Concerts { get; set; } = new();

    public ConcertListPage()
	{
		InitializeComponent();
        LoadConcerts();

        EventBus.ConcertCreated += async () =>
        {
            allConcerts = await App.Database.GetConcertsAsync();
            ApplySearchAndSort(defaultSort: true);
        };

        EventBus.ConcertUpdated += async () =>
        {
            allConcerts = await App.Database.GetConcertsAsync();
            ApplySearchAndSort(defaultSort: true);
        };

        ImportServices.ConcertsImported += async () =>
        {
            allConcerts = await App.Database.GetConcertsAsync();
            ApplySearchAndSort(defaultSort: true);
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Load data from your SQLite database
            allConcerts = await App.Database.GetConcertsAsync();

            Concerts.Clear();
            foreach (var concert in allConcerts)
                Concerts.Add(concert);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load concerts: {ex.Message}", "OK");
        }

        SortPicker.SelectedIndex = 0;
    }

    private async Task LoadConcerts()
    {
        allConcerts = await App.Database.GetConcertsAsync();

        // Default = newest created first
        ApplySearchAndSort(defaultSort: true);
    }

    private async void OnUpdateClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Concert selectedConcert)
        {
            // Navigate to AddConcertPage with the existing concert
            await Navigation.PushAsync(new AddConcertPage(selectedConcert));
        }
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Concert concert)
        {
            bool confirm = await DisplayAlert(
                "Delete Concert",
                $"Are you sure you want to delete '{concert.EventTitle}'?",
                "Yes",
                "No");

            if (confirm)
            {
                await App.Database.DeleteConcertAsync(concert);
                await LoadConcerts(); // Refresh the list
            }
        }
    }

    //When clicked on Details button it will take you to Details page
    private async void OnDetailsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is Concert concert)
        {
            await Navigation.PushAsync(new ConcertDetailsPage(concert));
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplySearchAndSort(e.NewTextValue);
    }

    private void OnSortPickerChanged(object sender, EventArgs e)
    {
        if (sender is not Picker picker || picker.SelectedItem is null)
            return;

        string selected = picker.SelectedItem.ToString();
        string searchText = SearchBar?.Text ?? string.Empty;

        switch (selected)
        {
            case "Oldest By Year":
                sortAscending = true;
                ApplySearchAndSort(searchText, sortByDate: true, ascending: true);
                break;

            case "Newest By Year":
                sortAscending = false;
                ApplySearchAndSort(searchText, sortByDate: true, ascending: false);
                break;

            case "Default":
            default:
                ApplySearchAndSort(searchText, defaultSort: true);
                break;
        }
    }

    private void ApplySearchAndSort(string searchText = "", bool sortByDate = true, bool ascending = true, bool defaultSort = false)
    {
        var filtered = allConcerts;

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            filtered = filtered
                .Where(c => c.EventTitle.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                         || c.Performers.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Sorting
        if (defaultSort)
        {
            // Default = newest created first
            filtered = filtered.OrderByDescending(c => c.Id).ToList();
        }
        else if (sortByDate)
        {
            filtered = ascending
                ? filtered.OrderBy(c => c.Date).ToList()
                : filtered.OrderByDescending(c => c.Date).ToList();
        }

        // Update UI
        ConcertListView.ItemsSource = filtered;
    }

    private async void OnConcertSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Concert selected)
        {
            await Navigation.PushAsync(new ConcertDetailsPage(selected));
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView cv)
        {
            _selectedConcerts = cv.SelectedItems.Cast<Concert>().ToList();
            DeleteSelectedButton.IsVisible = _selectedConcerts.Count > 0;
        }
    }

    private async void OnDeleteSelectedClicked(object sender, EventArgs e)
    {
        if (_selectedConcerts.Count == 0)
        {
            await DisplayAlert("No selection", "Please select at least one concert.", "OK");
            return;
        }

        bool confirm = await DisplayAlert(
            "Delete",
            $"Are you sure you want to delete {_selectedConcerts.Count} selected concerts?",
            "Yes", "No");

        if (!confirm)
            return;

        // Delete all selected concerts in parallel
        var deleteTasks = _selectedConcerts.Select(concert => App.Database.DeleteConcertAsync(concert));
        await Task.WhenAll(deleteTasks);

        _selectedConcerts.Clear();
        DeleteSelectedButton.IsVisible = false;

        await LoadConcerts(); // refresh list
    }

}