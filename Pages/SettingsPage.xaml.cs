using System;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using RuokalistaApp.Models;
using Android.OS;
using Microsoft.Maui;
using static System.Net.Mime.MediaTypeNames;
using Application = Microsoft.Maui.Controls.Application;

#if ANDROID
using Android.Content;
using Android.Provider;
using Firebase.Messaging;
#endif


namespace RuokalistaApp.Pages;


public partial class SettingsPage : ContentPage
{
	private bool _isInitializing = true;
	public SettingsPage()
	{
		InitializeComponent();

        CopyrightLabel.Text = $"© {DateTime.Now.Year} Kouluruokalista.fi";

		ThemePicker.SelectedIndex = Preferences.Default.Get("Teema", 0);
        
		if(Preferences.Get("kasvisruokalistaEnabled", true))
		{
			PiilotaKasvisruokalista.IsToggled = Preferences.Get("NaytaKasvis", true);
		}
		

		_isInitializing = false;
	}

	

   

    private void picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        

       if(ThemePicker.SelectedIndex == 1)
        {
            Application.Current.UserAppTheme = AppTheme.Dark;
            Preferences.Default.Set("Teema", 1);
        }
       else if(ThemePicker.SelectedIndex == 2)
        {
            Application.Current.UserAppTheme = AppTheme.Light;
            Preferences.Default.Set("Teema", 2);
        }
        else
        {
            Application.Current.UserAppTheme = AppTheme.Unspecified;
            Preferences.Default.Set("Teema", 0);
        }

       
    }

    

	private async void OnLabelTapped(object sender, EventArgs e)
	{
		var label = sender as Label;
		var argument = (string)((TapGestureRecognizer)label.GestureRecognizers[0]).CommandParameter;

		// Handle the tap event and use the argument
		if(argument == "kouluruokalista.fi")
        {
			await Launcher.OpenAsync(new Uri("https://kouluruokalista.fi"));

		}
       
	}


	private void NotifBtn_Clicked(object sender, EventArgs e)
	{
#if ANDROID
        try
        {
			var intent = new Intent(Settings.ActionAppNotificationSettings);
			intent.PutExtra(Settings.ExtraAppPackage, Android.App.Application.Context.PackageName);
			intent.AddFlags(ActivityFlags.NewTask);
			Android.App.Application.Context.StartActivity(intent);
		}
        catch (Exception)
        {
            DisplayAlert("Virhe", "Ilmoitusasetusten avaaminen ei onnistunut", "OK");
		}
#endif
	}

	private async void PiilotaKasvisruokalista_Toggled(object sender, ToggledEventArgs e)
	{
		//prevent the event from firing when the page is initializing
		if (_isInitializing) return;

		//if kasvisruokalista is not disabled on the server, prevent the user from enabling it
		if (!Preferences.Get("kasvisruokalistaEnabled", true))
		{
			PiilotaKasvisruokalista.IsToggled = false;

			string text = "Ominaisuus ei ole saatavilla tällä palvelimella";
			ToastDuration duration = ToastDuration.Long;
			double fontSize = 15;

			var toast = Toast.Make(text, duration, fontSize);

			await toast.Show();
			return;
		}

		//show the page
		Preferences.Set("NaytaKasvis", e.Value);
		App.Current.MainPage = new AppShell();
	}

	private void ChangeSchoolBtn_Clicked(object sender, EventArgs e)
	{
		//remove all currently set environment variables
		Preferences.Clear();
		//unregister notif channel
		//TODO!!!!!!!!!!!!!!!!!!!!!!!!!!
		//throw back to welcome
		
		App.SetCurrentAppColor(Config.PrimaryFallbackColor);
		Application.Current.MainPage = new WelcomePage();

	}
}