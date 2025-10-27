using System.Windows.Input;

namespace ConcertJournal.Views.Controls;

public partial class TitleNavBar : ContentView
{
    public ICommand NavigateHomeCommand { get; }
    public ICommand NavigateAddCommand { get; }
    public ICommand NavigateListCommand { get; }
    public ICommand NavigateSettingsCommand { get; }

    public TitleNavBar()
    {
        InitializeComponent();

        BindingContext = this;
    }

    private async void OnHomeClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//MainPage");
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//AddConcertPage");
    }

    private async void OnListClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//ConcertListPage");
    }
    private async void OnSettingsClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//SettingsPage");
    }
}