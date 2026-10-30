namespace Vek.WPF.Demo.Tests;

public sealed class DemoProviderIntegrationTests
{
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
