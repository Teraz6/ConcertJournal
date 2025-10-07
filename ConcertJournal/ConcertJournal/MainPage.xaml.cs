using ConcertJournal.Views;

namespace ConcertJournal
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

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
}
