namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Sorting;

public partial class Sorting : UserControl
{
    public Sorting() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
