namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Virtualization;

public partial class Virtualization : UserControl
{
    public Virtualization() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
