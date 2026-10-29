using Vek.Demo.Abstractions;

namespace Vek.Demo.Components.Pages.Docs.Blazor.DataGrid.ColumnManagement;

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
    private readonly IReadOnlyList<VekCodeViewTab> _codeSamples =
    [
        new("Sample.razor", VekCodeLanguage.Html, RazorExample),
        new("Sample.razor.cs", VekCodeLanguage.CSharp, CodeBehindExample)
    ];

    protected override async Task OnInitializedAsync() => _products = await ProductProvider.GetProductsAsync();
}
