using Vek.Demo.Abstractions;

namespace Vek.Demo.Components.Pages.Docs.Blazor.DataGrid.ContextActions;

public partial class ContextActions
{
    private const string RazorExample = """
        <VekDataGrid TItem="DemoProduct" Items="_products"
                     RowActionsTemplate="RenderRowActions" CellActionsTemplate="RenderCellActions">
            <PropertyColumn Property="product => product.Name" Title="Name" Key="Name" />
            <PropertyColumn Property="product => product.Price" Title="Price" Key="Price" Editable="true" />
        </VekDataGrid>
        """;

    private const string CodeBehindExample = """
        private RenderFragment<VekDataGridRowContext<DemoProduct>> RenderRowActions => row => builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, () => ShowProduct(row.Item)));
            builder.AddContent(2, $"Open {row.Item.Name} · row {row.Index + 1}");
            builder.CloseElement();
        };

        private void ShowProduct(DemoProduct product)
            => _feedback = $"Selected product: {product.Name}";
        """;

    [Inject]
    private IDemoProductProvider ProductProvider { get; set; } = default!;

    private IReadOnlyList<DemoProduct>? _products;
    private string _feedback = "Choose a row action to inspect a product.";
    private bool _showCodeBehind;
    private string SelectedCode => _showCodeBehind ? CodeBehindExample : RazorExample;

    protected override async Task OnInitializedAsync() => _products = await ProductProvider.GetProductsAsync();

    private RenderFragment<VekDataGridRowContext<DemoProduct>> RenderRowActions => context => builder =>
    {
        builder.OpenElement(0, "button");
        builder.AddAttribute(1, "type", "button");
        builder.AddAttribute(2, "aria-label", $"Show actions for {context.Item.Name}");
        builder.AddAttribute(3, "onclick", EventCallback.Factory.Create(this, () => ShowProduct(context.Item)));
        builder.AddContent(4, "Details");
        builder.CloseElement();
    };

    private RenderFragment<VekDataGridCellContext<DemoProduct>> RenderCellActions => context => builder =>
    {
        if (context.ColumnKey != "Price") return;
        builder.OpenElement(0, "button");
        builder.AddAttribute(1, "type", "button");
        builder.AddAttribute(2, "aria-label", $"Inspect price for {context.Item.Name}");
        builder.AddAttribute(3, "onclick", EventCallback.Factory.Create(this,
            () => _feedback = $"Price column for row {context.RowIndex + 1}: {context.Item.Price:C2}"));
        builder.AddContent(4, "Inspect");
        builder.CloseElement();
    };

    private void ShowProduct(DemoProduct product) => _feedback = $"Selected product: {product.Name}";
    private void ShowRazor() => _showCodeBehind = false;
    private void ShowCodeBehind() => _showCodeBehind = true;
}
