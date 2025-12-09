using System.Windows.Input;

namespace ConcertJournal.Views.Controls;

public partial class BottomNavBar : ContentView
{
    public ICommand NavigateHomeCommand { get; }
    public ICommand NavigateAddCommand { get; }
    public ICommand NavigateListCommand { get; }
    public ICommand NavigateSettingsCommand { get; }
    public ICommand NavigateStatisticsCommand { get; }

    // 1. Create a BindableProperty so we can set which page is active in XAML
    public static readonly BindableProperty ActiveRouteProperty =
        BindableProperty.Create(nameof(ActiveRoute), typeof(string), typeof(BottomNavBar), "Home");

    public string ActiveRoute
    {
        get => (string)GetValue(ActiveRouteProperty);
        set => SetValue(ActiveRouteProperty, value);
    }

    public ICommand NavigateCommand { get; }

    public BottomNavBar()
    {
        InitializeComponent();

        // 2. Centralize navigation logic
        NavigateCommand = new Command<string>(async (route) =>
        {
            // Prevent navigating to the page we are already on
            if (CurrentRouteMatches(route)) return;

            await Shell.Current.GoToAsync($"//{route}");
        });

        // Set the BindingContext to this control so XAML can access Commands and Properties
        Content.BindingContext = this;
    }

    private bool CurrentRouteMatches(string route)
    {
        // Simple check to see if the requested route matches the active one
        // Note: You might need to adjust logic depending on your Shell route naming
        return ActiveRoute == route;
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//MainPage");
    }

    private async void OnListClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//ConcertListPage");
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//AddConcertPage");
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//SettingsPage");
    }

    private async void OnStatisticsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//StatisticsPage");
    }
}