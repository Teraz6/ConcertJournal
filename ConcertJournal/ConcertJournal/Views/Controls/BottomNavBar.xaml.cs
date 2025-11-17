using ConcertJournal.Services;
using System.Windows.Input;

namespace ConcertJournal.Views.Controls;

public partial class BottomNavBar : ContentView
{
    public ICommand NavigateHomeCommand { get; }
    public ICommand NavigateAddCommand { get; }
    public ICommand NavigateListCommand { get; }
    public ICommand NavigateSettingsCommand { get; }
    public ICommand NavigateStatisticsCommand { get; }

    public BottomNavBar()
    {
        InitializeComponent();

        BindingContext = this;
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await NavigateWithAnimation("//MainPage",
            NavigationHelper.PageOrder["//MainPage"]);
    }

    private async void OnListClicked(object sender, EventArgs e)
    {
        await NavigateWithAnimation("//ConcertListPage",
            NavigationHelper.PageOrder["//ConcertListPage"]);
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await NavigateWithAnimation("//AddConcertPage",
            NavigationHelper.PageOrder["//AddConcertPage"]);
    }

    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await NavigateWithAnimation("//SettingsPage",
            NavigationHelper.PageOrder["//SettingsPage"]);
    }

    private async void OnStatisticsClicked(object sender, EventArgs e)
    {
        await NavigateWithAnimation("//StatisticsPage",
            NavigationHelper.PageOrder["//StatisticsPage"]);
    }

    private async Task NavigateWithAnimation(string route, int newIndex)
    {
        int oldIndex = NavigationHelper.CurrentIndex;

        bool goingRight = newIndex > oldIndex;

        // Apply direction-based animation
        Shell.Current.CustomShellMaui(new CustomShellMaui.Models.Transitions
        {
            Push = new CustomShellMaui.Models.Transition
            {
                CurrentPage = goingRight
                    ? CustomShellMaui.Enum.TransitionType.LeftOut
                    : CustomShellMaui.Enum.TransitionType.RightOut,

                NextPage = goingRight
                    ? CustomShellMaui.Enum.TransitionType.RightIn
                    : CustomShellMaui.Enum.TransitionType.LeftIn
            }
        });

        NavigationHelper.CurrentIndex = newIndex;

        await Shell.Current.GoToAsync(route);
    }
}