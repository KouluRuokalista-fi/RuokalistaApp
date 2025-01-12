namespace RuokalistaApp.Pages;

public partial class VotePage : ContentPage
{
	public VotePage()
	{
		InitializeComponent();

		webview1.Source = Preferences.Get("School", "") + "/Aanestys/Tulokset?isApp=true";
	}

    private void RefreshView_Refreshing(object sender, EventArgs e)
    {
		webview1.Reload();
		RefreshView1.IsRefreshing = false;
    }
}