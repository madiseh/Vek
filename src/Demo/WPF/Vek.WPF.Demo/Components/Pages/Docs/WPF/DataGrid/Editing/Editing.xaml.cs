namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Editing;

public partial class Editing : UserControl
{
    public Editing() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
