using System;
using System.Collections.Generic;
using Android;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Util;
using Android.Widget;
using Android.OS;
using Android.Content.Res;
using System.Net;
using Newtonsoft.Json;
using RuokalistaApp.Models;
using Android.Graphics;
using Color = Android.Graphics.Color;
using Path = System.IO.Path;

namespace RuokalistaApp
{

    [Activity(Label = "Ruokalista", Name = "com.arttu.ruokalista.AppWidget")]

    public class AppWidget : AppWidgetProvider
    {
        private static string AnnouncementClick = "AnnouncementClickTag";

        /// <summary>
        /// This method is called when the 'updatePeriodMillis' from the AppwidgetProvider passes,
        /// or the user manually refreshes/resizes.
        /// </summary>
        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            var me = new ComponentName(context, Java.Lang.Class.FromType(typeof(AppWidget)).Name);
            appWidgetManager.UpdateAppWidget(me, BuildRemoteViews(context, appWidgetIds));
        }

        private RemoteViews BuildRemoteViews(Context context, int[] appWidgetIds)
        {


            // Retrieve the widget layout. This is a RemoteViews, so we can't use 'FindViewById'
            var widgetView = new RemoteViews(context.PackageName, RuokalistaApp.Resource.Layout.Widget);


            SetTextViewText(widgetView);
            RegisterClicks(context, appWidgetIds, widgetView);

            return widgetView;
        }

        private void SetTextViewText(RemoteViews widgetView)
        {

            Ruokalista ruoka;
            var paikallinen = false;
            try
            {
                var url = "https://ruokalista.arttukuikka.fi/api/v1/Ruokalista";

                var client = new HttpClient();
                var response = client.GetAsync(url);
                var content = response.GetAwaiter().GetResult().Content.ReadAsStringAsync();
                var rawruoka = content.GetAwaiter().GetResult();
                
                ruoka = JsonConvert.DeserializeObject<Ruokalista>(rawruoka);

                File.WriteAllText(Path.Combine(FileSystem.Current.CacheDirectory, "ruoka.json"), rawruoka);

               


                if (DateTime.Now.DayOfWeek == DayOfWeek.Monday)
                {
                    widgetView.SetTextColor(Resource.Id.Maanantai, color: Color.Orange);
                    //muut valkoseks
                    widgetView.SetTextColor(Resource.Id.Tiistai, color: Color.White);
                    widgetView.SetTextColor(Resource.Id.Keskiviikko, color: Color.White);
                    widgetView.SetTextColor(Resource.Id.Torstai, color: Color.White);
                    widgetView.SetTextColor(Resource.Id.Perjantai, color: Color.White);

                }
                else if(DateTime.Now.DayOfWeek == DayOfWeek.Tuesday)
                {
                    widgetView.SetTextColor(Resource.Id.Tiistai, color: Color.Orange);

                    //muut valkoseks
                    widgetView.SetTextColor(Resource.Id.Maanantai, color: Color.White);
                    widgetView.SetTextColor(Resource.Id.Keskiviikko, color: Color.White);
                    widgetView.SetTextColor(Resource.Id.Torstai, color: Color.White);
                    widgetView.SetTextColor(Resource.Id.Perjantai, color: Color.White);
                }
                else if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday)
                {
                    widgetView.SetTextColor(Resource.Id.Keskiviikko, color: Color.Orange);

                    //muut valkoseks
                    widgetView.SetTextColor(Resource.Id.Tiistai, color: Color.White);
                    widgetView.SetTextColor(Resource.Id.Maanantai, color: Color.White);
                    widgetView.SetTextColor(Resource.Id.Torstai, color: Color.White);
                    widgetView.SetTextColor(Resource.Id.Perjantai, color: Color.White);
                }
                else if (DateTime.Now.DayOfWeek == DayOfWeek.Thursday)
                {
                    widgetView.SetTextColor(Resource.Id.Torstai, color: Color.Orange);
                    //muut valkoseks
                    widgetView.SetTextColor(Resource.Id.Tiistai, color: Color.White);
                    widgetView.SetTextColor(Resource.Id.Keskiviikko, color: Color.White);
                    widgetView.SetTextColor(Resource.Id.Maanantai, color: Color.White);
                    widgetView.SetTextColor(Resource.Id.Perjantai, color: Color.White);
                }
                else if (DateTime.Now.DayOfWeek == DayOfWeek.Friday)
                {
                    widgetView.SetTextColor(Resource.Id.Perjantai, color: Color.Orange);
                    //muut valkoseks
                    widgetView.SetTextColor(Resource.Id.Tiistai, color: Color.White);
                    widgetView.SetTextColor(Resource.Id.Keskiviikko, color: Color.White);
                    widgetView.SetTextColor(Resource.Id.Torstai, color: Color.White);
                    widgetView.SetTextColor(Resource.Id.Maanantai, color: Color.White);
                }

            }
            catch (Exception)
            {
                if (File.Exists(Path.Combine(FileSystem.Current.CacheDirectory, "ruoka.json")) && File.ReadAllText(Path.Combine(FileSystem.Current.CacheDirectory, "ruoka.json")) != null)
                {
                    
                    ruoka = JsonConvert.DeserializeObject<Ruokalista>(File.ReadAllText(Path.Combine(FileSystem.Current.CacheDirectory, "ruoka.json")));
                    paikallinen = true;
                }
                else
                {
                    ruoka = new Ruokalista() { Maanantai = "Error", Tiistai = "Error", Keskiviikko = "Error", Torstai = "Error"};
                }
            }

