namespace Vek.WPF.Demo.Components.Pages.Docs.WPF;

public partial class GettingStarted : UserControl
{
    public GettingStarted() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
