using Vek.Demo.Abstractions;
using Vek.Components.Components.DataGrid;

namespace Vek.Demo.Components.Pages.Docs.DataGrid.Filtering;

public partial class Filtering
{
    private const string RazorExample = """
        <VekDataGrid TItem="DemoProduct" Items="_products" FilterMode="VekDataGridFilterMode.ColumnRow"
                     FilterCaseSensitive="false" FilterChanged="OnFiltersChanged">
            <PropertyColumn Property="product => product.Name" Title="Name"
                            FilterOperator="VekDataGridFilterOperator.Contains" />
            <PropertyColumn Property="product => product.Price" Title="Price"
                            FilterOperator="VekDataGridFilterOperator.GreaterThanOrEqual" />
        </VekDataGrid>
        """;

    private const string CodeBehindExample = """
        using Microsoft.AspNetCore.Components;
        using Vek.Demo.Abstractions;
        using Vek.Components.Components.DataGrid;

        namespace MyApp.Components.Pages;

        public partial class Sample
        {
            [Inject]
            private IDemoProductProvider ProductProvider { get; set; } = default!;

            private IReadOnlyList<DemoProduct> _products = [];
            private IReadOnlyList<VekDataGridFilter> _filters = [];

            private void OnFiltersChanged(IReadOnlyList<VekDataGridFilter> filters)
                => _filters = filters;
        }
        """;

    [Inject]
    private IDemoProductProvider ProductProvider { get; set; } = default!;

    private IReadOnlyList<DemoProduct>? _products;
    private VekDataGridFilterMode _filterMode = VekDataGridFilterMode.Simple;
    private bool _showCodeBehind;

    private string SelectedCode => _showCodeBehind ? CodeBehindExample : RazorExample;

    protected override async Task OnInitializedAsync() => _products = await ProductProvider.GetProductsAsync();

    private void SetMode(VekDataGridFilterMode mode) => _filterMode = mode;
    private string ModeClass(VekDataGridFilterMode mode) => _filterMode == mode ? "is-active" : string.Empty;
    private void ShowRazor() => _showCodeBehind = false;
    private void ShowCodeBehind() => _showCodeBehind = true;
}
