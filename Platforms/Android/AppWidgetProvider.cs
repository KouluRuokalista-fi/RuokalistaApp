using System.Net;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;
using Newtonsoft.Json;
using RuokalistaApp.Models;
using System.Net.Http;
using System.Threading.Tasks;
using Android.Graphics;
using Color = Android.Graphics.Color;
using Java.Time;
using Java.Net;
using static Android.Gms.Common.Apis.Api;
using Android.Health.Connect.DataTypes;
using DayOfWeek = System.DayOfWeek;

namespace RuokalistaApp.Platforms.Android
{
	[BroadcastReceiver(Label = "Viikon ruokalista", Exported = true)]
	[IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
	[MetaData("android.appwidget.provider", Resource = "@xml/appwidgetprovider")]
	public class AppWidget : AppWidgetProvider
	{
		

		public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
		{
			foreach (var widgetId in appWidgetIds)
			{
				UpdateWidget(context, appWidgetManager, widgetId);
			}
		}

		private async void UpdateWidget(Context context, AppWidgetManager appWidgetManager, int widgetId)
		{

			var remoteViews = new RemoteViews(context.PackageName, Resource.Layout.Widget);
			try
			{
				// Create an intent to open the app
				Intent intent = new Intent(context, typeof(MainActivity));
				intent.AddFlags(ActivityFlags.NewTask);
				PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intent,
					PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
				remoteViews.SetOnClickPendingIntent(Resource.Id.widget_root, pendingIntent);
			}
			catch (Exception)
			{

			}

			//check if school is selected
			if (!Preferences.ContainsKey("School"))
			{
				SetErrorText(remoteViews, "Valitse koulusi sovelluksesta");
				appWidgetManager.UpdateAppWidget(widgetId, remoteViews);
				return;

			}


			var currentWeek = System.Globalization.ISOWeek.GetWeekOfYear(DateTime.Now);

			//check if next week should be shown
			//here might be a problem with the last week of the year
			if (IsWeekendOrFridayAfternoon())
			{
				currentWeek += 1;
			}

			//fetch data
			var apiurl = Preferences.Get("School", "") + $"/api/v1/Ruokalista/{DateTime.Now.Year}/{currentWeek}";
			if (Preferences.Get("NaytaKasvisWidgetissa", false))
			{
				apiurl = Preferences.Get("School", "") + $"/api/v1/KasvisRuokalista/{DateTime.Now.Year}/{currentWeek}";
			}

			try
			{
				using var client = new HttpClient();
				HttpResponseMessage response = await client.GetAsync(apiurl);
				if (response.IsSuccessStatusCode)
				{
					var menuData = await response.Content.ReadAsStringAsync();
					//success
					try
					{
						var Menu = JsonConvert.DeserializeObject<Ruokalista>(menuData);

						//data is correct, display and save:

						remoteViews.SetTextViewText(Resource.Id.Maanantai,
							$"Maanantai {GetDate(Menu, 1).ToString("dd.MM")}:\n{TrimText(Menu.Maanantai)}");
						remoteViews.SetTextViewText(Resource.Id.Tiistai,
							$"Tiistai {GetDate(Menu, 2).ToString("dd.MM")}:\n{TrimText(Menu.Tiistai)}");
						remoteViews.SetTextViewText(Resource.Id.Keskiviikko,
							$"Keskiviikko {GetDate(Menu, 3).ToString("dd.MM")}:\n{TrimText(Menu.Keskiviikko)}");
						remoteViews.SetTextViewText(Resource.Id.Torstai,
							$"Torstai {GetDate(Menu, 4).ToString("dd.MM")}:\n{TrimText(Menu.Torstai)}");
						remoteViews.SetTextViewText(Resource.Id.Perjantai,
							$"Perjantai {GetDate(Menu, 5).ToString("dd.MM")}:\n{TrimText(Menu.Perjantai)}");

						HighlightDay(Menu, remoteViews);

						//save to cache
						await File.WriteAllTextAsync(System.IO.Path.Combine(FileSystem.Current.CacheDirectory, "widgetCache.json"), menuData);
					}
					catch(Exception)
					{
						//data was not in the correct format or broken
						//try cache load 
						try
						{
							menuData = await File.ReadAllTextAsync(System.IO.Path.Combine(FileSystem.Current.CacheDirectory, "widgetCache.json"));

							var Menu = JsonConvert.DeserializeObject<Ruokalista>(menuData);

							//data in cache is correct, display:

							remoteViews.SetTextViewText(Resource.Id.Maanantai,
								$"Maanantai {GetDate(Menu, 1).ToString("dd.MM")}:\n{TrimText(Menu.Maanantai)}");
							remoteViews.SetTextViewText(Resource.Id.Tiistai,
								$"Tiistai {GetDate(Menu, 2).ToString("dd.MM")}:\n{TrimText(Menu.Tiistai)}");
							remoteViews.SetTextViewText(Resource.Id.Keskiviikko,
								$"Keskiviikko {GetDate(Menu, 3).ToString("dd.MM")}:\n{TrimText(Menu.Keskiviikko)}");
							remoteViews.SetTextViewText(Resource.Id.Torstai,
								$"Torstai {GetDate(Menu, 4).ToString("dd.MM")}:\n{TrimText(Menu.Torstai)}");
							remoteViews.SetTextViewText(Resource.Id.Perjantai,
								$"Perjantai {GetDate(Menu, 5).ToString("dd.MM")}:\n{TrimText(Menu.Perjantai)}");

							HighlightDay(Menu, remoteViews);

						}
						catch (Exception)
						{
							//cache load failure
							SetErrorText(remoteViews, "Virhe ladatessa ruokalistaa");
						}



						
						
					}
				}

				else if (response.StatusCode == HttpStatusCode.NotFound)
				{
					SetErrorText(remoteViews, "Tämän viikon ruokalistaa ei ole vielä olemassa");
				}
				else
				{
					//virhe
					SetErrorText(remoteViews, "Virhe ladatessa ruokalistaa: " + response.StatusCode.ToString());
				}
			}
			catch (Exception)
			{
				SetErrorText(remoteViews, "Virhe ladatessa ruokalistaa!");
			}

			

			appWidgetManager.UpdateAppWidget(widgetId, remoteViews);
		}

		private void HighlightDay(Ruokalista ruokalista, RemoteViews remoteViews)
		{
			//reset the color of text fields firtst
			remoteViews.SetTextColor(Resource.Id.Maanantai, Color.White);
			remoteViews.SetTextColor(Resource.Id.Tiistai, Color.White);
			remoteViews.SetTextColor(Resource.Id.Keskiviikko, Color.White);
			remoteViews.SetTextColor(Resource.Id.Torstai, Color.White);
			remoteViews.SetTextColor(Resource.Id.Perjantai, Color.White);



			var highlightColor = Color.ParseColor(Preferences.Default.Get("PrimaryColor", Config.PrimaryFallbackColor));
			if (GetDate(ruokalista, 1).Date == DateTime.Now.Date)
			{
				remoteViews.SetTextColor(Resource.Id.Maanantai, highlightColor);
			}
			else if (GetDate(ruokalista, 2).Date == DateTime.Now.Date)
			{
				remoteViews.SetTextColor(Resource.Id.Tiistai, highlightColor);
			}
			else if (GetDate(ruokalista, 3).Date == DateTime.Now.Date)
			{
				remoteViews.SetTextColor(Resource.Id.Keskiviikko, highlightColor);
			}
			else if (GetDate(ruokalista, 4).Date == DateTime.Now.Date)
			{
				remoteViews.SetTextColor(Resource.Id.Torstai, highlightColor);
			}
			else if (GetDate(ruokalista, 5).Date == DateTime.Now.Date)
			{
				remoteViews.SetTextColor(Resource.Id.Perjantai, highlightColor);
			}
		}

		private DateTime GetDate(Ruokalista ruoka, int dateOfWeek)
		{
			switch (dateOfWeek)
			{

				case 1:
					return System.Globalization.ISOWeek.ToDateTime(ruoka.Year, ruoka.WeekId, DayOfWeek.Monday);
				case 2:
					return System.Globalization.ISOWeek.ToDateTime(ruoka.Year, ruoka.WeekId, DayOfWeek.Tuesday);
				case 3:
					return System.Globalization.ISOWeek.ToDateTime(ruoka.Year, ruoka.WeekId, DayOfWeek.Wednesday);
				case 4:
					return System.Globalization.ISOWeek.ToDateTime(ruoka.Year, ruoka.WeekId, DayOfWeek.Thursday);
				case 5:
					return System.Globalization.ISOWeek.ToDateTime(ruoka.Year, ruoka.WeekId, DayOfWeek.Friday);
				default:
					return DateTime.MinValue;
			}

		}

		private void SetErrorText(RemoteViews remoteViews, string errorMessage)
		{
			remoteViews.SetTextViewText(Resource.Id.Maanantai, errorMessage);
			remoteViews.SetTextViewText(Resource.Id.Tiistai, "");
			remoteViews.SetTextViewText(Resource.Id.Keskiviikko, "");
			remoteViews.SetTextViewText(Resource.Id.Torstai, "");
			remoteViews.SetTextViewText(Resource.Id.Perjantai, "");
		}

		

		public static bool IsWeekendOrFridayAfternoon()
		{
			// Get current date and time
			DateTime now = DateTime.Now;

			// Check if it's Friday and after 12:00
			bool isFridayAfternoon = now.DayOfWeek == DayOfWeek.Friday && now.TimeOfDay.TotalHours >= 12;

			// Check if it's weekend (Saturday or Sunday)
			bool isWeekend = now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday;

			// Return true if either condition is met
			return isFridayAfternoon || isWeekend;
		}

		private string TrimText(string text)
		{
			if (string.IsNullOrEmpty(text)) return string.Empty;
			return text.Length > 100 ? text.Substring(0, 100) + "..." : text;
		}
	}
}
