
using Newtonsoft.Json;
using System.Net;
using RuokalistaApp.Models;
using Color = Microsoft.Maui.Graphics.Color;


namespace RuokalistaApp;

public partial class MainPage : ContentPage
{
	

	public MainPage()
	{
		InitializeComponent();
    }

	protected override void OnAppearing()
	{
		base.OnAppearing();

		Initialize(MainStack);
	}

	public async void Initialize(VerticalStackLayout stack, bool kasvisruoka = false)
    {
        await Load(stack, kasvisruoka);
    }

    public async Task Load(VerticalStackLayout stack, bool kasvisruoka = false)
    {
		//branded app feature
#if ANDROID
	    await Platforms.Android.RequestNotificationPerms.RequestNotificationPermission();
#endif

		string baseAddress = Preferences.Get("School", "");

        

		bool showingNextWeeksMenu = false;


		//get ruokalista
		HttpClient client = new HttpClient();
		client.BaseAddress = new Uri(baseAddress);

		var request = "";
		if (!kasvisruoka)
		{
			request = "/api/v1/ruokalista";
		}
		else
		{
			request = "/api/v1/kasvisruokalista";
		}


		if (IsNextWeek())
		{
			if (!kasvisruoka)
			{
				request = $"/api/v1/ruokalista/{DateTime.Now.Year}/{System.Globalization.ISOWeek.GetWeekOfYear(DateTime.Now) + 1}";
			}
			else
			{
				request = $"/api/v1/kasvisruokalista/{DateTime.Now.Year}/{System.Globalization.ISOWeek.GetWeekOfYear(DateTime.Now) + 1}";
			}
			showingNextWeeksMenu = true;
		}

		try
		{
			HttpResponseMessage response = await client.GetAsync(request);

			if (response.IsSuccessStatusCode)
			{
				var output = await response.Content.ReadAsStringAsync();

				Ruokalista ruoka = JsonConvert.DeserializeObject<Ruokalista>(output);

				if (!IsRuokaNullOrEmpty(ruoka))
				{
					//cache ruokalista for offline use
					if (!kasvisruoka)
					{
						File.WriteAllText(Path.Combine(FileSystem.Current.CacheDirectory, "ruoka.json"), output);
					}
					else
					{
						File.WriteAllText(Path.Combine(FileSystem.Current.CacheDirectory, "kasvisruoka.json"), output);
					}

					Render(ruoka, showingNextWeeksMenu, stack, kasvisruoka);
				}
				else
				{
					//error
					//yritä käyttää cachea jos on tälle viikolle
					await Error("Palvelin palautti ruokalistan virheellisessä muodossa", stack, kasvisruoka);
				}



			}
			else if (response.StatusCode == HttpStatusCode.NotFound)
			{
				//ruokalistaa ei ole vielä olemassa

				stack.Children.Clear();
				if (showingNextWeeksMenu)
				{
					if (!kasvisruoka)
					{
						stack.Children.Add(new Label() { Text = "Seuraavan viikon ruokalistaa ei ole vielä julkaistu", FontSize = 45, FontAttributes = FontAttributes.Bold });
					}
					else
					{
						stack.Children.Add(new Label() { Text = "Seuraavan viikon kasvisruokalistaa ei ole vielä julkaistu", FontSize = 45, FontAttributes = FontAttributes.Bold });
					}
				}
				else
				{
					if (!kasvisruoka)
					{
						stack.Children.Add(new Label() { Text = "Tämän viikon ruokalistaa ei ole vielä julkaistu", FontSize = 45, FontAttributes = FontAttributes.Bold });
					}
					else
					{
						stack.Children.Add(new Label() { Text = "Tämän viikon kasvisruokalistaa ei ole vielä julkaistu", FontSize = 45, FontAttributes = FontAttributes.Bold });
					}
				}
			}
			else
			{
				//error
				//yritä käyttää cachea jos on tälle viikolle
				await Error($"Virheellinen pyyntö, Tarkista verkkoyhteytesi. {response.StatusCode} {response.ReasonPhrase}", stack, kasvisruoka);
			}

		}
		catch(Exception e)
		{
			//error ei esim internettiä tai lentotila yms
			//yritä käyttää cachea jos on tälle viikolle
			//ei varmaa tuu mitään muita erroreita niin eiköhän tää riitä
			await Error($"Virhe lähettäessä pyyntöä palvelimelle, tarkista verkkoyhteytesi. {e.Message}", stack, kasvisruoka);
		}





		//seuraavan viikon ruokalista
		try
		{
			if (!showingNextWeeksMenu)
			{
				using (var client2 = new HttpClient())
				{
					client2.BaseAddress = new Uri(baseAddress);
					var url = "";
					if (!kasvisruoka)
					{
						url = $"api/v1/Ruokalista/{DateTime.Now.Year}/{System.Globalization.ISOWeek.GetWeekOfYear(DateTime.Now) + 1}";
					}
					else
					{
						url = $"api/v1/KasvisRuokalista/{DateTime.Now.Year}/{System.Globalization.ISOWeek.GetWeekOfYear(DateTime.Now) + 1}";
					}

					HttpResponseMessage response = await client2.GetAsync(url);
					if (response.IsSuccessStatusCode)
					{
						var btntext = "Näytä seuraavan viikon ruokalista";
						if (kasvisruoka)
						{
							btntext = "Näytä seuraavan viikon kasvisruokalista";
						}

						var seuraavabtn = new Button() { Text = btntext, HorizontalOptions = LayoutOptions.FillAndExpand, Margin = 5, Padding = 10, };
						seuraavabtn.Clicked += async (sender, args) => await Seuraavabtn_Clicked(response);
						stack.Children.Add(seuraavabtn);
					}
				}
			}
		}
		catch(Exception)
		{

		}


	}

