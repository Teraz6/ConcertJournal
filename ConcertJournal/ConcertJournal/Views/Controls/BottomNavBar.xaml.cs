using System.Windows.Input;

namespace ConcertJournal.Views.Controls;

public partial class BottomNavBar : ContentView
{
    public ICommand? NavigateHomeCommand { get; }
    public ICommand? NavigateListCommand { get; }
    public ICommand? NavigateSettingsCommand { get; }
    public ICommand? NavigateStatisticsCommand { get; }

    // 1. Create a BindableProperty so we can set which page is active in XAML
    public static readonly BindableProperty ActiveRouteProperty =
        BindableProperty.Create(nameof(ActiveRoute), typeof(string), typeof(BottomNavBar), string.Empty,
            propertyChanged: OnActiveRouteChanged);

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
            if (ActiveRoute == route) return;

            await Shell.Current.GoToAsync($"//{route}");
        });

        // Set the BindingContext to this control so XAML can access Commands and Properties
        Content.BindingContext = this;
    }

    private static void OnActiveRouteChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BottomNavBar bar)
        {
            bar.UpdateVisualStates((string)newValue);
        }
    }

    private void UpdateVisualStates(string activeRoute)
    {
        // Reset all to Normal, set matching to Selected
        UpdateState(HomeBtn, HomeLbl, "MainPage", activeRoute);
        UpdateState(ListBtn, ListLbl, "ConcertListPage", activeRoute);
        UpdateState(StatsBtn, StatsLbl, "StatisticsPage", activeRoute);
        UpdateState(SettingsBtn, SettingsLbl, "SettingsPage", activeRoute);
    }

    private void UpdateState(VisualElement btn, VisualElement lbl, string targetRoute, string activeRoute)
    {
        bool isSelected = targetRoute == activeRoute;
        string state = isSelected ? "Selected" : "Normal";

        // Apply state to Label
        VisualStateManager.GoToState(lbl, state);

        // Apply state to Button
        // If it's already selected and the user clicks it again, 
        // we force the state back to 'Selected' to override the native 'Normal'
        VisualStateManager.GoToState(btn, state);
    }
}