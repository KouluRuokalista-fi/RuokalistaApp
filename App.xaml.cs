using RuokalistaApp.Pages;

namespace RuokalistaApp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new WelcomePage();
		if (Preferences.Default.ContainsKey("Teema"))
		{
			var key = Preferences.Default.Get("Teema", 0);
            if(key == 0)
			{
				Application.Current.UserAppTheme = AppTheme.Unspecified;
			}
			else if(key == 1)
			{
                Application.Current.UserAppTheme = AppTheme.Dark;
            }
			else if(key == 2)
			{
                Application.Current.UserAppTheme = AppTheme.Light;
            }
        }
		
    }
}
