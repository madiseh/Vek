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
                Assert.Contains(Descendants<TextBlock>(window), text => text.Text == "This guide is being prepared." && text.Style is not null);
                return true;
            }
            finally
            {
                window.Close();
            }
        });
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
            try
            {
                return app.ProductProvider;
            }
            finally
            {
                app.Shutdown();
            }
        });

        Assert.NotNull(provider);
        var products = await provider.GetProductsAsync();

        Assert.Contains(products, product => product.Id == 1 && product.Name == "Desk Lamp");
    }

    private static Task<T> RunOnStaThreadAsync<T>(Func<T> action)
    {
        var completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        var thread = new Thread(() =>
        {
            try
            {
                completion.SetResult(action());
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return completion.Task;
    }
}
