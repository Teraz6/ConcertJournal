using ConcertJournal.Services;
using System.Windows;

namespace ConcertJournal.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
	}

    private bool isAngelTheme = true;

    private void OnSwitchThemeClicked(object sender, RoutedEventArgs e)
    {
        if (isAngelTheme)
            ThemeManager.SetDevilTheme();
        else
            ThemeManager.SetAngelTheme();

        isAngelTheme = !isAngelTheme;
    }
}