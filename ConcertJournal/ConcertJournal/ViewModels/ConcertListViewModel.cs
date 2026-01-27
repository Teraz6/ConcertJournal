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

    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial string TotalConcertsText { get; set; } = "Loading...";
    [ObservableProperty] public partial bool IsFilterPresented { get;set; }
    [ObservableProperty] public partial string FilterIcon { get; set; } = "filter_black.png";
    [ObservableProperty] public partial bool HasSelection { get; set; }

    // Search
    [ObservableProperty] public partial string SearchText { get; set; } = string.Empty;
    private CancellationTokenSource? _searchCancellation;

    // Sorting
    private const string SortPreferenceKey = "SortRadioGroup";
    [ObservableProperty] public partial string SelectedSort { get; set; }

    // 1. Constructor with Dependency Injection
    public ConcertListViewModel(IConcertService concertService)
    {
        _concertService = concertService;

        SelectedSort = Preferences.Get(SortPreferenceKey, "LatestAdded");

        UpdateFilterIcon();

        //This forces the UI to refresh "HasSelection" whenever the collection changes
        SelectedItems.CollectionChanged += (s, e) => UpdateSelection();

        // 2. Start the initial load
        _ = RefreshAllAsync();
    }

    [RelayCommand]
    public async Task RefreshAllAsync()
    {
        //// EMERGENCY DEBUG:
        //var rawDbPath = Path.Combine(FileSystem.AppDataDirectory, "concerts.db3");
        //System.Diagnostics.Debug.WriteLine($"DB PATH: {rawDbPath}");
        if (IsLoading) return;

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

            // If we got fewer items than requested, we've hit the end
            if (newConcerts.Count < PageSize)
                _hasMoreItems = false;

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
        // Pass the current SearchText to ensure the count matches what is on screen
        int count = await _concertService.GetConcertCountAsync(SearchText);

        TotalConcertsText = count switch
        {
            0 => string.IsNullOrWhiteSpace(SearchText)
                 ? "No concerts saved yet."
                 : "No matches found.",
            1 => "1 concert found.",
            _ => $"{count} concerts found."
        };
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

        bool confirm = await Shell.Current.DisplayAlertAsync("Delete",
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
        bool isDark = App.Current!.RequestedTheme == AppTheme.Dark;
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
        if (concert == null || IsLoading) return;

        // 1. If we are in selection mode, just toggle the item
        if (HasSelection)
        {
            ToggleSelection(concert);
            return;
        }

        // 2. Otherwise, navigate to details
        // It's a good idea to set a small "IsBusy" flag to prevent double-taps 
        // from opening two pages.
        await Shell.Current.GoToAsync(nameof(ConcertDetailsPage), new Dictionary<string, object>
    {
        { "Concert", concert }
    });
    }

    [RelayCommand]
    private void CardLongPress(Concert concert)
    {
        if (concert == null) return;

        // Haptic feedback provides a professional feel
        try { HapticFeedback.Default.Perform(HapticFeedbackType.LongPress); } catch { }

        // Toggle selection
        ToggleSelection(concert);
    }

    private void ToggleSelection(Concert concert)
    {
        if (concert == null) return;

        concert.IsSelected = !concert.IsSelected;

        if (concert.IsSelected)
        {
            if (!SelectedItems.Contains(concert))
                SelectedItems.Add(concert);
        }
        else
        {
            SelectedItems.Remove(concert);
        }

        // This property tells the UI whether to show the Delete bar
        HasSelection = SelectedItems.Count > 0;
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

    [RelayCommand]
    private async Task ShowOptions(Concert concert)
    {
        if (concert == null) return;

        // Simple placeholder to prevent binding errors
        string action = await Shell.Current.DisplayActionSheetAsync("Options", "Cancel", null, "Edit", "Delete");
        if (action == "Delete")
        {
            SelectedItems.Clear();
            SelectedItems.Add(concert);
            await DeleteSelectedAsync();
        }
        
        if (action == "Edit")
        {
            await Shell.Current.GoToAsync(nameof(AddConcertPage), new Dictionary<string, object>
        {
            { "SelectedConcert", concert }
        });
        }
    }

    // whenever SelectedSort changes.
    partial void OnSelectedSortChanged(string value)
    {
        if (string.IsNullOrEmpty(value)) return;

        // 1. Save the user's preference
        Preferences.Set(SortPreferenceKey, value);

        // 2. Refresh the list immediately
        // We use Task.Run or a fire-and-forget because this is a synchronous hook
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await RefreshAllAsync();
        });
    }

    //Search happens when text changes.
    partial void OnSearchTextChanged(string value)
    {
        // 1. Cancel the previous search timer if the user is still typing
        _searchCancellation?.Cancel();
        _searchCancellation = new CancellationTokenSource();
        var token = _searchCancellation.Token;

        // 2. Logic: If text is 1 or 2 letters, don't search (unless it's empty to reset the list)
        if (!string.IsNullOrWhiteSpace(value) && value.Length < 3)
            return;

        // 3. The Debounce Timer (400ms is usually the sweet spot)
        Task.Run(async () =>
        {
            try
            {
                await Task.Delay(400, token);

                // 4. If we haven't been cancelled, trigger the refresh
                if (!token.IsCancellationRequested)
                {
                    MainThread.BeginInvokeOnMainThread(async () =>
                    {
                        await RefreshAllAsync();
                    });
                }
            }
            catch (OperationCanceledException) { /* Do nothing if cancelled */ }
        }, token);
    }
}