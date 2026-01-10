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

        public App()
        {
            InitializeComponent();

            //Theme logic
            ApplyTheme();

            //Window sizing configuration
            ConfigureWindowSizing();

#if ANDROID
            // Request runtime permissions on Android
            _ = RequestPermissionsOnAndroid();
#endif
        }

        private void ApplyTheme()
        {
            string savedTheme = Preferences.Get("AppTheme", "Angel");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                Resources.MergedDictionaries.Clear();
                if (savedTheme == "Devil")
                    Resources.MergedDictionaries.Add(new Resources.Themes.DevilTheme());
                else
                    Resources.MergedDictionaries.Add(new Resources.Themes.AngelTheme());
            });
        }

        private void ConfigureWindowSizing()
        {
            Microsoft.Maui.Handlers.WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
            {
#if WINDOWS
            var nativeWindow = handler.PlatformView;
            nativeWindow.Activate();
            IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(windowHandle);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(WindowWidth, WindowHeight));
#endif
            });
        }

        //Override CreateWindow instead of using MainPage
        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());
            return window;
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

    }
}
