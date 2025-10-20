using ConcertJournal.Data;
using ConcertJournal.Services;
using ConcertJournal.Views;
using System.Resources;

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

namespace ConcertJournal
{
    public partial class App : Application
    {
        const int WindowWidth = 540;
        const int WindowHeight = 1000;

        public static DatabaseContext Database { get; private set; }
        public App()
        {
            InitializeComponent();


            // 🔹 Load saved theme preference (default: Angel)
            string savedTheme = Preferences.Get("AppTheme", "Angel");

            // 🔹 Apply selected theme
            App.Current?.Resources?.MergedDictionaries?.Clear();
            if (savedTheme == "Devil")
                App.Current?.Resources.MergedDictionaries.Add(new Resources.Themes.DevilTheme());
            else
                App.Current?.Resources.MergedDictionaries.Add(new Resources.Themes.AngelTheme());

            // Initialize the SQLite database
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "concerts.db3");
            Database = new DatabaseContext(dbPath);

            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
            {
            #if WINDOWS
                var mauiWindow = handler.VirtualView;
                var nativeWindow = handler.PlatformView;
                nativeWindow.Activate();
                IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
                WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
                AppWindow appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                appWindow.Resize(new SizeInt32(WindowWidth, WindowHeight));
            #endif
            });

            MainPage = new AppShell();
        }



        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new AppShell());
        //}
    }
}
