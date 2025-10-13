using ConcertJournal.Views;
using ConcertJournal.Models;            // <-- add this so Concert is known

namespace ConcertJournal
{
    public partial class MainPage : ContentPage
    {
        // This is the “light switch” the button listens to
        bool _isAddButtonVisible = true;
        public bool IsAddButtonVisible
        {
            get => _isAddButtonVisible;
            set { _isAddButtonVisible = value; OnPropertyChanged(); }
        }

        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;      // connect the switch to XAML
        }

        // When the page shows, check the database and set the switch
        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Get concerts from your DB
            var concerts = await App.Database.GetConcertsAsync();

            // Show button only if there are none
            IsAddButtonVisible = concerts.Count == 0;
        }

        private async void OnStartPageClicked(object sender, EventArgs e)
        {
            // (Optional) avoid pushing another MainPage on top of MainPage
            await Navigation.PopToRootAsync();
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
