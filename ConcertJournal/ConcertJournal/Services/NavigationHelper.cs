namespace ConcertJournal.Services
{
    public static class NavigationHelper
    {
        public static readonly Dictionary<string, int> PageOrder = new()
    {
        { "//MainPage", 0 },
        { "//ConcertListPage", 1 },
        { "//AddConcertPage", 2 },
        { "//StatisticsPage", 3 },
        { "//SettingsPage", 4 }
    };

        public static int CurrentIndex = 0;
    }
}
