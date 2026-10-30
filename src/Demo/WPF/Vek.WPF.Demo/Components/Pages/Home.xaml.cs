namespace Vek.WPF.Demo.Components.Pages;

public partial class Home : UserControl
{
    public Home()
    {
        InitializeComponent();
    }

    private void OpenRepository(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}
