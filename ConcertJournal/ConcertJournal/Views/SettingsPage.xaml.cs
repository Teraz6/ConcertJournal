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
}