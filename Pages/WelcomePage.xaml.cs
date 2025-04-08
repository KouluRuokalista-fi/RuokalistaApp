using AndroidX.ConstraintLayout.Utils.Widget;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Newtonsoft.Json;
namespace RuokalistaApp.Pages;

public partial class WelcomePage : ContentPage
{
	List<Endpoint> Endpoints = new List<Endpoint>();
	int Taps = 0;
	public WelcomePage()
	{
		InitializeComponent();
		LanguagePicker.SelectedIndex = 0;

		//get the endpoints from the api
		HttpClient client = new HttpClient();
		client.BaseAddress = new Uri("https://kouluruokalista.fi");
		var request = "/api/v1/GetEndpoints";
		try
		{
			HttpResponseMessage response = client.GetAsync(request).Result;
			if (response.IsSuccessStatusCode)
			{
				var result = response.Content.ReadAsStringAsync().Result;
				Endpoints = JsonConvert.DeserializeObject<List<Endpoint>>(result);
				foreach (var endpoint in Endpoints)
				{
					KouluPicker.Items.Add(endpoint.name);
				}
			}
			else
			{
				DisplayAlert("Virhe " + response.StatusCode.ToString(), "Virhe ladatessa sisältöä", "ok");
				return;
			}
		}
		catch(Exception ex)
		{
			DisplayAlert("Virhe", "Virhe yhdistäessä palvelimeen, tarkista verkkoyhteytesi!", "ok");
			return;
		}

	}

	private async void Button_Clicked(object sender, EventArgs e)
	{
		Loader.IsVisible = true;
		Loader.IsRunning = true;
		ActionButton.IsEnabled = false;
		//validate dev url and set it to prefs
		//kysy primarycolor
		//kysy kasvisruoka
		//setup notif channel

		if (Preferences.Default.Get("DevMode", false) == true)
		{
			if(string.IsNullOrEmpty(KouluURLInput.Text))
			{
				await DisplayAlert("Virhe", "Palvelimen osoite ei voi olla tyhjä", "ok");
				return;
			}
			else if(!KouluURLInput.Text.StartsWith("http") || !KouluURLInput.Text.StartsWith("https"))
			{
				await DisplayAlert("Virhe", "Palvelimen osoite ei ole validi", "ok");
				return;
			}
			else
			{
				Preferences.Default.Set("School", KouluURLInput.Text);
			}
		}
		else if(KouluPicker.SelectedItem == null)
		{
			string text = "Valitse koulu (pakollinen)";
			ToastDuration duration = ToastDuration.Short;
			double fontSize = 15;

			var toast = Toast.Make(text, duration, fontSize);

			await toast.Show();
			Loader.IsVisible = false;
			Loader.IsRunning = false;
			ActionButton.IsEnabled = true;
			return;
		}


		//TODO register notif channels and set the env
		try
		{
			var ServerConfig = await Config.GetServerConfig(Preferences.Default.Get("School", ""));
			var primaryColor = ServerConfig["primaryColor"] ?? Config.PrimaryFallbackColor;
			App.SetCurrentAppColor(primaryColor);
			Preferences.Default.Set("PrimaryColor", primaryColor);
			Preferences.Default.Set("kasvisruokalistaEnabled", bool.Parse(ServerConfig["kasvisruokalistaEnabled"] ?? "false"));

			Preferences.Default.Set("SetupDone", true);

			Application.Current.MainPage = new AppShell();


		}
		catch(Exception ex)
		{
			await DisplayAlert("Virhe", ex.Message, "ok");
			Loader.IsVisible = false;
			Loader.IsRunning = false;
			ActionButton.IsEnabled = true;
		}
	}

	private void LanguagePicker_SelectedIndexChanged(object sender, EventArgs e)
	{
		Preferences.Default.Set("Lang", LanguagePicker.SelectedItem.ToString());

		//TODO: set th app lang
	}

	private void KouluPicker_SelectedIndexChanged(object sender, EventArgs e)
	{
		var selectedSchool = Endpoints.Where(x => x.name == KouluPicker.SelectedItem.ToString()).First();
		Preferences.Default.Set("School", "https://" + selectedSchool.url);
	}

	private class Endpoint
	{
		public string name { get; set; }
		public string url { get; set; }

	}

	private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
	{
		if (KouluURLInput.IsVisible && Taps >= 3)
		{
			KouluURLInput.IsVisible = false;
			KehittajaLabel.IsVisible = false;

			KouluPicker.IsEnabled = true;

			Preferences.Default.Set("DevMode", false);
			Taps = 0;
		}
		else if(Taps >= 3)
		{
			KouluURLInput.IsVisible = true;
			KehittajaLabel.IsVisible = true;

			KouluPicker.IsEnabled = false;

			Preferences.Default.Set("DevMode", true);
			Taps = 0;
		}
		Taps++;
	}
}