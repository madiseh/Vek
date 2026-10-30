namespace Vek.WPF.Demo.Views;

public partial class DocumentationView : UserControl
{
    public DocumentationView()
    {
        InitializeComponent();
    }

    private void NavigationSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // A navigation destination stays active when Ctrl+click/Space clears ListBox selection.
        if (sender is ListBox { SelectedItem: null } navigation && DataContext is MainWindowViewModel model)
        {
            navigation.SetCurrentValue(ListBox.SelectedItemProperty, model.SelectedPage);
        }
    }
}
