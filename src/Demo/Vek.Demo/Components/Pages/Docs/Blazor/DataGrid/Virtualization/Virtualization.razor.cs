using Vek.Blazor.DataGrid.Components.DataGrid;
using Vek.Demo.Abstractions;

namespace Vek.Demo.Components.Pages.Docs.Blazor.DataGrid.Virtualization;

public partial class Virtualization
{
    private const string RazorExample = """
        <VekDataGrid TItem="DemoProduct" Items="_products" VirtualizeRows="true"
                     VirtualItemSize="48" VirtualOverscanCount="4">
            <PropertyColumn Property="product => product.Name" Title="Name" />
            <PropertyColumn Property="product => product.Category" Title="Category" />
            <PropertyColumn Property="product => product.Price" Title="Price" Format="C2" />
        </VekDataGrid>

        <VekDataGrid TItem="DemoProduct" VirtualizeRows="true"
                     ServerVirtualData="LoadVirtualRangeAsync" VirtualItemSize="48" />
        """;

    private const string CodeBehindExample = """
        private async Task<VekDataGridData<DemoProduct>> LoadVirtualRangeAsync(
            VekDataGridState<DemoProduct> state,
            int startIndex,
            int count,
            CancellationToken cancellationToken)
        {
            var matching = await ProductSource.QueryAsync(state, cancellationToken);
            var items = matching.Skip(startIndex).Take(count).ToArray();
            return new VekDataGridData<DemoProduct>(items, matching.Count);
        }
        """;

    private static readonly string[] Categories = ["Furniture", "Stationery", "Lighting", "Storage"];
    private static readonly IReadOnlyList<DemoProduct> Products = Enumerable.Range(1, 500)
        .Select(id => new DemoProduct(
            id,
            $"Product {id:D3}",
            Categories[(id - 1) % Categories.Length],
            5m + ((id * 17) % 250)))
        .ToArray();

    private IReadOnlyList<DemoProduct> _products => Products;
    private readonly IReadOnlyList<VekCodeViewTab> _codeSamples =
    [
        new("Sample.razor", VekCodeLanguage.Html, RazorExample),
        new("Sample.razor.cs", VekCodeLanguage.CSharp, CodeBehindExample)
    ];

    private Task<VekDataGridData<DemoProduct>> LoadVirtualRangeAsync(
        VekDataGridState<DemoProduct> state,
        int startIndex,
        int count,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IEnumerable<DemoProduct> matching = Products;
        foreach (var filter in state.Filters)
        {
            var value = filter.Value?.ToString() ?? string.Empty;
            matching = filter.ColumnKey switch
            {
                "Name" when filter.Operator == VekDataGridFilterOperator.Contains =>
                    matching.Where(product => product.Name.Contains(value, filter.CaseSensitive
                        ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)),
                "Category" when filter.Operator == VekDataGridFilterOperator.Contains =>
                    matching.Where(product => product.Category.Contains(value, filter.CaseSensitive
                        ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase)),
                _ => matching
            };
        }

        IOrderedEnumerable<DemoProduct>? ordered = null;
        foreach (var sort in state.Sorts.OrderBy(sort => sort.Priority))
        {
            Func<DemoProduct, object?> selector = sort.ColumnKey switch
            {
                "Name" => product => product.Name,
                "Category" => product => product.Category,
                "Price" => product => product.Price,
                _ => product => product.Id
            };
            ordered = ordered is null
                ? sort.Direction == VekSortDirection.Ascending ? matching.OrderBy(selector) : matching.OrderByDescending(selector)
                : sort.Direction == VekSortDirection.Ascending ? ordered.ThenBy(selector) : ordered.ThenByDescending(selector);
        }

        var results = (ordered ?? matching.OrderBy(product => product.Id)).ToArray();
        var items = results.Skip(startIndex).Take(count).ToArray();
        return Task.FromResult(new VekDataGridData<DemoProduct>(items, results.Length));
    }

}
