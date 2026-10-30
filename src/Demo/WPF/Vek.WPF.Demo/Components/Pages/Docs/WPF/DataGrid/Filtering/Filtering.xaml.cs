namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Filtering;

public partial class Filtering : UserControl
{
    public Filtering() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
