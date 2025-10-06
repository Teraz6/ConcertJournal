using ConcertJournal.Views;

namespace ConcertJournal
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnAddConcertClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AddConcertPage());
        }
    }
}
