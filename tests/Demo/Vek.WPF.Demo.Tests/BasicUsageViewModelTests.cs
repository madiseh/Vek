using Vek.Demo.Abstractions;
using Vek.WPF.Demo.ViewModels;

namespace Vek.WPF.Demo.Tests;

public sealed class BasicUsageViewModelTests
{
    /// <summary>
    /// The Basic Usage sample displays the products supplied by the shared demo provider.
    /// </summary>
    [Fact]
    public async Task BasicUsageSampleLoadsProductsFromTheSharedProvider()
    {
        var products = Enumerable.Range(1, 6)
            .Select(id => new DemoProduct(id, $"Product {id}", "Lighting", id * 10m))
            .ToArray();
        var provider = new FixedDemoProductProvider(products);
        var viewModel = new BasicUsageViewModel(provider);

        await viewModel.LoadProductsAsync();

        Assert.Equal(5, viewModel.Products.Count);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, viewModel.Products.Select(product => product.Id));
        Assert.Equal(1, provider.RequestCount);
    }

    /// <summary>
    /// Re-entering Basic Usage shares one provider load instead of duplicating its product request.
    /// </summary>
    [Fact]
    public async Task RepeatedLoadRequestsUseOneProviderRequest()
    {
        var products = Enumerable.Range(1, 5)
            .Select(id => new DemoProduct(id, $"Product {id}", "Lighting", id * 10m))
            .ToArray();
        var provider = new FixedDemoProductProvider(products);
        var viewModel = new BasicUsageViewModel(provider);

        await viewModel.LoadProductsAsync();
        await viewModel.LoadProductsAsync();

        Assert.Equal(1, provider.RequestCount);
        Assert.Equal(5, viewModel.Products.Count);
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
}
