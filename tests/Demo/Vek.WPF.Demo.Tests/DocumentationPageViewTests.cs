namespace Vek.WPF.Demo.Tests;

public sealed class DocumentationPageViewTests
{
    /// <summary>
    /// The Basic Usage sample loads products when an already-loaded documentation view changes pages.
    /// </summary>
    [Fact]
    public async Task BasicUsageLoadsProductsWhenPageChangesAfterViewIsLoaded()
    {
        var state = await RunOnStaThreadAsync(() =>
        {
            var provider = new FixedDemoProductProvider(
                Enumerable.Range(1, 5).Select(id => new DemoProduct(id, $"Product {id}", "Lighting", id * 10m)).ToArray());
            var viewModel = new BasicUsageViewModel(provider);
            var gettingStarted = DemoPageCatalog.All.Single(page => page.Id == "getting-started");
            var basicUsage = DemoPageCatalog.All.Single(page => page.Id == "vekdatagrid/basic-usage");
            var view = new DocumentationPageView
            {
                Page = gettingStarted,
                BasicUsage = viewModel,
                DataContext = gettingStarted
            };
            var window = new Window { Content = view };

            try
            {
                window.Show();
                window.UpdateLayout();

                view.Page = basicUsage;
                view.DataContext = basicUsage;
                window.UpdateLayout();

                var frame = new DispatcherFrame();
                var loadedProbe = new DispatcherTimer(DispatcherPriority.Background, window.Dispatcher)
                {
                    Interval = TimeSpan.FromMilliseconds(25)
                };
                loadedProbe.Tick += (_, _) =>
                {
                    if (viewModel.Products.Count == 5)
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

                return (viewModel.Products.Count, provider.RequestCount);
            }
            finally
            {
                window.Close();
            }
        });

        Assert.Equal(5, state.Count);
        Assert.Equal(1, state.RequestCount);
    }

    private sealed class FixedDemoProductProvider(IReadOnlyList<DemoProduct> products) : IDemoProductProvider
    {
        public int RequestCount { get; private set; }

        public Task<IReadOnlyList<DemoProduct>> GetProductsAsync(CancellationToken cancellationToken = default)
        {
            RequestCount++;
            return Task.FromResult(products);
        }
    }

    private static Task<T> RunOnStaThreadAsync<T>(Func<T> action)
        => WpfTestDispatcher.RunAsync(action);
}
