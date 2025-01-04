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

		if(Preferences.Get("SetupDone", false))
		{
			Application.Current.MainPage = new AppShell();

			var color = Preferences.Get("PrimaryColor", "#0244ea");//VAIHA TÄNNE OIKEA DEFAULT VÄRI VÄRI!!!!!!!!
			Application.Current.Resources["Primary"] = Color.FromArgb(color);
			MainPage.Appearing += (s, e) => UpdateAndroidSystemBars(color);
		}
		else
		{
			Application.Current.MainPage = new WelcomePage();
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
