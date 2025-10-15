using ConcertJournal.Views;

namespace ConcertJournal
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(AddConcertPage), typeof(AddConcertPage));
            Routing.RegisterRoute(nameof(ConcertListPage), typeof(ConcertListPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
        }

    }
}
