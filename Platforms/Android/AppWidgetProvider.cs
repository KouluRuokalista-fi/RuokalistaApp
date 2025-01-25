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

namespace RuokalistaApp.Platforms.Android
{
	[BroadcastReceiver(Label = "Menu of the Week", Exported = true)]
	[IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
	[MetaData("android.appwidget.provider", Resource = "@xml/appwidgetprovider")]
	public class AppWidget : AppWidgetProvider
	{
		private readonly string ApiUrl = Preferences.Get("School", "") + "/api/v1/ruokalista";

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
				string menuData = await FetchMenuData();

				if (menuData != null)
				{
					var Menu = JsonConvert.DeserializeObject<Ruokalista>(menuData);

					remoteViews.SetTextViewText(Resource.Id.Maanantai, $"Maanantai {GetDate(Menu, 1).ToString("dd.MM")}:\n{TrimText(Menu.Maanantai)}");
					remoteViews.SetTextViewText(Resource.Id.Tiistai, $"Tiistai {GetDate(Menu, 2).ToString("dd.MM")}:\n{TrimText(Menu.Tiistai)}");
					remoteViews.SetTextViewText(Resource.Id.Keskiviikko, $"Keskiviikko {GetDate(Menu, 3).ToString("dd.MM")}:\n{TrimText(Menu.Keskiviikko)}");
					remoteViews.SetTextViewText(Resource.Id.Torstai, $"Torstai {GetDate(Menu, 4).ToString("dd.MM")}:\n{TrimText(Menu.Torstai)}");
					remoteViews.SetTextViewText(Resource.Id.Perjantai, $"Perjantai {GetDate(Menu, 5).ToString("dd.MM")}:\n{TrimText(Menu.Perjantai)}");

					HighlightDay(Menu, remoteViews);
				}
				else
				{
					remoteViews.SetTextViewText(Resource.Id.Maanantai, "Virhe ladatessa ruokalistaa");
					remoteViews.SetTextViewText(Resource.Id.Tiistai, "");
					remoteViews.SetTextViewText(Resource.Id.Keskiviikko, "");
					remoteViews.SetTextViewText(Resource.Id.Torstai, "");
					remoteViews.SetTextViewText(Resource.Id.Perjantai, "");
					if (!Preferences.ContainsKey("School"))
					{
						remoteViews.SetTextViewText(Resource.Id.Maanantai, "Valitse koulusi sovelluksesta");
					}
				}
			}
			catch(Exception ex)
			{
				remoteViews.SetTextViewText(Resource.Id.Maanantai, "Virhe ladatessa ruokalistaa");
				remoteViews.SetTextViewText(Resource.Id.Tiistai, "Virhe:\n" + ex.Message);
				remoteViews.SetTextViewText(Resource.Id.Keskiviikko, "");
				remoteViews.SetTextViewText(Resource.Id.Torstai, "");
				remoteViews.SetTextViewText(Resource.Id.Perjantai, "");
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



			var highlightColor = Color.ParseColor(Preferences.Default.Get("primaryColor", Config.PrimaryFallbackColor));
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

		private async Task<string> FetchMenuData()
		{
			try
			{
				using var client = new HttpClient();
				return await client.GetStringAsync(ApiUrl);
			}
			catch
			{
				return null;
			}
		}

		private string TrimText(string text)
		{
			if (string.IsNullOrEmpty(text)) return string.Empty;
			return text.Length > 75 ? text.Substring(0, 75) + "..." : text;
		}
	}
}
