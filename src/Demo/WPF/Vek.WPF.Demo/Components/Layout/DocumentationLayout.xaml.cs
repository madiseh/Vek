namespace Vek.WPF.Demo.Components.Layout;

public partial class DocumentationLayout : UserControl
{
    private MainWindowViewModel? _model;

    public DocumentationLayout()
    {
        InitializeComponent();
        DocumentationNavigation.Items.Add(Destination(DemoPageCatalog.Documentation.Single(page => page.Id == "getting-started")));
        var components = new TreeViewItem { Header = "Components", IsExpanded = false };
        foreach (var name in new[] { "VekDataGrid", "VekCodeView" })
        {
            var group = new TreeViewItem { Header = name, IsExpanded = true };
            foreach (var page in DemoPageCatalog.Documentation.Where(page => page.Section == name)) group.Items.Add(Destination(page));
            components.Items.Add(group);
        }
        DocumentationNavigation.Items.Add(components);
        DataContextChanged += (_, args) =>
        {
            if (_model is not null) PropertyChangedEventManager.RemoveHandler(_model, SelectedPageChanged, nameof(MainWindowViewModel.SelectedPage));
            _model = args.NewValue as MainWindowViewModel;
            if (_model is not null) PropertyChangedEventManager.AddHandler(_model, SelectedPageChanged, nameof(MainWindowViewModel.SelectedPage));
            UpdateSelection();
        };
    }

    private static TreeViewItem Destination(DemoPageDefinition page) => new() { Header = page.Title, Tag = page };

    private void NavigationSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> args)
    {
        if (args.NewValue is TreeViewItem { Tag: DemoPageDefinition page }) _model?.SelectPage(page);
        else if (args.NewValue is null) Dispatcher.BeginInvoke(new Action(UpdateSelection));
    }

    private void NavigateHome(object sender, RoutedEventArgs args) =>
        _model?.SelectPage(DemoPageCatalog.All.Single(page => page.Id == "home"));

    private void SelectedPageChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(MainWindowViewModel.SelectedPage)) UpdateSelection();
    }

    private void UpdateSelection()
    {
        if (_model?.SelectedPage is { } currentPage)
        {
            var pageView = (UserControl)Activator.CreateInstance(currentPage.PageType)!;
            pageView.DataContext = currentPage;
            SelectedPageHost.Content = pageView;
        }

        foreach (var destination in Destinations(DocumentationNavigation))
            destination.IsSelected = destination.Tag is DemoPageDefinition page && page == _model?.SelectedPage;
    }

    private static IEnumerable<TreeViewItem> Destinations(ItemsControl parent)
    {
        foreach (TreeViewItem item in parent.Items)
        {
            if (item.Tag is DemoPageDefinition) yield return item;
            foreach (var child in Destinations(item)) yield return child;
        }
    }
}
