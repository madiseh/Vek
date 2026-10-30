using Vek.Blazor.DataGrid.Components.DataGrid;
using Vek.Demo.Abstractions;

namespace Vek.Demo.Components.Pages.Docs.Blazor.DataGrid.ServerData;

public partial class ServerData
{
    private const string RazorExample = """
        <VekDataGrid TItem="DemoProduct" ServerData="LoadServerPageAsync" PageSize="5">
            <PropertyColumn Property="product => product.Name" Title="Name" />
            <PropertyColumn Property="product => product.Category" Title="Category" />
            <PropertyColumn Property="product => product.Price" Title="Price" Format="C2" />
        </VekDataGrid>
        """;

    private const string CodeBehindExample = """
        using Vek.Components.Components.DataGrid;
        using Vek.Demo.Abstractions;

        private async Task<VekDataGridData<DemoProduct>> LoadServerPageAsync(
            VekDataGridState<DemoProduct> state,
            CancellationToken cancellationToken)
        {
            var products = await ProductProvider.GetProductsAsync(cancellationToken);
            var matching = ApplyFilters(products, state.Filters);
            var ordered = ApplySorts(matching, state.Sorts);
            var page = ordered.Skip((state.Page - 1) * state.PageSize).Take(state.PageSize).ToArray();
            return new VekDataGridData<DemoProduct>(page, ordered.Count);
        }
        """;

    [Inject]
    private IDemoProductProvider ProductProvider { get; set; } = default!;

    private readonly IReadOnlyList<VekCodeViewTab> _codeSamples =
    [
        new("Sample.razor", VekCodeLanguage.Html, RazorExample),
        new("Sample.razor.cs", VekCodeLanguage.CSharp, CodeBehindExample)
    ];

    private async Task<VekDataGridData<DemoProduct>> LoadServerPageAsync(
        VekDataGridState<DemoProduct> state,
        CancellationToken cancellationToken)
    {
        var products = await ProductProvider.GetProductsAsync(cancellationToken);
        IEnumerable<DemoProduct> matching = products;
        foreach (var filter in state.Filters)
        {
            matching = ApplyFilter(matching, filter);
        }

        var ordered = ApplySorts(matching, state.Sorts).ToArray();
        var page = ordered.Skip((state.Page - 1) * state.PageSize).Take(state.PageSize).ToArray();
        return new VekDataGridData<DemoProduct>(page, ordered.Length);
    }

    private static IEnumerable<DemoProduct> ApplyFilter(
        IEnumerable<DemoProduct> products,
        VekDataGridFilter filter)
    {
        var comparison = filter.CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var value = filter.Value?.ToString() ?? string.Empty;
        return filter.ColumnKey switch
        {
            "Name" => products.Where(product => MatchesText(product.Name, filter.Operator, value, comparison)),
            "Category" => products.Where(product => MatchesText(product.Category, filter.Operator, value, comparison)),
            "Price" when decimal.TryParse(value, out var price) => products.Where(product =>
                filter.Operator switch
                {
                    VekDataGridFilterOperator.GreaterThan => product.Price > price,
                    VekDataGridFilterOperator.GreaterThanOrEqual => product.Price >= price,
                    VekDataGridFilterOperator.LessThan => product.Price < price,
                    VekDataGridFilterOperator.LessThanOrEqual => product.Price <= price,
                    VekDataGridFilterOperator.Equals => product.Price == price,
                    VekDataGridFilterOperator.NotEquals => product.Price != price,
                    _ => true
                }),
            _ => products
        };
    }

    private static bool MatchesText(string text, VekDataGridFilterOperator op, string value, StringComparison comparison)
        => op switch
        {
            VekDataGridFilterOperator.Contains => text.Contains(value, comparison),
            VekDataGridFilterOperator.StartsWith => text.StartsWith(value, comparison),
            VekDataGridFilterOperator.EndsWith => text.EndsWith(value, comparison),
            VekDataGridFilterOperator.Equals => string.Equals(text, value, comparison),
            VekDataGridFilterOperator.NotEquals => !string.Equals(text, value, comparison),
            _ => true
        };

    private static IEnumerable<DemoProduct> ApplySorts(
        IEnumerable<DemoProduct> products,
        IReadOnlyList<VekDataGridSort> sorts)
    {
        IOrderedEnumerable<DemoProduct>? ordered = null;
        foreach (var sort in sorts.OrderBy(sort => sort.Priority))
        {
            Func<DemoProduct, object?> key = sort.ColumnKey switch
            {
                "Name" => product => product.Name,
                "Category" => product => product.Category,
                "Price" => product => product.Price,
                _ => product => product.Id
            };
            ordered = ordered is null
                ? sort.Direction == VekSortDirection.Ascending
                    ? products.OrderBy(key)
                    : products.OrderByDescending(key)
                : sort.Direction == VekSortDirection.Ascending
                    ? ordered.ThenBy(key)
                    : ordered.ThenByDescending(key);
        }

        return ordered ?? products.OrderBy(product => product.Id);
    }

}
