namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Aggregation;

public partial class Aggregation : UserControl
{
    public Aggregation() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
