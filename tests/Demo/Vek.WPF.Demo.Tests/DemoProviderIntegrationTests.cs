using Vek.Demo.Abstractions;
using Vek.WPF.DataGrid.Components;
using Vek.WPF.Demo.ViewModels;
using Vek.SimulatedData;

namespace Vek.WPF.Demo.Tests;

public sealed class DemoProviderIntegrationTests
{
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
                var navigation = Assert.Single(Descendants<ListBox>(window));
                Assert.NotNull(navigation.ItemContainerStyle);
                Assert.Equal("getting-started", Assert.IsType<DemoPageDefinition>(navigation.SelectedItem).Id);
                Assert.Equal(DemoPageImplementationStatus.Implemented,
                    Assert.IsType<DemoPageDefinition>(navigation.SelectedItem).ImplementationStatus);
                Assert.Contains(Descendants<TextBox>(window), text => text.Text.Contains("<UseWPF>true</UseWPF>"));
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
