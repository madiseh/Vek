using Vek.Demo.Abstractions;

namespace Vek.Demo.Components.Pages.Docs.DataGrid.BasicUsage;

public partial class BasicUsage
{
    private const string RazorExample = """
        <VekDataGrid TItem="DemoProduct" Items="_products" PageSize="10">
            <PropertyColumn Property="product => product.Name" Title="Name" />
            <PropertyColumn Property="product => product.Category" Title="Category" />
            <PropertyColumn Property="product => product.Price" Title="Price" Format="C2" />
            <TemplateColumn Key="product-id" Title="Product ID">
                <CellTemplate Context="product">@product.Id</CellTemplate>
            </TemplateColumn>
        </VekDataGrid>
        """;

    private const string CodeBehindExample = """
        using Microsoft.AspNetCore.Components;
        using Vek.Demo.Abstractions;

        namespace MyApp.Components.Pages;

        public partial class Sample
        {
            [Inject]
            private IDemoProductProvider ProductProvider { get; set; } = default!;

            private IReadOnlyList<DemoProduct> _products = [];

            protected override async Task OnInitializedAsync()
            {
                _products = await ProductProvider.GetProductsAsync();
            }
        }
        """;

    [Inject]
    private IDemoProductProvider ProductProvider { get; set; } = default!;

    private IReadOnlyList<DemoProduct>? _products;
    private bool _showCodeBehind;

    private string SelectedCode => _showCodeBehind ? CodeBehindExample : RazorExample;

    protected override async Task OnInitializedAsync()
    {
        _products = await ProductProvider.GetProductsAsync();
    }

    private void ShowRazor() => _showCodeBehind = false;

    private void ShowCodeBehind() => _showCodeBehind = true;
}
