namespace Vek.WPF.Demo.Tests;

public sealed class DemoProviderIntegrationTests
{
    /// <summary>
    /// Deselecting a sidebar item preserves the active page and restores the visible selection without binding errors.
    /// </summary>
    [Fact]
    public async Task ClearingDocumentationSelectionPreservesTheActivePageWithoutBindingErrors()
    {
        await RunOnStaThreadAsync(() =>
        {
            using var errors = new System.IO.StringWriter();
            using var listener = new System.Diagnostics.TextWriterTraceListener(errors);
            var previousLevel = System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level;
            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Error;
            System.Diagnostics.PresentationTraceSources.DataBindingSource.Listeners.Add(listener);
            var window = new MainWindow();
            try
            {
                window.Show();
                var model = Assert.IsType<MainWindowViewModel>(window.DataContext);
                model.GetStartedCommand.Execute(null);
                window.UpdateLayout();
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                var navigation = Assert.Single(Descendants<TreeView>(window));
                var selectedPage = model.SelectedPage;

                Assert.IsType<TreeViewItem>(navigation.SelectedItem).IsSelected = false;
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                window.UpdateLayout();

                Assert.Same(selectedPage, model.SelectedPage);
                Assert.Same(selectedPage, Assert.IsType<TreeViewItem>(navigation.SelectedItem).Tag);
                var sorting = model.Pages.Single(page => page.Id == "vekdatagrid/sorting");
                var components = Assert.IsType<TreeViewItem>(navigation.Items[1]);
                components.IsExpanded = true;
                window.UpdateLayout();
                var group = Assert.IsType<TreeViewItem>(components.Items[0]);
                group.Items.Cast<TreeViewItem>().Single(item => Equals(item.Tag, sorting)).IsSelected = true;
                Assert.Same(sorting, model.SelectedPage);
                Assert.Empty(errors.ToString());
                return true;
            }
            finally
            {
                window.Close();
                System.Diagnostics.PresentationTraceSources.DataBindingSource.Listeners.Remove(listener);
                System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = previousLevel;
            }
        });
    }

    /// <summary>
    /// Home exposes working documentation navigation, the repository action, and the reference's gradient visual hierarchy.
    /// </summary>
    [Fact]
    public async Task HomeOffersDocumentationAndGitHubActionsWithReferenceVisuals()
    {
        await RunOnStaThreadAsync(() =>
        {
            var window = new MainWindow();
            try
            {
                window.Show();
                window.UpdateLayout();
                var home = Assert.Single(Descendants<HomeView>(window));
                var repositoryText = Assert.Single(Descendants<TextBlock>(home)
                    .Where(text => text.Inlines.OfType<System.Windows.Documents.Hyperlink>().Any()));
                var repository = Assert.Single(repositoryText.Inlines.OfType<System.Windows.Documents.Hyperlink>());
                Assert.Equal("Star on GitHub", new System.Windows.Documents.TextRange(repository.ContentStart, repository.ContentEnd).Text);
                Assert.Equal("https://github.com/madiseh/Vek", repository.NavigateUri.AbsoluteUri);
                Assert.True(repository.Focusable);
                Assert.True(repository.IsEnabled);
                Assert.Contains(Descendants<Grid>(home), grid => grid.Background is RadialGradientBrush);
                Assert.Contains(Descendants<TextBlock>(home), text => text.Text == "Built for WPF." && text.Foreground is LinearGradientBrush);
                Assert.Contains(Descendants<System.Windows.Shapes.Path>(home), path => path.Fill is LinearGradientBrush && !path.Data.IsEmpty());

                var start = Assert.Single(Descendants<Button>(home));
                Assert.NotNull(start.Command);
                start.Command.Execute(null);
                window.UpdateLayout();
                Assert.Equal("getting-started", Assert.IsType<MainWindowViewModel>(window.DataContext).SelectedPage.Id);
                Assert.Single(Descendants<DocumentationView>(window));
                return true;
            }
            finally
            {
                window.Close();
            }
        });
    }

