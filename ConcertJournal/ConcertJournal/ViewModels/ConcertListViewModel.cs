using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConcertJournal.Models;
using ConcertJournal.ServiceInterface;
using ConcertJournal.Views;
using System.Collections.ObjectModel;

namespace ConcertJournal.ViewModels;

public partial class ConcertListViewModel : ObservableObject
{
    private readonly IConcertService _concertService;

    // Use a HashSet or List to track selected IDs for performance
    private readonly HashSet<int> _selectedConcertIds = new();

    // Data Collections
    public ObservableCollection<Concert> Concerts { get; } = new();
    public ObservableCollection<Concert> SelectedItems { get; } = new();

    // Paging & State
    private const int PageSize = 15;
    private int _currentPage = 0;
    private bool _hasMoreItems = true;

    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private string _searchText = string.Empty;
    [ObservableProperty] private string _totalConcertsText = "Loading...";
    [ObservableProperty] private bool _isFilterPresented;
    [ObservableProperty] private string _filterIcon;
    [ObservableProperty] private bool _hasSelection;

    // Sorting
    private const string SortPreferenceKey = "SortRadioGroup";
    [ObservableProperty] private string _selectedSort;

    // 1. Constructor with Dependency Injection
    public ConcertListViewModel(IConcertService concertService)
    {
        _concertService = concertService;
        _selectedSort = Preferences.Get(SortPreferenceKey, "Default");
        UpdateFilterIcon();

        //This forces the UI to refresh "HasSelection" whenever the collection changes
        SelectedItems.CollectionChanged += (s, e) => UpdateSelection();
    }

    [RelayCommand]
    public async Task RefreshAllAsync()
    {
        //// EMERGENCY DEBUG:
        //var rawDbPath = Path.Combine(FileSystem.AppDataDirectory, "concerts.db3");
        //System.Diagnostics.Debug.WriteLine($"DB PATH: {rawDbPath}");

        _currentPage = 0;
        _hasMoreItems = true;
        Concerts.Clear();
        await LoadMoreConcertsAsync();
        await UpdateTotalCountAsync();
    }

    [RelayCommand]
    public async Task LoadMoreConcertsAsync()
    {
        if (IsLoading || !_hasMoreItems) return;

        IsLoading = true;
        try
        {
            int skip = _currentPage * PageSize;

            var newConcerts = await _concertService.GetConcertsPagedAsync(
                skip,
                PageSize,
                SelectedSort,
                SearchText);

            if (newConcerts.Count < PageSize) _hasMoreItems = false;

            foreach (var concert in newConcerts)
                Concerts.Add(concert);

            _currentPage++;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task UpdateTotalCountAsync()
    {
        // 3. Service handles fetching all for the count
        var all = await _concertService.GetConcertsPagedAsync(0, 9999, "Default", "");
        TotalConcertsText = all?.Count > 0 ? $"Total concerts: {all.Count}" : "No concert data available.";
    }

    [RelayCommand]
    private async Task SearchAsync() => await RefreshAllAsync();

    [RelayCommand]
    private async Task SortChanged(string newSortValue)
    {
        if (string.IsNullOrEmpty(newSortValue)) return;

        SelectedSort = newSortValue;
        Preferences.Set(SortPreferenceKey, newSortValue);
        await RefreshAllAsync();
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        if (SelectedItems.Count == 0) return;

        bool confirm = await Shell.Current.DisplayAlert("Delete",
            $"Delete {SelectedItems.Count} concerts?", "Yes", "No");

        if (!confirm) return;

        // Snapshot for deletion
        var itemsToDelete = SelectedItems.Cast<Concert>().ToList();
        foreach (var concert in itemsToDelete)
        {
            await _concertService.DeleteConcertAsync(concert);
            Concerts.Remove(concert);
        }

        ClearSelection();
        await UpdateTotalCountAsync();
    }

    [RelayCommand]
    private async Task GoAddConcertAsync()
    {
        await Shell.Current.GoToAsync(nameof(AddConcertPage));
    }

    // --- UI & Selection Logic ---

    [RelayCommand]
    private void ToggleFilter()
    {
        IsFilterPresented = !IsFilterPresented;
        UpdateFilterIcon();
    }

    private void UpdateFilterIcon()
    {
        bool isDark = App.Current.RequestedTheme == AppTheme.Dark;
        string colorSuffix = isDark ? "white" : "black";

        string iconName = IsFilterPresented ? "up" : "filter";

        FilterIcon = $"{iconName}_{colorSuffix}.png";
    }

    [RelayCommand]
    private void UpdateSelection() => HasSelection = SelectedItems.Count > 0;

    [RelayCommand]
    private void UnselectAll()
    {
        SelectedItems.Clear();
        HasSelection = false;
    }

    [RelayCommand]
    private async Task CardClicked(Concert concert)
    {
        if (concert == null) return;

        // 1. If we are in selection mode, just toggle the item
        if (HasSelection)
        {
            ToggleSelection(concert);
            return;
        }

        // 2. Otherwise, navigate to details
        // Tip: Use a 'IsBusy' check to prevent double-navigation crashes
        await Shell.Current.GoToAsync(nameof(ConcertDetailsPage), new Dictionary<string, object>
    {
        { "Concert", concert }
    });
    }

    [RelayCommand]
    private void CardLongPress(Concert concert)
    {
        if (concert == null) return;

        // Provide physical feedback
        try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }

        // Always toggle the selection on long press
        ToggleSelection(concert);
    }

    private void ToggleSelection(Concert concert)
    {
        if (concert == null) return;

        concert.IsSelected = !concert.IsSelected;

        if (concert.IsSelected)
            SelectedItems.Add(concert);
        else
            SelectedItems.Remove(concert);

        UpdateSelection();
    }

    [RelayCommand]
    private void ClearSelection()
    {
        var items = SelectedItems.Cast<Concert>().ToList();

        foreach (var item in items)
        {
            item.IsSelected = false;
        }

        SelectedItems.Clear();
        UpdateSelection();
    }
}