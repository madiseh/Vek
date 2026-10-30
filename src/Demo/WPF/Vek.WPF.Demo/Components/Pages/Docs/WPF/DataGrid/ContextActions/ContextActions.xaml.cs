namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.ContextActions;

public partial class ContextActions : UserControl
{
    public ContextActions() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
