namespace ConcertJournal.Views;

public partial class StatisticsPage : ContentPage
{
	public StatisticsPage()
	{
		InitializeComponent();
	}

	private async void OnPerformersClicked(object sender, EventArgs e)
	{
		await Shell.Current.GoToAsync($"//Performers");
    }
}
