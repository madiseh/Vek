namespace Vek.Demo.Abstractions;

public interface IDemoProductProvider
{
    Task<IReadOnlyList<DemoProduct>> GetProductsAsync(CancellationToken cancellationToken = default);
}
