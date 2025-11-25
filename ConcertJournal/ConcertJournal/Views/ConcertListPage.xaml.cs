using ConcertJournal.Models;
using ConcertJournal.Services;
using InputKit.Shared.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using RadioButton = InputKit.Shared.Controls.RadioButton;

namespace ConcertJournal.Views;

public partial class ConcertListPage : UraniumUI.Pages.UraniumContentPage
{

    private List<Concert> allConcerts = new();
    private List<Concert> _selectedConcerts = new();
    
    public ObservableCollection<Concert> Concerts { get; set; } = new(); // Items currently displayed
    private List<Concert> allConcertsLoaded = new();  // raw paged items from DB

    //Sorting
    private bool sortAscending = true;
    private const string SortPreferenceKey = "SortRadioGroup";

    //For optimizing loading
    private const int PageSize = 15;
    private int _currentPage = 0;
    private bool _isLoadingMore = false;
    private bool _hasMoreItems = true;

    public ConcertListPage()
	{
		InitializeComponent();


        EventBus.ConcertCreated += async () => await RefreshAllAsync();
        EventBus.ConcertUpdated += async () => await RefreshAllAsync();
        ImportServices.ConcertsImported += async () => await RefreshAllAsync();

        // Subscribe to SelectedItemChanged
        SortRadioGroup.SelectedItemChanged += OnSortRadioSelectedChanged;

        // Restore previously saved selection
        string savedSort = Preferences.Get(SortPreferenceKey, "Default");
        foreach (var rb in SortRadioGroup.Children.OfType<RadioButton>())
        {
            rb.IsChecked = rb.Value?.ToString() == savedSort;
        }
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

        foreach (var rb in SortRadioGroup.Children.OfType<RadioButton>())
        {
            rb.IsChecked = rb.Text == savedSort;
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

    private async Task LoadMoreConcertsAsync()
    {
        if (_isLoadingMore || !_hasMoreItems) return;
        _isLoadingMore = true;

        try
        {
            int skip = _currentPage * PageSize;
            string sortBy = "Default";
            bool ascending = true;
            string searchText = SearchBar?.Text ?? string.Empty;

            string selectedValue = SortRadioGroup.SelectedItem?.ToString() ?? "Default";

            if (selectedValue == "OldestByDate")
            {
                sortBy = "OldestByDate";
                ascending = true;
            }
            else if (selectedValue == "NewestByDate")
            {
                sortBy = "NewestByDate";
                ascending = false;
            }

            var newConcerts = await App.Database.GetConcertsPagedAsync(skip, PageSize, sortBy, ascending, searchText);
            _currentPage++;

            if (newConcerts.Count == 0)
            {
                _hasMoreItems = false;
                return;
            }

            foreach (var c in newConcerts)
                Concerts.Add(c);

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
        if (sender is ImageButton button && button.CommandParameter is Concert selectedConcert)
        {
            // Navigate to AddConcertPage with the existing concert
            await Navigation.PushAsync(new AddConcertPage(selectedConcert));
        }
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
        _currentPage = 0;
        _hasMoreItems = true;
        Concerts.Clear();
        await LoadMoreConcertsAsync();
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _currentPage = 0;
        _hasMoreItems = true;
        Concerts.Clear();
        await LoadMoreConcertsAsync();
    }

    // This will fire whenever a radio button is selected
    private void OnSortRadioSelectedChanged(object sender, EventArgs e)
    {
        string selectedValue = SortRadioGroup.SelectedItem?.ToString() ?? "Default";

        // Save preference
        Preferences.Set(SortPreferenceKey, selectedValue);

        // Apply sorting
        _ = ApplySortFromPreferenceAsync(selectedValue);
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
            SelectionButtonBackground.IsVisible = hasSelection;
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
        SelectionButtonBackground.IsVisible = false;

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
        SelectionButtonBackground.IsVisible = false;
    }
}