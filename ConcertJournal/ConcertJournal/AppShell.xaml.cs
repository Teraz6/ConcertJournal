using ConcertJournal.Views;

namespace ConcertJournal
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Configure transitions for Root, Push and Pop
            Shell.Current.CustomShellMaui(new CustomShellMaui.Models.Transitions
            {
                Root = new CustomShellMaui.Models.TransitionRoot
                {
                    // Transition type for current page when doing Root navigation ("//")
                    CurrentPage = CustomShellMaui.Enum.TransitionType.FadeOut
                },
                Push = new CustomShellMaui.Models.Transition
                {
                    // When pushing: current page slides left out, next page slides in from right
                    CurrentPage = CustomShellMaui.Enum.TransitionType.LeftOut,
                    NextPage = CustomShellMaui.Enum.TransitionType.RightIn
                },
                Pop = new CustomShellMaui.Models.Transition
                {
                    // When popping: current page slides right out, next page slides in from left
                    CurrentPage = CustomShellMaui.Enum.TransitionType.RightOut,
                    NextPage = CustomShellMaui.Enum.TransitionType.LeftIn
                }
            });

            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(AddConcertPage), typeof(AddConcertPage));
            Routing.RegisterRoute(nameof(ConcertListPage), typeof(ConcertListPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            Routing.RegisterRoute(nameof(ConcertDetailsPage), typeof(ConcertDetailsPage));
            Routing.RegisterRoute(nameof(StatisticsPage), typeof(StatisticsPage));
        }

    }
}
