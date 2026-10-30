using Bunit;
using Microsoft.AspNetCore.Components;
using Vek.Blazor.DataGrid.Components.DataGrid;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class ColumnLayoutTests : BunitContext
{
    /// <summary>Verifies a column group spans exactly its configured visible columns.</summary>
    [Fact]
    public void ColumnGroupSpansConfiguredColumns()
    {
        var cut = RenderGrid(parameters => parameters.Add(grid => grid.ColumnGroups,
        [
            new VekDataGridColumnGroup("Product details", ["Name", "Category"])
        ]));

        var group = cut.Find("thead tr.column-group-row th");

        Assert.Equal("2", group.GetAttribute("colspan"));
        Assert.Equal("Product details", group.TextContent.Trim());
    }

    /// <summary>Verifies resizing a column updates its rendered width.</summary>
    [Fact]
    public void ColumnResizeUpdatesConfiguredWidth()
    {
        var cut = RenderGrid(parameters => parameters.Add(grid => grid.ColumnResizeMode, VekDataGridColumnResizeMode.Column));

        cut.Find("input[aria-label='Resize Name column']").Input("220");

        Assert.Contains("width: 220px", cut.Find("thead th[data-column-key='Name']").GetAttribute("style"));
        Assert.Contains("width: 220px", cut.Find("tbody tr td").GetAttribute("style"));
    }

    /// <summary>Verifies row style selectors receive the displayed item and its zero-based result index.</summary>
    [Fact]
    public void RowStyleFunctionReceivesItemAndIndex()
    {
        var received = new List<(TestProduct Item, int Index)>();
        var cut = RenderGrid(parameters => parameters.Add(grid => grid.RowStyle, (item, index) =>
        {
            received.Add((item, index));
            return index == 0 ? "background-color: #eef2ff" : null;
        }));

        foreach (var (item, index) in DataGridTestHelpers.Products.Select((item, index) => (item, index)))
            Assert.Contains((item, index), received);
        Assert.Contains("background-color: #eef2ff", cut.Find("tbody tr").GetAttribute("style"));
    }

    /// <summary>Verifies fixed-header mode marks the table header for sticky positioning.</summary>
    [Fact]
    public void FixedHeaderKeepsHeaderVisible()
    {
        var cut = RenderGrid(parameters => parameters.Add(grid => grid.FixedHeader, true));

        Assert.Contains("is-sticky", cut.Find("thead").ClassName);
    }

    /// <summary>Verifies responsive mode constrains the grid and contains wide content in its horizontal scroller.</summary>
    [Fact]
    public void NarrowGridAvoidsPageOverflow()
    {
        var cut = RenderGrid(parameters => parameters.Add(grid => grid.Responsive, true));

        Assert.Contains("is-responsive", cut.Find(".vek-data-grid").ClassName);
        Assert.Equal("true", cut.Find(".table-scroll-container").GetAttribute("data-responsive"));
    }

    private IRenderedComponent<VekDataGrid<TestProduct>> RenderGrid(
        Action<ComponentParameterCollectionBuilder<VekDataGrid<TestProduct>>>? configure = null) =>
        Render<VekDataGrid<TestProduct>>(parameters =>
        {
            parameters.Add(grid => grid.Items, DataGridTestHelpers.Products);
            parameters.AddChildContent(Columns());
            configure?.Invoke(parameters);
        });

    private static RenderFragment Columns() => builder =>
    {
        builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
        builder.AddAttribute(1, nameof(PropertyColumn<TestProduct, string>.Property),
            (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Name));
        builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, string>.Title), "Name");
        builder.AddAttribute(3, nameof(PropertyColumn<TestProduct, string>.Key), "Name");
        builder.CloseComponent();
        builder.OpenComponent<PropertyColumn<TestProduct, string>>(4);
        builder.AddAttribute(5, nameof(PropertyColumn<TestProduct, string>.Property),
            (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Category));
        builder.AddAttribute(6, nameof(PropertyColumn<TestProduct, string>.Title), "Category");
        builder.AddAttribute(7, nameof(PropertyColumn<TestProduct, string>.Key), "Category");
        builder.CloseComponent();
    };
}
