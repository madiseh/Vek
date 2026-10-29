using Vek.Demo.Abstractions;

namespace Vek.Demo.Components.Pages.Docs.Blazor.DataGrid.Hierarchy;

public partial class Hierarchy
{
    private const string RazorExample = """
        <VekDataGrid TItem="DemoProduct" Items="_products" SingleExpandedRow="true">
            <HierarchyColumn TItem="DemoProduct">
                <DetailTemplate Context="detail">
                    <strong>@detail.Item.Name</strong>
                    <span>@detail.Item.Category · @detail.Item.Price.ToString("C2")</span>
                </DetailTemplate>
            </HierarchyColumn>
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
            private bool InitiallyExpanded(DemoProduct product)
                => product.Category == "Furniture";
        }
        """;

    [Inject]
    private IDemoProductProvider ProductProvider { get; set; } = default!;

    private IReadOnlyList<DemoProduct>? _products;
    private bool _singleExpanded = true;
    private bool _showCodeBehind;

    private string SelectedCode => _showCodeBehind ? CodeBehindExample : RazorExample;
    private static bool InitiallyExpanded(DemoProduct product) => product.Category == "Furniture";

    protected override async Task OnInitializedAsync() => _products = await ProductProvider.GetProductsAsync();
    private void ShowRazor() => _showCodeBehind = false;
    private void ShowCodeBehind() => _showCodeBehind = true;
}
