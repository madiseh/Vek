using Vek.Demo.Abstractions;

namespace Vek.Demo.Components.Pages.Docs.Blazor.DataGrid.Pagination;

public partial class Pagination
{
    private const string RazorExample = """
        <VekDataGrid TItem="DemoProduct" Items="_products" @bind-CurrentPage="_currentPage"
                     @bind-PageSize="_pageSize" PageSizeOptions="_pageSizeOptions">
            <PropertyColumn Property="product => product.Name" Title="Name" />
            <PagerContent Context="pager">
                <button @onclick="() => pager.SetPageAsync(pager.State.Page - 1)">Previous</button>
                <span>Page @pager.State.Page of @pager.TotalPages (@pager.TotalItems items)</span>
                <button @onclick="() => pager.SetPageAsync(pager.State.Page + 1)">Next</button>
            </PagerContent>
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
            private int _currentPage = 1;
            private int _pageSize = 10;
            private readonly int[] _pageSizeOptions = [5, 10, 25];

            private void ShowPager(VekDataGridPagerContext<DemoProduct> pager) { }

            protected override async Task OnInitializedAsync()
                => _products = await ProductProvider.GetProductsAsync();
        }
        """;

    [Inject]
    private IDemoProductProvider ProductProvider { get; set; } = default!;

    private IReadOnlyList<DemoProduct>? _products;
    private readonly int[] _pageSizeOptions = [3, 5, 10];
    private int _pageSize = 3;
    private int _currentPage = 1;
    private bool _showCodeBehind;

    private string SelectedCode => _showCodeBehind ? CodeBehindExample : RazorExample;

    protected override async Task OnInitializedAsync() => _products = await ProductProvider.GetProductsAsync();
    private void ShowRazor() => _showCodeBehind = false;
    private void ShowCodeBehind() => _showCodeBehind = true;
}
