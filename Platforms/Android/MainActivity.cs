using Android.App;
using Android.Content.PM;
using Android.OS;
using Firebase.Messaging;

namespace RuokalistaApp;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density, LaunchMode = LaunchMode.SingleTop)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        CreateNotificationChannel();

	}



	private void CreateNotificationChannel()
    {
        var isFirstTime = Preferences.Get("IsFirstTime", true);

        if(isFirstTime)
        {
			//poista vanha ruokalista firebasetopic
			FirebaseMessaging.Instance.UnsubscribeFromTopic("Ilmoitukset");

			FirebaseMessaging.Instance.SubscribeToTopic("Isokyro");

			if (OperatingSystem.IsOSPlatformVersionAtLeast("android", 26))
			{
				var notificationmanager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);

                //poista vanha ruokalista kanava
                try
                {
					notificationmanager.DeleteNotificationChannel("Main");
				}
                catch(Exception)
                { }

				notificationmanager.CreateNotificationChannel(new NotificationChannel("Ruokalista", "Ruokalista", NotificationImportance.Default));
				notificationmanager.CreateNotificationChannel(new NotificationChannel("Kasvisruokalista", "Kasvisruokalista", NotificationImportance.None));
				notificationmanager.CreateNotificationChannel(new NotificationChannel("Yleiset", "Yleiset", NotificationImportance.Default));


			}

			Preferences.Set("IsFirstTime", false);
		}

		
        
    }
}
