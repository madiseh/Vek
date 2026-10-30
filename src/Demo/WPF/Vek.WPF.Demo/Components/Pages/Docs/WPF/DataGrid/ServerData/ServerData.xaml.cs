namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.ServerData;

public partial class ServerData : UserControl
{
    public ServerData() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
