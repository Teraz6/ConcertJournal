namespace ConcertJournal.Services
{
    public static class ThemeManager
    {
        private const string DevilThemePath = "Resources/Styles/DevilTheme.xaml";
        private const string AngelThemePath = "Resources/Styles/AngelTheme.xaml";

        private static ResourceDictionary? _currentTheme;

        public static void ApplyTheme(string themePath)
        {
            var newTheme = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/{themePath}", UriKind.Absolute)
            };

            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(newTheme);
        }

        public static void SetAngelTheme()
        {
            ApplyTheme("Resources/Styles/AngelTheme.xaml");
        }

        public static void SetDevilTheme()
        {
            ApplyTheme("Resources/Styles/DevilTheme.xaml");
        }
    }
}
