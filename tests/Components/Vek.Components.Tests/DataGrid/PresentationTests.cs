using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class PresentationTests : BunitContext
{
    /// <summary>Verifies grid toolbar actions and a custom header render in their semantic regions.</summary>
    [Fact]
    public void ToolbarAndHeaderTemplatesRender()
    {
        var cut = RenderGrid(parameters => parameters
            .Add(grid => grid.ToolbarTemplate, Text("Export products"))
            .Add(grid => grid.HeaderTemplate, (RenderFragment)(builder =>
            {
                builder.OpenElement(0, "tr");
                builder.AddAttribute(1, "class", "custom-header");
                builder.OpenElement(2, "th");
                builder.AddAttribute(3, "colspan", "1");
                builder.AddContent(4, "Inventory");
                builder.CloseElement();
                builder.CloseElement();
            }))); 

        Assert.Contains("Export products", cut.Find(".grid-toolbar").TextContent);
        Assert.Equal("Inventory", cut.Find("thead tr.custom-header th").TextContent.Trim());
    }

    /// <summary>Verifies row templates can render custom row content instead of default cells.</summary>
    [Fact]
    public void RowTemplateOverridesDefaultCells()
    {
        var cut = RenderGrid(parameters => parameters.Add(grid => grid.RowTemplate,
            (RenderFragment<TestProduct>)(product => builder =>
            {
                builder.OpenElement(0, "td");
                builder.AddAttribute(1, "colspan", "1");
                builder.AddContent(2, $"Custom row: {product.Name}");
                builder.CloseElement();
            })));

        Assert.Contains("Custom row: Task Chair", cut.Find("tbody").TextContent);
        Assert.DoesNotContain("Task Chair", cut.Find("thead").TextContent);
        Assert.Single(cut.FindAll("tbody tr:first-child td"));
    }

    /// <summary>Verifies a custom footer is rendered even when the grid has no aggregate calculations.</summary>
    [Fact]
    public void FooterTemplateRendersWithoutAggregateDefinitions()
    {
        var cut = RenderGrid(parameters => parameters.Add(grid => grid.FooterTemplate,
            (RenderFragment<VekDataGridFooterContext<TestProduct>>)(context => builder =>
                builder.AddContent(0, $"Footer rows {context.Items.Count}"))));

        Assert.Contains("Footer rows 3", cut.Find("tfoot").TextContent);
    }

    /// <summary>Verifies custom loading and empty content appear for the corresponding data states.</summary>
    [Fact]
    public void LoadingAndEmptyTemplatesRenderForTheirStates()
    {
        var pending = new TaskCompletionSource<VekDataGridData<TestProduct>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var loading = Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.ServerData, (_, _) => pending.Task)
            .Add(grid => grid.LoadingContent, Text("Loading inventory"))
            .Add(grid => grid.EmptyContent, Text("No products found"))
            .AddChildContent(NameColumn()));

        loading.WaitForAssertion(() => Assert.Contains("Loading inventory", loading.Find(".loading-state").TextContent));
        pending.SetResult(new VekDataGridData<TestProduct>([], 0));
        loading.WaitForAssertion(() => Assert.Contains("No products found", loading.Find(".empty-state").TextContent));

        var empty = Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, Array.Empty<TestProduct>())
            .Add(grid => grid.EmptyContent, Text("No products found"))
            .AddChildContent(NameColumn()));
        Assert.Contains("No products found", empty.Find(".empty-state").TextContent);
    }

    /// <summary>Verifies grid culture is available to formatters and custom edit templates.</summary>
    [Fact]
    public void CultureFormatsValuesAndEditors()
    {
        var french = CultureInfo.GetCultureInfo("fr-FR");
        var cut = Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, [new TestProduct("Desk Lamp", 49.99m)])
            .Add(grid => grid.Culture, french)
            .Add(grid => grid.EditMode, VekDataGridEditMode.Cell)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<PropertyColumn<TestProduct, decimal?>>(0);
                builder.AddAttribute(1, nameof(PropertyColumn<TestProduct, decimal?>.Property),
                    (System.Linq.Expressions.Expression<Func<TestProduct, decimal?>>)(product => product.Price));
                builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, decimal?>.Title), "Price");
                builder.AddAttribute(3, nameof(PropertyColumn<TestProduct, decimal?>.Format), "N2");
                builder.AddAttribute(4, nameof(PropertyColumn<TestProduct, decimal?>.Editable), true);
                builder.AddAttribute(5, nameof(PropertyColumn<TestProduct, decimal?>.EditTemplate),
                    (RenderFragment<VekDataGridEditContext<TestProduct>>)(context => fragment =>
                        fragment.AddContent(0, context.Culture.Name)));
                builder.CloseComponent();
            }));

        Assert.Contains("49,99", cut.Find("tbody").TextContent);
        cut.Find("button[aria-label='Edit Price']").Click();
        Assert.Contains("fr-FR", cut.Find("tbody").TextContent);
    }

    /// <summary>Verifies the table exposes its accessible name and current sort state.</summary>
    [Fact]
    public void TableHasAccessibleNameAndSortState()
    {
        var cut = RenderGrid(parameters =>
        {
            parameters.Add(grid => grid.AriaLabel, "Product inventory");
            parameters.AddUnmatched("data-testid", "product-table");
        });

        Assert.Equal("Product inventory", cut.Find("table").GetAttribute("aria-label"));
        Assert.Equal("product-table", cut.Find("table").GetAttribute("data-testid"));
        cut.Find("button[aria-label='Sort by Name']").Click();
        Assert.Equal("ascending", cut.Find("th[data-column-key='Name']").GetAttribute("aria-sort"));
    }

    /// <summary>Verifies native grid actions remain in the tab order and reorderable headings are keyboard reachable.</summary>
    [Fact]
    public void AllGridActionsAreKeyboardReachable()
    {
        var cut = RenderGrid(parameters => parameters
            .Add(grid => grid.ColumnReorderMode, VekDataGridColumnReorderMode.Header)
            .Add(grid => grid.ColumnResizeMode, VekDataGridColumnResizeMode.Column)
            .Add(grid => grid.FilterMode, VekDataGridFilterMode.ColumnRow)
            .Add(grid => grid.SelectionMode, VekDataGridSelectionMode.Multiple)
            .AddChildContent(ColumnsWithSelection()));

        Assert.Equal("0", cut.Find("th[data-column-key='Name']").GetAttribute("tabindex"));
        Assert.Equal("Sort by Name", cut.Find("button[aria-label='Sort by Name']").GetAttribute("aria-label"));
        Assert.Equal("Resize Name column", cut.Find("input[aria-label='Resize Name column']").GetAttribute("aria-label"));
        Assert.Equal("Select all visible rows", cut.Find("input[aria-label='Select all visible rows']").GetAttribute("aria-label"));
        Assert.All(cut.FindAll("button, input, select"), element =>
            Assert.NotEqual("-1", element.GetAttribute("tabindex")));
    }

    private IRenderedComponent<VekDataGrid<TestProduct>> RenderGrid(
        Action<ComponentParameterCollectionBuilder<VekDataGrid<TestProduct>>> configure) =>
        Render<VekDataGrid<TestProduct>>(parameters =>
        {
            parameters.Add(grid => grid.Items, DataGridTestHelpers.Products);
            parameters.AddChildContent(NameColumn());
            configure(parameters);
        });

    private static RenderFragment NameColumn() => builder =>
    {
        builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
        builder.AddAttribute(1, nameof(PropertyColumn<TestProduct, string>.Property),
            (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Name));
        builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, string>.Title), "Name");
        builder.AddAttribute(3, nameof(PropertyColumn<TestProduct, string>.Key), "Name");
        builder.CloseComponent();
    };

    private static RenderFragment ColumnsWithSelection() => builder =>
    {
        builder.OpenComponent<SelectionColumn<TestProduct>>(0);
        builder.CloseComponent();
        builder.AddContent(1, NameColumn());
    };

    private static RenderFragment Text(string value) => builder => builder.AddContent(0, value);
}
