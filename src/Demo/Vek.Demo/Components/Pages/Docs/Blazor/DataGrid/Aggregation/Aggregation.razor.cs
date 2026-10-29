using Vek.Demo.Abstractions;
using Vek.Components.Components.DataGrid;

namespace Vek.Demo.Components.Pages.Docs.Blazor.DataGrid.Aggregation;

public partial class Aggregation
{
    private const string RazorExample = """
        <VekDataGrid TItem="DemoProduct" Items="_products" GroupBy="_groups" Aggregates="_aggregates">
            <PropertyColumn Property="product => product.Category" Title="Category" Key="Category">
                <GroupTemplate Context="group">
                    <strong>@group.Title: @group.Value</strong>
                </GroupTemplate>
            </PropertyColumn>
            <PropertyColumn Property="product => product.Price" Title="Price" Key="Price" />
            <FooterTemplate Context="footer">@footer.Items.Count products</FooterTemplate>
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
            private IReadOnlyList<VekDataGridGroupDefinition> _groups = [new("Category")];
            private IReadOnlyList<VekDataGridAggregateDefinition<DemoProduct>> _aggregates =
            [
                new("Sum", "Price", VekDataGridAggregateType.Sum, Format: "C2"),
                new("Average", "Price", VekDataGridAggregateType.Average, Format: "C2"),
                new("Count", "Price", VekDataGridAggregateType.Count)
            ];
        }
        """;

    [Inject]
    private IDemoProductProvider ProductProvider { get; set; } = default!;

    private IReadOnlyList<DemoProduct>? _products;
    private IReadOnlyList<VekDataGridGroupDefinition> _groups = [new("Category")];
    private IReadOnlyList<VekDataGridAggregateDefinition<DemoProduct>> _aggregates =
    [
        new("Sum", "Price", VekDataGridAggregateType.Sum, Format: "C2"),
        new("Average", "Price", VekDataGridAggregateType.Average, Format: "C2"),
        new("Count", "Price", VekDataGridAggregateType.Count)
    ];
    private bool _showCodeBehind;

    private string SelectedCode => _showCodeBehind ? CodeBehindExample : RazorExample;
    protected override async Task OnInitializedAsync() => _products = await ProductProvider.GetProductsAsync();
    private void ShowRazor() => _showCodeBehind = false;
    private void ShowCodeBehind() => _showCodeBehind = true;
}
