using System.Windows.Input;

namespace ConcertJournal.Views.Controls;

public partial class BottomNavBar : ContentView
{
    public ICommand? NavigateHomeCommand { get; }
    public ICommand? NavigateAddCommand { get; }
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
        UpdateState(AddBtn, AddLbl, "AddConcertPage", activeRoute);
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

    // Ensure states are correct when the control first loads
    //protected override void OnHandlerChanged()
    //{
    //    base.OnHandlerChanged();
    //    UpdateVisualStates(ActiveRoute);
    //}

    //private async void OnHomeClicked(object sender, EventArgs e)
    //{
    //    await Shell.Current.GoToAsync($"//MainPage");
    //}

    //private async void OnListClicked(object sender, EventArgs e)
    //{
    //    await Shell.Current.GoToAsync($"//ConcertListPage");
    //}

    //private async void OnAddClicked(object sender, EventArgs e)
    //{
    //    await Shell.Current.GoToAsync($"//AddConcertPage");
    //}

    //private async void OnSettingsClicked(object sender, EventArgs e)
    //{
    //    await Shell.Current.GoToAsync($"//SettingsPage");
    //}

    //private async void OnStatisticsClicked(object sender, EventArgs e)
    //{
    //    await Shell.Current.GoToAsync($"//StatisticsPage");
    //}
}