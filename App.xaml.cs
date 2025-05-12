using Firebase.Messaging;
using RuokalistaApp.Pages;
using System.Net;

namespace RuokalistaApp;

public partial class App : Application
{
	public App()
	{
		//branded app static values setup
		Preferences.Default.Set("School", "https://isokyro.kouluruokalista.fi");
		Preferences.Default.Set("PrimaryColor", Config.PrimaryFallbackColor);
		Preferences.Default.Set("Lang", "fi");
		Preferences.Default.Set("CustomApp", true);
		Preferences.Set("SetupDone", true);
#if ANDROID
		FirebaseMessaging.Instance.SubscribeToTopic("GlobalNotifications");
		FirebaseMessaging.Instance.SubscribeToTopic("isokyro.kouluruokalista.fi"); //format: isokyro.kouluruokalista.fi
#endif



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

			var color = Preferences.Get("PrimaryColor", Config.PrimaryFallbackColor);
			Application.Current.Resources["Primary"] = Color.FromArgb(color);
			MainPage.Appearing += (s, e) => UpdateAndroidSystemBars(color);
		}
		else
		{
			Application.Current.MainPage = new WelcomePage();

			var color = Config.PrimaryFallbackColor;
			Application.Current.Resources["Primary"] = Color.FromArgb(color);
			MainPage.Appearing += (s, e) => UpdateAndroidSystemBars(color);
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
