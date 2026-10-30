namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Presentation;

public partial class Presentation : UserControl
{
    public Presentation() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
