using System.Diagnostics;
using System.Windows.Navigation;

namespace Vek.WPF.Demo.Components.Pages;

public partial class Home : UserControl
{
    public Home()
    {
        InitializeComponent();
    }

    private void OpenRepository(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
        e.Handled = true;
    }
}
