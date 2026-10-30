namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.ColumnLayout;

public partial class ColumnLayout : UserControl
{
    public ColumnLayout() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
