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
	[BroadcastReceiver(Label = "Päivän ruoka", Exported = true)]
	[IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
	[MetaData("android.appwidget.provider", Resource = "@xml/smallwidgetprovider")]
	public class SmallWidget : AppWidgetProvider
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

			// Create an intent to open the app
			Intent intent = new Intent(context, typeof(MainActivity));
			intent.AddFlags(ActivityFlags.NewTask);
			PendingIntent pendingIntent = PendingIntent.GetActivity(context, 0, intent, PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);
			remoteViews.SetOnClickPendingIntent(Resource.Id.widget_root, pendingIntent);



			remoteViews.SetTextViewText(Resource.Id.Ruoka, DateTime.Now.ToString("dd.MM.yyyy"));


			appWidgetManager.UpdateAppWidget(widgetId, remoteViews);
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
			return text.Length > 100 ? text.Substring(0, 100) + "..." : text;
		}
	}
}
