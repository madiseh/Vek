using Vek.Demo.Abstractions;

namespace Vek.Demo.Components.Pages.Docs.DataGrid.ColumnLayout;

public partial class ColumnLayout
{
    private const string RazorExample = """
        <VekDataGrid TItem="DemoProduct" Items="_products" ColumnGroups="_columnGroups"
                     ColumnResizeMode="VekDataGridColumnResizeMode.Column" Density="VekDataGridDensity.Compact"
                     Bordered="true" Striped="true" FixedHeader="true" Responsive="true"
                     RowClass="GetRowClass" CellClass="GetCellClass">
            <PropertyColumn Property="product => product.Name" Title="Name" Key="Name" Width="210" />
            <PropertyColumn Property="product => product.Category" Title="Category" Key="Category" Width="160" />
            <PropertyColumn Property="product => product.Price" Title="Price" Key="Price" Format="C2" Width="140" />
        </VekDataGrid>
        """;

    private const string CodeBehindExample = """
        [Inject]
        private IDemoProductProvider ProductProvider { get; set; } = default!;

        private IReadOnlyList<DemoProduct> _products = [];
        private IReadOnlyList<VekDataGridColumnGroup> _columnGroups =
        [
            new("Product", ["Name", "Category"]),
            new("Pricing", ["Price"])
        ];

        protected override async Task OnInitializedAsync()
            => _products = await ProductProvider.GetProductsAsync();
        """;

    [Inject]
    private IDemoProductProvider ProductProvider { get; set; } = default!;

    private IReadOnlyList<DemoProduct>? _products;
    private readonly IReadOnlyList<VekDataGridColumnGroup> _columnGroups =
    [
        new("Product", ["Name", "Category"]),
        new("Pricing", ["Price"])
    ];
    private bool _showCodeBehind;
    private string SelectedCode => _showCodeBehind ? CodeBehindExample : RazorExample;

    protected override async Task OnInitializedAsync() => _products = await ProductProvider.GetProductsAsync();

    private string? GetRowClass(DemoProduct product, int index)
        => product.Price >= 150 ? "high-value-row" : null;

    private string? GetCellClass(DemoProduct product, string columnKey, int index)
        => columnKey == "Price" ? "price-cell" : null;

    private void ShowRazor() => _showCodeBehind = false;
    private void ShowCodeBehind() => _showCodeBehind = true;
}
