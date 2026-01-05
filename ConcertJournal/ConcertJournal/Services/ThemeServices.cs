namespace ConcertJournal.Services
{
    public static class ThemeServices
    {
        private const string DevilThemePath = "Resources/Themes/DevilTheme.xaml";
        private const string AngelThemePath = "Resources/Themes/AngelTheme.xaml";

        public static void ApplyTheme(string themePath)
        {
            var newTheme = new ResourceDictionary
            {
                Source = new Uri($"pack://application:,,,/{themePath}", UriKind.Absolute)
            };

            if (Application.Current?.Resources?.MergedDictionaries == null)
                return;

            Application.Current.Resources.MergedDictionaries.Add(newTheme);
        }

        public static void SetAngelTheme()
        {
            ApplyTheme("Resources/Themes/AngelTheme.xaml");
        }

        public static void SetDevilTheme()
        {
            ApplyTheme("Resources/Themes/DevilTheme.xaml");
        }
    }
}
