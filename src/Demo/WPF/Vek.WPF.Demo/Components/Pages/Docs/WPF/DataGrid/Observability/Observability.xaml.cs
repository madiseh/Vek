namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Observability;

public partial class Observability : UserControl
{
    public Observability() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
