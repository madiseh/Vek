using System.Collections.ObjectModel;
using Vek.Demo.Abstractions;

namespace Vek.Demo.Components.Pages.Docs.DataGrid.Observability;

public partial class Observability
{
    private const string RazorExample = """
        <button type="button" @onclick="AddProduct">Add sample product</button>
        <VekDataGrid TItem="DemoProduct" Items="_products" AriaLabel="Observable product inventory">
            <PropertyColumn Property="product => product.Name" Title="Name" />
            <PropertyColumn Property="product => product.Category" Title="Category" />
            <PropertyColumn Property="product => product.Price" Title="Price" Format="C2" />
        </VekDataGrid>
        """;

    private const string CodeBehindExample = """
        [Inject]
        private IDemoProductProvider ProductProvider { get; set; } = default!;

        private ObservableCollection<DemoProduct> _products = [];
        private int _nextId;

        protected override async Task OnInitializedAsync()
        {
            _products = new(await ProductProvider.GetProductsAsync());
            _nextId = _products.Count + 1;
        }

        private void AddProduct()
            => _products.Add(new DemoProduct(_nextId, $"Observable Product {_nextId++}", "Sample", 1.25m));
        """;

    [Inject]
    private IDemoProductProvider ProductProvider { get; set; } = default!;

    private ObservableCollection<DemoProduct>? _products;
    private int _nextId;
    private bool _showCodeBehind;
    private string SelectedCode => _showCodeBehind ? CodeBehindExample : RazorExample;

    protected override async Task OnInitializedAsync()
    {
        _products = new ObservableCollection<DemoProduct>(await ProductProvider.GetProductsAsync());
        _nextId = _products.Count + 1;
    }

    private void AddProduct()
    {
        _products!.Add(new DemoProduct(_nextId, $"Observable Product {_nextId}", "Sample", 1.25m));
        _nextId++;
    }

    private void ShowRazor() => _showCodeBehind = false;
    private void ShowCodeBehind() => _showCodeBehind = true;
}
