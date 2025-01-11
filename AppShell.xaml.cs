using RuokalistaApp.Pages;

namespace RuokalistaApp;

public partial class AppShell : Shell
{
	public static TabBar PublicTabBar;
	public AppShell()
	{
		InitializeComponent();
		PublicTabBar = tabbar;

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
