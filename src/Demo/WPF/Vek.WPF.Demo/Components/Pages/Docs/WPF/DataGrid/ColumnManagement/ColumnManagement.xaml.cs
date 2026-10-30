namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.ColumnManagement;

public partial class ColumnManagement : UserControl
{
    public ColumnManagement() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
