using System.Text.Json;
using Vek.Demo.Abstractions;

namespace Vek.SimulatedData;

public sealed class JsonDemoProductProvider : IDemoProductProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<DemoProduct>> GetProductsAsync(CancellationToken cancellationToken = default)
    {
        var dataPath = Path.Combine(AppContext.BaseDirectory, "products.json");

        await using var stream = File.OpenRead(dataPath);
        var products = await JsonSerializer.DeserializeAsync<List<DemoProduct>>(
            stream,
            SerializerOptions,
            cancellationToken);

        if (products is null)
        {
            return Array.Empty<DemoProduct>();
        }

        return products;
    }
}