            if (!paikallinen)
            {
                widgetView.SetTextViewText(RuokalistaApp.Resource.Id.textView1, "Päivitetty: " + DateTime.Now.ToString("dd/MM/ HH:mm"));
            }
            else
            {
                widgetView.SetTextViewText(RuokalistaApp.Resource.Id.textView1, "Päivitetty: " + DateTime.Now.ToString("dd/MM/ HH:mm") + " (Paikallinen versio)");
            }
            


            widgetView.SetTextViewText(RuokalistaApp.Resource.Id.Maanantai, "Maanantai\n" + ruoka.Maanantai);
            widgetView.SetTextViewText(RuokalistaApp.Resource.Id.Tiistai, "Tiistai\n" + ruoka.Tiistai);
            widgetView.SetTextViewText(RuokalistaApp.Resource.Id.Keskiviikko, "Keskiviikko\n" + ruoka.Keskiviikko);
            widgetView.SetTextViewText(RuokalistaApp.Resource.Id.Torstai, "Torstai\n" + ruoka.Torstai);
            widgetView.SetTextViewText(RuokalistaApp.Resource.Id.Perjantai, "Perjantai\n" + ruoka.Perjantai);


        }

        private void RegisterClicks(Context context, int[] appWidgetIds, RemoteViews widgetView)
        {
            var intent = new Intent(context, typeof(AppWidget));
            intent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
            intent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, appWidgetIds);

            // Register click event for the Background
            var piBackground = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.Immutable);
            widgetView.SetOnClickPendingIntent(RuokalistaApp.Resource.Id.widgetBackground, piBackground);

            // Register click event for the Announcement-icon

        }

        private PendingIntent GetPendingSelfIntent(Context context, string action)
        {
            var intent = new Intent(context, typeof(AppWidget));
            intent.SetAction(action);
            return PendingIntent.GetBroadcast(context, 0, intent, 0);
        }

        /// <summary>
        /// This method is called when clicks are registered.
        /// </summary>
        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);

            // Check if the click is from the "Announcement" button
            if (AnnouncementClick.Equals(intent.Action))
            {
                var pm = context.PackageManager;
                try
                {
                    var packageName = "com.android.settings";
                    var launchIntent = pm.GetLaunchIntentForPackage(packageName);
                    context.StartActivity(launchIntent);
                }
                catch
                {
                    // Something went wrong :)
                }
            }
        }
    }
}