	private async Task Error(string reason, VerticalStackLayout stack, bool kasvisruokalista = false)
	{
		await DisplayAlert("Virhe", reason, "OK");

		if (!kasvisruokalista)
		{
			if (!File.Exists(Path.Combine(FileSystem.Current.CacheDirectory, "ruoka.json")))
			{
				stack.Children.Clear();
				stack.Children.Add(new Label() { Text = "Ruokalistaa ei voitu ladata", FontSize = 45, FontAttributes = FontAttributes.Bold });
				return;
			}
		}
		else
		{
			if (!File.Exists(Path.Combine(FileSystem.Current.CacheDirectory, "kasvisruoka.json")))
			{
				stack.Children.Clear();
				stack.Children.Add(new Label() { Text = "Ruokalistaa ei voitu ladata", FontSize = 45, FontAttributes = FontAttributes.Bold });
				return;
			}
		}

		var output = "";
		if (kasvisruokalista)
		{
			output = File.ReadAllText(Path.Combine(FileSystem.Current.CacheDirectory, "kasvisruoka.json"));
		}
		else
		{
			output = File.ReadAllText(Path.Combine(FileSystem.Current.CacheDirectory, "ruoka.json"));
		}


		Ruokalista ruoka = JsonConvert.DeserializeObject<Ruokalista>(output);
		if (ruoka.WeekId == System.Globalization.ISOWeek.GetWeekOfYear(DateTime.Now))
		{
			Render(ruoka, false, stack, kasvisruokalista);
		}
		else
		{
			stack.Children.Clear();
			stack.Children.Add(new Label() { Text = "Ruokalistaa ei voitu ladata", FontSize = 45, FontAttributes = FontAttributes.Bold });
		}
	}


