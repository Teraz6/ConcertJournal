using ConcertJournal.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ConcertJournal.Views;

public partial class ConcertListPage : ContentPage
{

    private List<Concert> allConcerts = new();
    private bool sortAscending = true;
    public ObservableCollection<Concert> Concerts { get; set; } = new();

    public ConcertListPage()
	{
		InitializeComponent();
        LoadConcerts();
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
    }

    private async Task LoadConcerts()
    {
        allConcerts = await App.Database.GetConcertsAsync();

        // Default sort ascending
        sortAscending = true;
        ApplySearchAndSort();
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
        var picker = sender as Picker;
        if (picker.SelectedIndex == -1) return;

        var selected = picker.SelectedItem.ToString();

        if (selected == "Oldest By Year")
            sortAscending = true;
        else if (selected == "Newest By Year")
            sortAscending = false;
        else
        {
            // "Default" selected — no sorting
            ApplySearchAndSort(SearchBar.Text, skipSorting: true);
            return;
        }

        ApplySearchAndSort(SearchBar.Text);
    }

    private void ApplySearchAndSort(string searchText = "", bool skipSorting = false)
    {
        var filtered = allConcerts
            .Where(c => string.IsNullOrEmpty(searchText) ||
                        (c.EventTitle?.ToLower().Contains(searchText.ToLower()) ?? false) ||
                        (c.Performers?.ToLower().Contains(searchText.ToLower()) ?? false))
            .ToList();

        IEnumerable<Concert> result = filtered;

        if (!skipSorting)
        {
            result = sortAscending
                ? filtered.OrderBy(c => c.Date)
                : filtered.OrderByDescending(c => c.Date);
        }

        Concerts.Clear();
        foreach (var concert in result)
            Concerts.Add(concert);

        ConcertListView.ItemsSource = Concerts;
    }

    private async void OnConcertSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Concert selected)
        {
            await Navigation.PushAsync(new ConcertDetailsPage(selected));
            ((CollectionView)sender).SelectedItem = null;
        }
    }

}