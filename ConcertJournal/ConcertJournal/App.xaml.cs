using ConcertJournal.Data;
using ConcertJournal.Services;
using ConcertJournal.Views;
using System.Resources;

#if WINDOWS
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Windows.Graphics;
#endif

#if ANDROID
using Microsoft.Maui.ApplicationModel; // For Permissions
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

            // Check for updates
            var updateService = new UpdateServices();
            Task.Run(() => updateService.CheckForUpdateAsync());

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


#if ANDROID
            // Request runtime permissions on Android
            _ = RequestPermissionsOnAndroid();
#endif
        }

#if ANDROID
        private async Task RequestPermissionsOnAndroid()
        {
            try
            {
                var mainActivity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
                if (mainActivity is MainActivity activity)
                    await activity.RequestAppPermissionsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Permission request failed: {ex.Message}");
            }
        }
#endif

        // 🔹 NEW: Override CreateWindow instead of using MainPage
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());
            return window;
        }



        //protected override Window CreateWindow(IActivationState? activationState)
        //{
        //    return new Window(new AppShell());
        //}
    }
}
