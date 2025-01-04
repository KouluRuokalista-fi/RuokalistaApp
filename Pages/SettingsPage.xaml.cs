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
    private static int SecretCounter = 0;
	public SettingsPage()
	{
		InitializeComponent();

        CopyrightLabel.Text = $"© {DateTime.Now.Year} Arttu Kuikka";

		ThemePicker.SelectedIndex = Preferences.Default.Get("Teema", 0);
        

		PiilotaKasvisruokalista.IsToggled = Preferences.Get("PiilotaKasvis", false);
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

	private void PiilotaKasvisruokalista_Toggled(object sender, ToggledEventArgs e)
	{
		Preferences.Set("PiilotaKasvis", e.Value);
		//TODO: application.jotain et tekee heti eikä uudelleenkäynnistyksen jälkee

	}

	private void ChangeSchoolBtn_Clicked(object sender, EventArgs e)
	{
		//remove all currently set environment variables
		//unregister notif channel
		//throw back to welcome

		var color = "#05fc1d";
		Application.Current.Resources["Primary"] = Color.FromArgb(color);
	}
}