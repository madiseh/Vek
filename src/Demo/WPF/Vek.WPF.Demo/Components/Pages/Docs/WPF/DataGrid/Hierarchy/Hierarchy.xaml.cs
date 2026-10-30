namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Hierarchy;

public partial class Hierarchy : UserControl
{
    public Hierarchy() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
