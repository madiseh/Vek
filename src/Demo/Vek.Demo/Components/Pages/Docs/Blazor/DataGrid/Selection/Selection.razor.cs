using Vek.Demo.Abstractions;
using Vek.Components.Components.DataGrid;

namespace Vek.Demo.Components.Pages.Docs.Blazor.DataGrid.Selection;

public partial class Selection
{
    private const string RazorExample = """
        <VekDataGrid TItem="DemoProduct" Items="_products"
                     SelectionMode="VekDataGridSelectionMode.Multiple"
                     @bind-SelectedItems="_selectedProducts"
                     IsRowSelectable="CanSelectProduct">
            <SelectionColumn TItem="DemoProduct" />
            <PropertyColumn Property="product => product.Name" Title="Name" />
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
            private IReadOnlyList<DemoProduct> _selectedProducts = [];

            private bool CanSelectProduct(DemoProduct product)
                => product.Category != "Discontinued";
        }
        """;

    [Inject]
    private IDemoProductProvider ProductProvider { get; set; } = default!;

    private IReadOnlyList<DemoProduct>? _products;
    private IReadOnlyList<DemoProduct> _selectedProducts = [];
    private bool _showCodeBehind;

    private string SelectedCode => _showCodeBehind ? CodeBehindExample : RazorExample;

    protected override async Task OnInitializedAsync() => _products = await ProductProvider.GetProductsAsync();
    private bool CanSelectProduct(DemoProduct product) => product.Category != "Discontinued";
    private void ShowRazor() => _showCodeBehind = false;
    private void ShowCodeBehind() => _showCodeBehind = true;
}
