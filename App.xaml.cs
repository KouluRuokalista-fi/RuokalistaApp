using RuokalistaApp.Pages;

namespace RuokalistaApp;

public partial class App : Application
{
	public App()
	{
		
		if (Preferences.Default.ContainsKey("Teema"))
		{
			var key = Preferences.Default.Get("Teema", 0);
			if (key == 0)
			{
				Application.Current.UserAppTheme = AppTheme.Unspecified;
			}
			else if (key == 1)
			{
				Application.Current.UserAppTheme = AppTheme.Dark;
			}
			else if (key == 2)
			{
				Application.Current.UserAppTheme = AppTheme.Light;
			}
		}


		InitializeComponent();

		

	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		if (Preferences.Get("SetupDone", false))
		{
			var window = new Window(new AppShell());


			var color = Preferences.Get("PrimaryColor", Config.PrimaryFallbackColor);
			Application.Current.Resources["Primary"] = Color.FromArgb(color);
			window.Activated += (s, e) => UpdateAndroidSystemBars(color);

			return window;
		}
		else
		{
			var window = new Window(new WelcomePage());
			Application.Current.OpenWindow(window);

			var color = Config.PrimaryFallbackColor;
			Application.Current.Resources["Primary"] = Color.FromArgb(color);
			window.Activated += (s, e) => UpdateAndroidSystemBars(color);

			return window;
		}
	}

	public static void SetCurrentAppColor(string color)
	{
		Application.Current.Resources["Primary"] = Color.FromArgb(color);
		UpdateAndroidSystemBars(color);
	}

	private static void UpdateAndroidSystemBars(string color)
	{
#if ANDROID
		if (Platform.CurrentActivity?.Window != null && Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.Lollipop)
		{
			var androidColor = Android.Graphics.Color.ParseColor(color);
			// Set the status bar color
			Platform.CurrentActivity.Window.SetStatusBarColor(androidColor);

			// Set the navigation bar color
			Platform.CurrentActivity.Window.SetNavigationBarColor(androidColor);
		}
#endif
	}
}
