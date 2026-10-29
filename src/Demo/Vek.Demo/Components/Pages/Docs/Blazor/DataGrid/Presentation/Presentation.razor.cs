using Vek.Demo.Abstractions;

namespace Vek.Demo.Components.Pages.Docs.Blazor.DataGrid.Presentation;

public partial class Presentation
{
    private const string RazorExample = """
        <VekDataGrid TItem="DemoProduct" Items="_products" Culture="_culture"
                     AriaLabel="Product inventory" data-testid="presentation-grid">
            <ToolbarTemplate>
                <span>Product inventory</span>
                <span>@_products.Count products</span>
            </ToolbarTemplate>
            <HeaderTemplate>
                <tr><th colspan="3">DEMO INVENTORY</th></tr>
            </HeaderTemplate>
            <PropertyColumn Property="product => product.Name" Title="Name">
                <CellTemplate Context="product"><strong>@product.Name</strong></CellTemplate>
            </PropertyColumn>
            <PropertyColumn Property="product => product.Price" Title="Price" Format="C2" Editable="true">
                <EditTemplate Context="edit">
                    <output>@edit.Item.Price?.ToString("C2", edit.Culture)</output>
                </EditTemplate>
            </PropertyColumn>
        </VekDataGrid>
        """;

    private const string CodeBehindExample = """
        [Inject]
        private IDemoProductProvider ProductProvider { get; set; } = default!;

        private IReadOnlyList<DemoProduct> _products = [];
        private CultureInfo _culture = CultureInfo.GetCultureInfo("fr-FR");

        protected override async Task OnInitializedAsync()
            => _products = await ProductProvider.GetProductsAsync();
        """;

    [Inject]
    private IDemoProductProvider ProductProvider { get; set; } = default!;

    private IReadOnlyList<DemoProduct>? _products;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("fr-FR");
    private readonly IReadOnlyList<VekCodeViewTab> _codeSamples =
    [
        new("Sample.razor", VekCodeLanguage.Html, RazorExample),
        new("Sample.razor.cs", VekCodeLanguage.CSharp, CodeBehindExample)
    ];

    protected override async Task OnInitializedAsync() => _products = await ProductProvider.GetProductsAsync();

}