	private void Render(Ruokalista ruoka, bool seuraavaViikko, VerticalStackLayout stack, bool kasvisruokalista = false)
	{
		var lista = new List<Day>();

		lista.Add(new Day() { ruoka = ruoka.Maanantai, dateTime = System.Globalization.ISOWeek.ToDateTime(ruoka.Year, ruoka.WeekId, DayOfWeek.Monday) });
		lista.Add(new Day() { ruoka = ruoka.Tiistai, dateTime = System.Globalization.ISOWeek.ToDateTime(ruoka.Year, ruoka.WeekId, DayOfWeek.Tuesday) });
		lista.Add(new Day() { ruoka = ruoka.Keskiviikko, dateTime = System.Globalization.ISOWeek.ToDateTime(ruoka.Year, ruoka.WeekId, DayOfWeek.Wednesday) });
		lista.Add(new Day() { ruoka = ruoka.Torstai, dateTime = System.Globalization.ISOWeek.ToDateTime(ruoka.Year, ruoka.WeekId, DayOfWeek.Thursday) });
		lista.Add(new Day() { ruoka = ruoka.Perjantai, dateTime = System.Globalization.ISOWeek.ToDateTime(ruoka.Year, ruoka.WeekId, DayOfWeek.Friday) });

		stack.Children.Clear();

		if (seuraavaViikko)
		{
			if (!kasvisruokalista)
			{
				stack.Children.Add(new Label() { Text = String.Format("Seuraavan\nviikon ({0})\nruokalista", ruoka.WeekId.ToString()), FontSize = 45, FontAttributes = FontAttributes.Bold });
			}
			else
			{
				stack.Children.Add(new Label() { Text = String.Format("Seuraavan\nviikon ({0})\nkasvisruokalista", ruoka.WeekId.ToString()), FontSize = 45, FontAttributes = FontAttributes.Bold });
			}
		}
		else
		{
			if (!kasvisruokalista)
			{
				stack.Children.Add(new Label() { Text = String.Format("Tämän\nviikon ({0})\nruokalista", ruoka.WeekId.ToString()), FontSize = 45, FontAttributes = FontAttributes.Bold });
			}
			else
			{
				stack.Children.Add(new Label() { Text = String.Format("Tämän\nviikon ({0})\nkasvisruokalista", ruoka.WeekId.ToString()), FontSize = 45, FontAttributes = FontAttributes.Bold });
			}
		}



		var paivat = new Dictionary<int, string>() { { 1, "Maanantai" }, { 2, "Tiistai" }, { 3, "Keskiviikko" }, { 4, "Torstai" }, { 5, "Perjantai" } };

		foreach (var paiva in lista)
		{
			if (paiva.dateTime.Day == DateTime.Now.Day)
			{
				stack.Children.Add(new Label() { Text = String.Format("{0} {1}", paivat[(int)paiva.dateTime.DayOfWeek], paiva.dateTime.ToString("dd.MM")), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, FontSize = 20, TextColor = Color.FromArgb("FFA500") });
				stack.Children.Add(new Label() { Text = paiva.ruoka, FontSize = 20, HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center, TextColor = Color.FromArgb("FFA500") });
			}
			else
			{
				stack.Children.Add(new Label() { Text = String.Format("{0} {1}", paivat[(int)paiva.dateTime.DayOfWeek], paiva.dateTime.ToString("dd.MM")), HorizontalOptions = LayoutOptions.Center, FontAttributes = FontAttributes.Bold, FontSize = 20 });
				stack.Children.Add(new Label() { Text = paiva.ruoka, FontSize = 20, HorizontalOptions = LayoutOptions.Center, HorizontalTextAlignment = TextAlignment.Center });
			}
		}
	}

	private bool IsRuokaNullOrEmpty(Ruokalista ruoka)
	{
		if (!string.IsNullOrEmpty(ruoka.Maanantai) && !string.IsNullOrEmpty(ruoka.Tiistai) && !string.IsNullOrEmpty(ruoka.Keskiviikko) && !string.IsNullOrEmpty(ruoka.Torstai) && !string.IsNullOrEmpty(ruoka.Perjantai))
		{
			return false;
		}
		return true;
	}

	private bool IsNextWeek()
	{
		if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday || DateTime.Today.DayOfWeek == DayOfWeek.Saturday || (DateTime.Today.DayOfWeek == DayOfWeek.Friday && DateTime.Now.Hour > 12))
		{
			return true;
		}
		return false;
	}


    private async Task Seuraavabtn_Clicked(HttpResponseMessage response)
    {
        await Shell.Current.GoToAsync("SeuraavaViikko", new Dictionary<string, object> { ["response"] = response });
    }
}

