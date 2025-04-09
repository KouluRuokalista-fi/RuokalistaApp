namespace RuokalistaApp.Pages;

public partial class VotePage : ContentPage
{
	public VotePage()
	{
		InitializeComponent();
		var mode = "dark";
		if (Application.Current.RequestedTheme == AppTheme.Light)
		{
			mode = "light";
		}


		webview1.Source = Preferences.Get("School", "") + $"/Aanestys/Tulokset?isApp=true&mode={mode}";
	}

    private void RefreshView_Refreshing(object sender, EventArgs e)
    {
		webview1.Reload();
		RefreshView1.IsRefreshing = false;
    }
}