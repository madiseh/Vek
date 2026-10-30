namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Grouping;

public partial class Grouping : UserControl
{
    public Grouping() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
