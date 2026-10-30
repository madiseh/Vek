namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Selection;

public partial class Selection : UserControl
{
    public Selection() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
