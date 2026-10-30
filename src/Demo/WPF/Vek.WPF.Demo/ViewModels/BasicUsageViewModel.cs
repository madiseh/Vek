using System.Collections.ObjectModel;

namespace Vek.WPF.Demo.ViewModels;

public sealed class BasicUsageViewModel(IDemoProductProvider productProvider)
{
    public ObservableCollection<DemoProduct> Products { get; } = [];

    public async Task LoadProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await productProvider.GetProductsAsync(cancellationToken);
        Products.Clear();
        foreach (var product in products.Take(5))
        {
            Products.Add(product);
        }
    }
}
