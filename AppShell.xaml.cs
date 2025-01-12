using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using RuokalistaApp.Pages;

namespace RuokalistaApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute("SeuraavaViikko", typeof(NextWeekPage));
        Routing.RegisterRoute("Main", typeof(MainPage));
        Routing.RegisterRoute("Welcome", typeof(WelcomePage));

        if(Preferences.Get("kasvisruokalistaEnabled", false))
		{
			if (!Preferences.Get("NaytaKasvis", false))
			{
				tabbar.Items.Remove(KasvisruokaTab);
			}
		}
		else
		{
			tabbar.Items.Remove(KasvisruokaTab);
		}

        
		
        CheckForConfigUpdatesAsync();
	}

	private async void CheckForConfigUpdatesAsync()
	{
		try
		{
			var ServerConfig = await Task.Run(() => Config.GetServerConfig(Preferences.Default.Get("School", "")));
			var primaryColor = ServerConfig["primaryColor"] ?? "#0074ff";

			if(Preferences.Default.Get("PrimaryColor", "#0074ff") != primaryColor)
			{
				App.SetCurrentAppColor(primaryColor);
				Preferences.Default.Set("PrimaryColor", primaryColor);
			}

			Preferences.Default.Set("kasvisruokalistaEnabled", bool.Parse(ServerConfig["kasvisruokalistaEnabled"] ?? "false"));
		}
		catch (Exception)
		{
			string text = "Virhe hakiessa päivityksiä palvelimelta!";
			ToastDuration duration = ToastDuration.Short;
			double fontSize = 15;

			var toast = Toast.Make(text, duration, fontSize);

			await toast.Show();
		}
	}

}
