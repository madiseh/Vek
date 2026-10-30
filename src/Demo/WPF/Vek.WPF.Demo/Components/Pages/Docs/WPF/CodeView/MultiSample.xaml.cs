namespace Vek.WPF.Demo.Components.Pages.Docs.WPF.CodeView;

public partial class MultiSample : UserControl
{
    public MultiSample() => InitializeComponent();

    private void NavigateToSection(object sender, RoutedEventArgs args)
    {
        if (sender is Button { Tag: FrameworkElement target }) target.BringIntoView();
    }
}
