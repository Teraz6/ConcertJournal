using ConcertJournal.Models;
using ConcertJournal.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ConcertJournal.Views;

public partial class ConcertListPage : ContentPage
{

    private List<Concert> allConcerts = new();
    private List<Concert> _selectedConcerts = new();
    
    public ObservableCollection<Concert> Concerts { get; set; } = new(); // Items currently displayed
    private List<Concert> allConcertsLoaded = new();  // raw paged items from DB

    //Sorting
    private bool sortAscending = true;
    private const string SortPreferenceKey = "SortPickerSelection";

    //For optimizing loading
    private const int PageSize = 10;
    private int _currentPage = 0;
    private bool _isLoadingMore = false;
    private bool _hasMoreItems = true;

    public ConcertListPage()
	{
		InitializeComponent();


        EventBus.ConcertCreated += async () => await RefreshAllAsync();
        EventBus.ConcertUpdated += async () => await RefreshAllAsync();
        ImportServices.ConcertsImported += async () => await RefreshAllAsync();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        BindingContext = this;

        try
        {
            _currentPage = 0;
            _hasMoreItems = true;
            Concerts.Clear();
            await LoadMoreConcertsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load concerts: {ex.Message}", "OK");
        }

        string savedSort = Preferences.Get(SortPreferenceKey, "Default");

        if (SortPicker.Items.Contains(savedSort))
        {
            SortPicker.SelectedItem = savedSort;
        }
        else
        {
            SortPicker.SelectedItem = "Default";
        }
        await ApplySortFromPreferenceAsync(savedSort);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        EventBus.ConcertCreated -= async () => await RefreshAllAsync();
        EventBus.ConcertUpdated -= async () => await RefreshAllAsync();
        ImportServices.ConcertsImported -= async () => await RefreshAllAsync();
    }   

    private async Task RefreshAllAsync()
    {
        _currentPage = 0;
        _hasMoreItems = true;
        Concerts.Clear();
        await LoadMoreConcertsAsync();
    }

    //Not used. Loads entire list and is bad for performance
    private async Task LoadConcertsAsync()
    {
        allConcerts = await App.Database.GetConcertsAsync();
        string savedSort = Preferences.Get(SortPreferenceKey, "Default");
        await ApplySortFromPreferenceAsync(savedSort);
    }

    private async Task LoadMoreConcertsAsync()
    {
        if (_isLoadingMore || !_hasMoreItems) return;
        _isLoadingMore = true;

        try
        {
            int skip = _currentPage * PageSize;
            var newConcerts = await App.Database.GetConcertsPagedAsync(skip, PageSize);
            _currentPage++;

            if (newConcerts.Count == 0)
            {
                _hasMoreItems = false;
                return;
            }

            allConcertsLoaded.AddRange(newConcerts);

            // Apply current filter/sort
            ApplySearchAndSort(SearchBar?.Text ?? string.Empty);

            if (newConcerts.Count < PageSize)
                _hasMoreItems = false;
        }
        finally
        {
            _isLoadingMore = false;
        }

    }

    private async void OnRemainingItemsThresholdReached(object sender, EventArgs e)
    {
        if (_isLoadingMore) return;
        await LoadMoreConcertsAsync();
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
                await RefreshAllAsync(); // Refresh the list
            }
        }
    }

    //When clicked on Details button it will take you to Details page
    private async void OnDetailsClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is Concert concert)
        {
            await Navigation.PushAsync(new ConcertDetailsPage(concert));
        }
    }

    private async Task ApplySortFromPreferenceAsync(string selected)
    {
        string searchText = SearchBar?.Text ?? string.Empty;

        switch (selected)
        {
            case "Oldest By Date":
                sortAscending = true;
                ApplySearchAndSort(searchText, sortByDate: true, ascending: true);
                break;

            case "Newest By Date":
                sortAscending = false;
                ApplySearchAndSort(searchText, sortByDate: true, ascending: false);
                break;

            case "Default":
            default:
                ApplySearchAndSort(searchText, defaultSort: true);
                break;
        }
        await Task.CompletedTask;
    }

    // Applies search filter and sorting to the allConcerts list and updates the UI. ToDo: Add small delay for better performance
    private void ApplySearchAndSort(string searchText = "", bool sortByDate = true, bool ascending = true, bool defaultSort = false)
    {
        var filtered = allConcertsLoaded;

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            filtered = filtered
                .Where(c => c.EventTitle.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                         || c.Performers.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        if (defaultSort)
            filtered = filtered.OrderByDescending(c => c.Id).ToList();
        else if (sortByDate)
            filtered = ascending
                ? filtered.OrderBy(c => c.Date).ToList()
                : filtered.OrderByDescending(c => c.Date).ToList();

        Concerts.Clear();
        foreach (var c in filtered)
            Concerts.Add(c);
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string savedSort = Preferences.Get(SortPreferenceKey, "Default");
        _ = ApplySortFromPreferenceAsync(savedSort);
    }

    private void OnSortPickerChanged(object sender, EventArgs e)
    {
        if (sender is not Picker picker || picker.SelectedItem is null)
            return;

        string selected = picker.SelectedItem.ToString();

        // Save preference
        Preferences.Set(SortPreferenceKey, selected);

        // Apply sorting immediately
        _ = ApplySortFromPreferenceAsync(selected);
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

            bool hasSelection = _selectedConcerts.Count > 0;
            DeleteSelectedButton.IsVisible = hasSelection;
            UnselectAllButton.IsVisible = hasSelection;
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
        UnselectAllButton.IsVisible = false;

        await RefreshAllAsync(); // refresh list
    }

    private void OnUnselectAllClicked(object sender, EventArgs e)
    {
        _selectedConcerts.Clear();

        // Clear selection in CollectionView
        ConcertListView.SelectedItems.Clear();

        // Hide both buttons when no selection
        DeleteSelectedButton.IsVisible = false;
        UnselectAllButton.IsVisible = false;
    }

}