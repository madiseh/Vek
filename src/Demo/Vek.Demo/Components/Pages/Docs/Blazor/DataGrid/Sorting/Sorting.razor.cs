using Vek.Demo.Abstractions;

namespace Vek.Demo.Components.Pages.Docs.Blazor.DataGrid.Sorting;

public partial class Sorting
{
    private const string RazorExample = """
        <VekDataGrid TItem="DemoProduct" Items="_products" PageSize="10" SortMode="VekDataGridSortMode.Multiple">
            <PropertyColumn Property="product => product.Name" Title="Name" />
            <PropertyColumn Property="product => product.Category" Title="Category" />
            <PropertyColumn Property="product => product.Price" Title="Price" Format="C2" />
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
    private readonly IReadOnlyList<VekCodeViewTab> _codeSamples =
    [
        new("Sample.razor", VekCodeLanguage.Html, RazorExample),
        new("Sample.razor.cs", VekCodeLanguage.CSharp, CodeBehindExample)
    ];


    protected override async Task OnInitializedAsync()
    {
        _products = await ProductProvider.GetProductsAsync();
    }


}
