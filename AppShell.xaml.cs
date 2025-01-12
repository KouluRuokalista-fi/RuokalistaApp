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

        
		
        
    }
}
