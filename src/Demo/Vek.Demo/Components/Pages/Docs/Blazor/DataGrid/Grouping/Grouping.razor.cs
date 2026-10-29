using Vek.Demo.Abstractions;
using Vek.Components.Components.DataGrid;

namespace Vek.Demo.Components.Pages.Docs.Blazor.DataGrid.Grouping;

public partial class Grouping
{
    private const string RazorExample = """
        <VekDataGrid TItem="DemoProduct" Items="_products" GroupBy="_groups">
            <PropertyColumn Property="product => product.Category" Title="Category" Key="Category">
                <GroupTemplate Context="group">
                    <strong>@group.Title: @group.Value</strong>
                    <span>@group.Count products</span>
                </GroupTemplate>
            </PropertyColumn>
            <PropertyColumn Property="product => product.Name" Title="Name" Key="Name" />
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
            private IReadOnlyList<VekDataGridGroupDefinition> _groups =
            [
                new("Category"),
                new("Name", VekDataGridGroupSortOrder.Descending)
            ];

            private void OnGroupChanged(IReadOnlyList<VekDataGridGroupDefinition> groups) { }
        }
        """;

    [Inject]
    private IDemoProductProvider ProductProvider { get; set; } = default!;

    private IReadOnlyList<DemoProduct>? _products;
    private IReadOnlyList<VekDataGridGroupDefinition> _groups =
    [
        new("Category"),
        new("Name")
    ];
    private bool _showCodeBehind;

    private string SelectedCode => _showCodeBehind ? CodeBehindExample : RazorExample;

    protected override async Task OnInitializedAsync() => _products = await ProductProvider.GetProductsAsync();
    private void OnGroupChanged(IReadOnlyList<VekDataGridGroupDefinition> groups) => _groups = groups;
    private void ShowRazor() => _showCodeBehind = false;
    private void ShowCodeBehind() => _showCodeBehind = true;
}
