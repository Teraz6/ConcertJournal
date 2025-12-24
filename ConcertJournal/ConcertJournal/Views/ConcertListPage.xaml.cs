using ConcertJournal.Models;
using ConcertJournal.Services;
using InputKit.Shared.Controls;
using System.Collections.ObjectModel;
using System.Diagnostics;
using UraniumUI.ViewExtensions;
using RadioButton = InputKit.Shared.Controls.RadioButton;

namespace ConcertJournal.Views;

public partial class ConcertListPage : UraniumUI.Pages.UraniumContentPage
{

    private List<Concert> allConcerts = new();
    private List<Concert> _selectedConcerts = new();
    
    public ObservableCollection<Concert> Concerts { get; set; } = new(); // Items currently displayed
    private List<Concert> allConcertsLoaded = new();  // raw paged items from DB

    //Sorting
    //private readonly bool sortAscending = true;
    private const string SortPreferenceKey = "SortRadioGroup";

    //For optimizing loading
    private const int PageSize = 15;
    private int _currentPage = 0;
    private bool _isLoadingMore = false;
    private bool _hasMoreItems = true;

    //Hold timer for selection. Hold duration 0.75s
    private bool _isPressing;
    private bool _selectionTriggered;
    private const int HoldDuration = 750;

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
        LoadTotalConcerts();

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
            rb.IsChecked = rb.Value?.ToString() == savedSort;
        }

        await ApplySortFromPreferenceAsync(savedSort);

        // ⭐ NEW FIX: Explicitly update button visibility based on the CollectionView's current state.
        // The SelectedItems collection is often reset by MAUI upon returning, but checking ensures correctness.
        bool hasSelection = ConcertListView.SelectedItems.Count > 0;
        DeleteSelectedButton.IsVisible = hasSelection;
        UnselectAllButton.IsVisible = hasSelection;
        SelectionButtonBackground.IsVisible = hasSelection;
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

    //List sorting
    private async Task ApplySortFromPreferenceAsync(string selected)
    {
        _currentPage = 0;
        _hasMoreItems = true;
        Concerts.Clear();
        await LoadMoreConcertsAsync();
    }

    //Searchbar result loading
    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        _currentPage = 0;
        _hasMoreItems = true;
        Concerts.Clear();
        await LoadMoreConcertsAsync();
    }

    // This will fire whenever a radio button is selected
    private void OnSortRadioSelectedChanged(object? sender, EventArgs e)
    {
        string selectedValue = SortRadioGroup.SelectedItem?.ToString() ?? "Default";

        // Save preference
        Preferences.Set(SortPreferenceKey, selectedValue);

        // Apply sorting
        _ = ApplySortFromPreferenceAsync(selectedValue);
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView cv)
        {
            // Update the local list of selected concerts
            _selectedConcerts = cv.SelectedItems.Cast<Concert>().ToList(); // This is good

            bool hasSelection = _selectedConcerts.Count > 0;
            // The Visibility properties are updated here
            DeleteSelectedButton.IsVisible = hasSelection;
            UnselectAllButton.IsVisible = hasSelection;
            SelectionButtonBackground.IsVisible = hasSelection;
        }
    }

    private async void OnDeleteSelectedClicked(object sender, EventArgs e)
    {
        // ⭐ FIX: Get the currently selected items directly from the CollectionView
        var itemsToDelete = ConcertListView.SelectedItems.Cast<Concert>().ToList();

        if (itemsToDelete.Count == 0)
        {
            await DisplayAlert("No selection", "Please select at least one concert.", "OK");

            // Ensure buttons are hidden if the list is empty (in case they were stale)
            DeleteSelectedButton.IsVisible = false;
            UnselectAllButton.IsVisible = false;
            SelectionButtonBackground.IsVisible = false;

            return;
        }

        bool confirm = await DisplayAlert(
        "Delete",
        $"Are you sure you want to delete {itemsToDelete.Count} selected concerts?", // Use itemsToDelete.Count
        "Yes", "No");

        if (!confirm)
            return;

        // Delete all selected concerts in parallel
        var deleteTasks = itemsToDelete.Select(concert => App.Database.DeleteConcertAsync(concert));
        await Task.WhenAll(deleteTasks);

        // Clear the CollectionView selection state (This will trigger OnSelectionChanged)
        ConcertListView.SelectedItems.Clear();

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

    //Filter icon open/close button
    private void OnBackdropClicked(object sender, EventArgs e)
    {
        FilterBackdrop.IsPresented = !FilterBackdrop.IsPresented;
        if (FilterBackdrop.IsPresented)
        {
            FilterButton.SetDynamicResource(Image.SourceProperty, "UpIcon");
        }
        else
        {
            FilterButton.SetDynamicResource(Image.SourceProperty, "FilterIcon");
        }
    }

    //Filter close button
    private void OnCloseFilterClicked(object sender, EventArgs e)
    {
        FilterBackdrop.IsPresented = false;
    }

    private async void LoadTotalConcerts()
    {
         var concerts = await App.Database.GetConcertsAsync();

        if (concerts == null || concerts.Count == 0)
        {
            TotalConcertsLabel.Text = "No concert data available.";
            return;
        }

        // Total concerts
        TotalConcertsLabel.Text = $"Total concerts: {concerts.Count}";
    }

    //Options icon
    private async void OnOptionsClicked(object sender, EventArgs e)
    {
        string editOption = "Edit";
        string deleteOption = "Delete";

        var options = new string[] { editOption, deleteOption } ;

        if (sender is ImageButton button && button.BindingContext is Concert concert)
        {
            //Use event title as sheet title
            string selectedOption = await DisplayActionSheet(concert.EventTitle, "Cancel", null, options);

            if (selectedOption == editOption)
            {
                await Navigation.PushAsync(new AddConcertPage(concert));
            }
       
            else if (selectedOption == deleteOption)
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
    }

    // 1. PRESSED: Start the timer
    private async void OnCardPressed(object sender, EventArgs e)
    {
        _isPressing = true;
        _selectionTriggered = false;

        if (sender is Button btn && btn.BindingContext is Concert concert)
        {
            // Wait for 500ms
            await Task.Delay(HoldDuration);

            // If still pressing after 500ms, trigger SELECTION
            if (_isPressing)
            {
                _selectionTriggered = true;

                // Vibrate for feedback (optional, requires permissions, wrap in try-catch)
                try { HapticFeedback.Perform(HapticFeedbackType.LongPress); } catch { }

                // Perform Selection Logic
                if (ConcertListView.SelectedItems.Contains(concert))
                {
                    ConcertListView.SelectedItems.Remove(concert);
                }
                else
                {
                    ConcertListView.SelectedItems.Add(concert);
                }
            }
        }
    }

    // 2. RELEASED: Stop the timer state
    private void OnCardReleased(object sender, EventArgs e)
    {
        _isPressing = false;
    }

    // 3. CLICKED: Navigate ONLY if it wasn't a long press
    private async void OnCardClicked(object sender, EventArgs e)
    {
        if (_selectionTriggered)
        {
            // If we just finished a long press, do nothing (don't navigate)
            return;
        }

        if (sender is Button btn && btn.BindingContext is Concert concert)
        {
            // Standard Navigation
            await Navigation.PushAsync(new ConcertDetailsPage(concert));

            // Clear visual selection if it happened accidentally
            if (ConcertListView.SelectedItems.Contains(concert))
            {
                ConcertListView.SelectedItems.Remove(concert);
            }
        }
    }

    //TODO: make selection buttons that appear for unselecting all reset when going to details page. Happens on android not windows.
}