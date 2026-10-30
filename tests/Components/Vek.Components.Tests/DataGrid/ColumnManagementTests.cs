using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.Blazor.DataGrid.Components.DataGrid;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class ColumnManagementTests : BunitContext
{
    /// <summary>Verifies the column panel toggles visibility and reports changes through the grid API.</summary>
    [Fact]
    public void ColumnVisibilityCanBeToggled()
    {
        var visibilityChanges = new List<bool>();
        var cut = RenderGrid(
            columns: Columns(categoryVisible: true, categoryVisibleChanged: EventCallback.Factory.Create<bool>(this, visibilityChanges.Add)),
            columnPanel: true);
        cut.Find("button[aria-label='Manage columns']").Click();

        cut.Find("input[aria-label='Show Category column']").Change(false);

        Assert.DoesNotContain(cut.FindAll("thead th"), header => header.TextContent.Contains("Category", StringComparison.Ordinal));
        Assert.DoesNotContain("Furniture", cut.Find("tbody").TextContent);
        Assert.Contains("Category", cut.Find(".column-panel").TextContent);
        Assert.Equal(new[] { false }, visibilityChanges);
    }

    /// <summary>Verifies the column panel search filters its available visibility and reorder controls.</summary>
    [Fact]
    public void ColumnPanelFiltersColumnsBySearch()
    {
        var cut = RenderGrid(columns: Columns(), columnPanel: true);
        cut.Find("button[aria-label='Manage columns']").Click();

        cut.Find("input[aria-label='Search columns']").Input("Price");

        Assert.Equal("Price", cut.FindAll(".column-panel .column-title").Single().TextContent.Trim());
    }

    /// <summary>Verifies panel keyboard controls move a column to the requested insert position.</summary>
    [Fact]
    public void PanelReorderingUsesInsertPosition()
    {
        var cut = RenderGrid(columns: Columns(), columnPanel: true, reorderMode: VekDataGridColumnReorderMode.Panel);
        cut.Find("button[aria-label='Manage columns']").Click();

        cut.Find("button[aria-label='Move Category column down']").Click();

        Assert.Equal(new[] { "Name", "Price", "Category" }, HeaderNames(cut));
    }

    /// <summary>Verifies header dragging reorders columns only when header reordering is enabled.</summary>
    [Fact]
    public void HeaderReorderingUsesConfiguredMode()
    {
        var disabled = RenderGrid(columns: Columns(), reorderMode: VekDataGridColumnReorderMode.Panel);
        Assert.All(disabled.FindAll("thead th"), header => Assert.Equal("false", header.GetAttribute("draggable")));

        var enabled = RenderGrid(columns: Columns(), reorderMode: VekDataGridColumnReorderMode.Header);
        var headers = enabled.FindAll("thead th");
        Assert.All(headers, header => Assert.Equal("true", header.GetAttribute("draggable")));
        headers[0].TriggerEvent("ondragstart", new DragEventArgs());
        enabled.FindAll("thead th")[1].TriggerEvent("ondrop", new DragEventArgs());

        Assert.Equal(new[] { "Category", "Name", "Price" }, HeaderNames(enabled));
        enabled.FindAll("thead th")[2].KeyDown("ArrowLeft");
        Assert.Equal(new[] { "Category", "Price", "Name" }, HeaderNames(enabled));
    }

    /// <summary>Verifies a column declared invisible is omitted from both the table header and row cells.</summary>
    [Fact]
    public void HiddenColumnsAreExcludedFromRendering()
    {
        var cut = RenderGrid(columns: Columns(categoryVisible: false));

        Assert.Equal(new[] { "Name", "Price" }, HeaderNames(cut));
        Assert.DoesNotContain("Furniture", cut.Find("tbody").TextContent);
    }

    private IRenderedComponent<VekDataGrid<TestProduct>> RenderGrid(
        RenderFragment columns,
        bool columnPanel = false,
        VekDataGridColumnReorderMode reorderMode = VekDataGridColumnReorderMode.None) => Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, DataGridTestHelpers.Products)
            .Add(grid => grid.ShowColumnPanel, columnPanel)
            .Add(grid => grid.ColumnReorderMode, reorderMode)
            .AddChildContent(columns));

    private static string[] HeaderNames(IRenderedComponent<VekDataGrid<TestProduct>> cut) => cut
        .FindAll("thead th[data-column-key]")
        .Select(header => header.TextContent.Trim())
        .ToArray();

    private static RenderFragment Columns(bool categoryVisible = true, EventCallback<bool> categoryVisibleChanged = default) => builder =>
    {
        builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
        builder.AddAttribute(1, nameof(PropertyColumn<TestProduct, string>.Property),
            (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Name));
        builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, string>.Title), "Name");
        builder.CloseComponent();
        builder.OpenComponent<PropertyColumn<TestProduct, string>>(3);
        builder.AddAttribute(4, nameof(PropertyColumn<TestProduct, string>.Property),
            (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Category));
        builder.AddAttribute(5, nameof(PropertyColumn<TestProduct, string>.Title), "Category");
        builder.AddAttribute(6, nameof(PropertyColumn<TestProduct, string>.Visible), categoryVisible);
        if (categoryVisibleChanged.HasDelegate)
            builder.AddAttribute(7, nameof(PropertyColumn<TestProduct, string>.VisibleChanged), categoryVisibleChanged);
        builder.CloseComponent();
        builder.OpenComponent<PropertyColumn<TestProduct, decimal?>>(8);
        builder.AddAttribute(9, nameof(PropertyColumn<TestProduct, decimal?>.Property),
            (System.Linq.Expressions.Expression<Func<TestProduct, decimal?>>)(product => product.Price));
        builder.AddAttribute(10, nameof(PropertyColumn<TestProduct, decimal?>.Title), "Price");
        builder.CloseComponent();
    };
}
