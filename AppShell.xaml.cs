using RuokalistaApp.Pages;

namespace RuokalistaApp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute("LoginPage", typeof(LoginPage));
        Routing.RegisterRoute("LuoUusi", typeof(CreateNewPage));
        Routing.RegisterRoute("Tiedot", typeof(InfoPage));
        Routing.RegisterRoute("Muokkaa", typeof(EditPage));
        Routing.RegisterRoute("SeuraavaViikko", typeof(NextWeekPage));
        Routing.RegisterRoute("Main", typeof(MainPage));
        Routing.RegisterRoute("Welcome", typeof(WelcomePage));

		

		if (Preferences.Default.Get("IsAdmin", false))
		{
            tabbar.Items.Add(new ShellContent()
            {
                Title = "Admin",
                Route = "AdminPage",
                Icon = "admin.png",
                ContentTemplate = new DataTemplate(() => new AdminPage())
            });
        }

        if(Preferences.Get("PiilotaKasvis", false))
        {
            tabbar.Items.Remove(KasvisruokaTab);
        }
		
        
    }
}
