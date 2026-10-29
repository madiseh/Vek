using Vek.Demo.Abstractions;

namespace Vek.Demo.Components.Pages.Docs.DataGrid.ColumnManagement;

public partial class ColumnManagement
{
    private const string RazorExample = """
        <VekDataGrid TItem="DemoProduct" Items="_products" ShowColumnPanel="true"
                     ColumnReorderMode="VekDataGridColumnReorderMode.Both">
            <PropertyColumn Property="product => product.Name" Title="Name" />
            <PropertyColumn Property="product => product.Category" Title="Category" />
            <PropertyColumn Property="product => product.Price" Title="Price" @bind-Visible="_showPrice" />
        </VekDataGrid>
        """;

    private const string CodeBehindExample = """
        [Inject]
        private IDemoProductProvider ProductProvider { get; set; } = default!;

        private IReadOnlyList<DemoProduct>? _products;
        private bool _showPrice = true;

        protected override async Task OnInitializedAsync()
            => _products = await ProductProvider.GetProductsAsync();
        """;

    [Inject]
    private IDemoProductProvider ProductProvider { get; set; } = default!;

    private IReadOnlyList<DemoProduct>? _products;
    private bool _showPrice = true;
    private bool _showCodeBehind;
    private string SelectedCode => _showCodeBehind ? CodeBehindExample : RazorExample;

    protected override async Task OnInitializedAsync() => _products = await ProductProvider.GetProductsAsync();
    private void ShowRazor() => _showCodeBehind = false;
    private void ShowCodeBehind() => _showCodeBehind = true;
}
