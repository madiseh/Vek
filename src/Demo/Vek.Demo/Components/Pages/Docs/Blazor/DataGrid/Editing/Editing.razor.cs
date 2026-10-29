using Vek.Demo.Abstractions;
using Vek.Components.Components.DataGrid;

namespace Vek.Demo.Components.Pages.Docs.Blazor.DataGrid.Editing;

public partial class Editing
{
    private const string RazorExample = """
        <VekDataGrid TItem="DemoProduct" Items="_products" EditMode="VekDataGridEditMode.Row"
                     CloneItem="CloneProduct" CommitEdit="ValidateProduct" ItemCommitted="SaveProduct">
            <PropertyColumn Property="product => product.Name" Title="Name" Editable="true">
                <EditTemplate Context="edit">
                    <input value="@edit.Item.Name"
                           @onchange="args => edit.UpdateItem(edit.Item with { Name = args.Value?.ToString() ?? string.Empty })" />
                    <button @onclick="edit.CommitAsync">Save</button>
                    <button @onclick="edit.CancelAsync">Cancel</button>
                </EditTemplate>
            </PropertyColumn>
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
            private DemoProduct CloneProduct(DemoProduct product) => product with { };
            private Task<bool> ValidateProduct(DemoProduct product)
                => Task.FromResult(!string.IsNullOrWhiteSpace(product.Name));
            private void SaveProduct(DemoProduct product)
                => _products = _products.Select(item => item.Id == product.Id ? product : item).ToArray();
        }
        """;

    [Inject]
    private IDemoProductProvider ProductProvider { get; set; } = default!;

    private IReadOnlyList<DemoProduct>? _products;
    private string? _editMessage;
    private bool _showCodeBehind;

    private string SelectedCode => _showCodeBehind ? CodeBehindExample : RazorExample;

    protected override async Task OnInitializedAsync() => _products = await ProductProvider.GetProductsAsync();
    private DemoProduct CloneProduct(DemoProduct product) => product with { };
    private Task<bool> ValidateProduct(DemoProduct product)
    {
        _editMessage = string.IsNullOrWhiteSpace(product.Name) ? "A product name is required." : null;
        return Task.FromResult(_editMessage is null);
    }

    private void SaveProduct(DemoProduct product)
    {
        _products = _products?.Select(item => item.Id == product.Id ? product : item).ToArray();
        _editMessage = "Product saved.";
    }

    private void ShowRazor() => _showCodeBehind = false;
    private void ShowCodeBehind() => _showCodeBehind = true;
}
