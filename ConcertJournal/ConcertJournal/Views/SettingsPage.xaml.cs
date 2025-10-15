using ConcertJournal.Resources.Themes;
using Microsoft.Maui.Storage;

namespace ConcertJournal.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
	}

    private bool isAngelTheme = true;

    private void OnSwitchThemeClicked(object sender, EventArgs e)
    {
        App.Current.Resources.MergedDictionaries.Clear();

        if (isAngelTheme)
        {
            App.Current.Resources.MergedDictionaries.Add(new DevilTheme());
            Preferences.Set("AppTheme", "Devil");
        }
        else
        {
            App.Current.Resources.MergedDictionaries.Add(new AngelTheme());
            Preferences.Set("AppTheme", "Angel");
        }

        isAngelTheme = !isAngelTheme;
    }

    //NavigationBar code 
    private async void OnStartPageClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new MainPage(), false);
    }

    private async void OnAddConcertClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddConcertPage(), false);
    }

    private async void OnConcertListClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new ConcertListPage(), false);
    }
}