    /// <summary>
    /// The real window loads Home and documentation with theme resources available during view creation.
    /// </summary>
    [Fact]
    public async Task MainWindowLoadsHomeAndDocumentationWithoutResourceErrors()
    {
        await RunOnStaThreadAsync(() =>
        {
            var window = new MainWindow();
            try
            {
                window.Show();
                window.UpdateLayout();
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);
                Assert.Single(Descendants<HomeView>(window));

                var button = Assert.Single(Descendants<Button>(window));
                Assert.NotNull(button.Command);
                button.Command.Execute(null);
                window.UpdateLayout();
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

                Assert.Single(Descendants<DocumentationView>(window));
                var navigation = Assert.Single(Descendants<TreeView>(window));
                var selected = Assert.IsType<TreeViewItem>(navigation.SelectedItem);
                Assert.NotNull(selected.Style);
                Assert.Equal("getting-started", Assert.IsType<DemoPageDefinition>(selected.Tag).Id);
                Assert.Equal(DemoPageImplementationStatus.Implemented,
                    Assert.IsType<DemoPageDefinition>(selected.Tag).ImplementationStatus);
                Assert.Contains(Descendants<TextBlock>(window), text => text.Text == "This guide is being prepared.");
                return true;
            }
            finally
            {
                window.Close();
            }
        });
    }

    /// <summary>
    /// The native Basic Usage grid displays the first five shared JSON products from the application provider.
    /// </summary>
    [Fact]
    public async Task MainWindowBasicUsageDisplaysFiveSharedProductsInTheNativeGrid()
    {
        var loadedProducts = await RunOnStaThreadAsync(() =>
        {
            var window = new MainWindow(new JsonDemoProductProvider());
            try
            {
                var viewModel = Assert.IsType<MainWindowViewModel>(window.DataContext);
                window.Show();
                window.UpdateLayout();
                viewModel.GetStartedCommand.Execute(null);
                window.UpdateLayout();

                Assert.Equal("getting-started", viewModel.SelectedPage.Id);
                viewModel.SelectPage(DemoPageCatalog.All.Single(page => page.Id == "vekdatagrid/basic-usage"));
                window.UpdateLayout();
                window.Dispatcher.Invoke(() => { }, DispatcherPriority.ApplicationIdle);

                var pageView = Assert.Single(Descendants<DocumentationPageView>(window));
                Assert.Equal("vekdatagrid/basic-usage", pageView.Page?.Id);
                Assert.NotNull(pageView.BasicUsage);
                var grid = Assert.Single(Descendants<VekDataGrid>(window));
                var frame = new DispatcherFrame();
                var loadedProbe = new DispatcherTimer(DispatcherPriority.Background, window.Dispatcher)
                {
                    Interval = TimeSpan.FromMilliseconds(25)
                };
                loadedProbe.Tick += (_, _) =>
                {
                    if (grid.Items.Count >= 5)
                    {
                        frame.Continue = false;
                    }
                };

                var timeout = new DispatcherTimer(DispatcherPriority.Background, window.Dispatcher)
                {
                    Interval = TimeSpan.FromSeconds(5)
                };
                timeout.Tick += (_, _) => frame.Continue = false;
                loadedProbe.Start();
                timeout.Start();
                Dispatcher.PushFrame(frame);
                loadedProbe.Stop();
                timeout.Stop();

                return (Names: grid.Items.OfType<DemoProduct>().Select(product => product.Name).ToArray(),
                    Count: viewModel.BasicUsage!.Products.Count,
                    Error: viewModel.BasicUsage.LoadErrorMessage);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.True(loadedProducts.Count == 5, loadedProducts.Error ?? "The Basic Usage product collection did not load.");
        Assert.Equal(new[] { "Desk Lamp", "Task Chair", "Notebook Set", "Monitor Stand", "Northstar LED Desk Lamp" }, loadedProducts.Names);
    }

    private static IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is T match)
            {
                yield return match;
            }

            foreach (var descendant in Descendants<T>(child))
            {
                yield return descendant;
            }
        }
    }

    /// <summary>
    /// The application composition root supplies products from the shared JSON data.
    /// </summary>
    [Fact]
    public async Task AppProviderLoadsTheSharedJsonProducts()
    {
        var provider = await RunOnStaThreadAsync(() =>
        {
            var app = new App();
            app.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            return app.ProductProvider;
        });

        Assert.NotNull(provider);
        var products = await provider.GetProductsAsync();

        Assert.Contains(products, product => product.Id == 1 && product.Name == "Desk Lamp");
    }

    private static Task<T> RunOnStaThreadAsync<T>(Func<T> action)
        => WpfTestDispatcher.RunAsync(action);
}
