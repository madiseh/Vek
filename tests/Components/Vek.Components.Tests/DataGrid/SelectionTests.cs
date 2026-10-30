using Bunit;
using Microsoft.AspNetCore.Components;
using Vek.Blazor.DataGrid.Components.DataGrid;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class SelectionTests : BunitContext
{
    /// <summary>Verifies selecting a row in single mode updates the selected item.</summary>
    [Fact]
    public void RowClickSelectsSingleItem()
    {
        var cut = RenderSelectionGrid(selectionMode: VekDataGridSelectionMode.Single, rowClickSelection: true);

        cut.Find("tbody tr:nth-child(2)").Click();

        Assert.Equal("Desk Lamp", cut.Instance.SelectedItem?.Name);
        Assert.Contains("is-selected", cut.Find("tbody tr:nth-child(2)").ClassList);
    }

    /// <summary>Verifies the selection column checkboxes toggle multiple items.</summary>
    [Fact]
    public void CheckboxSelectsMultipleItems()
    {
        var cut = RenderSelectionGrid(selectionMode: VekDataGridSelectionMode.Multiple);

        cut.Find("tbody tr:nth-child(1) input[type=checkbox]").Change(true);
        cut.Find("tbody tr:nth-child(3) input[type=checkbox]").Change(true);

        Assert.Equal(new[] { "Task Chair", "Notebook Set" }, cut.Instance.SelectedItems.Select(item => item.Name));
    }

    /// <summary>Verifies the selection-column header selects all selectable rows on the current page.</summary>
    [Fact]
    public void SelectAllCheckboxAffectsVisibleRows()
    {
        var cut = RenderSelectionGrid(selectionMode: VekDataGridSelectionMode.Multiple);

        cut.Find("thead input[type=checkbox]").Change(true);

        Assert.Equal(3, cut.Instance.SelectedItems.Count);
    }

    /// <summary>Verifies disabled rows cannot be selected with row clicks or checkboxes.</summary>
    [Fact]
    public void DisabledRowsCannotBeSelected()
    {
        var cut = RenderSelectionGrid(
            selectionMode: VekDataGridSelectionMode.Multiple,
            rowClickSelection: true,
            isRowSelectable: product => product.Name != "Desk Lamp");

        var row = cut.Find("tbody tr:nth-child(2)");
        Assert.True(row.QuerySelector("input[type=checkbox]")!.HasAttribute("disabled"));
        row.Click();

        Assert.Empty(cut.Instance.SelectedItems);
        cut.Find("thead input[type=checkbox]").Change(true);
        Assert.Equal(2, cut.Instance.SelectedItems.Count);
        Assert.True(cut.Find("thead input[type=checkbox]").HasAttribute("checked"));
    }

    /// <summary>Verifies an item comparer keeps selection attached to refreshed row instances.</summary>
    [Fact]
    public void ComparerKeepsSelectionAfterItemsRefresh()
    {
        var products = DataGridTestHelpers.Products.ToArray();
        var cut = RenderSelectionGrid(
            products,
            selectionMode: VekDataGridSelectionMode.Multiple,
            comparer: EqualityComparer<TestProduct>.Create((left, right) => left?.Name == right?.Name, product => product.Name.GetHashCode()));
        cut.Find("tbody tr:nth-child(2) input[type=checkbox]").Change(true);

        products = products.Select(product => product with { }).ToArray();
        cut.Render(parameters => parameters.Add(grid => grid.Items, products));

        Assert.Equal("Desk Lamp", cut.Instance.SelectedItems.Single().Name);
        Assert.True(cut.Find("tbody tr:nth-child(2) input[type=checkbox]").HasAttribute("checked"));
    }

    /// <summary>Verifies the configured policy clears selections when a row is filtered out.</summary>
    [Fact]
    public async Task FilteredOutSelectionFollowsConfiguredPolicy()
    {
        var cut = RenderSelectionGrid(
            selectionMode: VekDataGridSelectionMode.Multiple,
            filteredSelectionMode: VekDataGridFilteredSelectionMode.Clear);
        cut.Find("tbody tr:nth-child(1) input[type=checkbox]").Change(true);

        await cut.InvokeAsync(() => cut.Instance.ApplyFilterAsync("Name", VekDataGridFilterOperator.Contains, "Notebook"));

        Assert.Empty(cut.Instance.SelectedItems);
    }

    private IRenderedComponent<VekDataGrid<TestProduct>> RenderSelectionGrid(
        TestProduct[]? items = null,
        VekDataGridSelectionMode selectionMode = VekDataGridSelectionMode.None,
        VekDataGridFilteredSelectionMode filteredSelectionMode = VekDataGridFilteredSelectionMode.Keep,
        bool rowClickSelection = false,
        Func<TestProduct, bool>? isRowSelectable = null,
        IEqualityComparer<TestProduct>? comparer = null) => Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, items ?? DataGridTestHelpers.Products)
            .Add(grid => grid.PageSize, 10)
            .Add(grid => grid.SelectionMode, selectionMode)
            .Add(grid => grid.RowClickSelection, rowClickSelection)
            .Add(grid => grid.IsRowSelectable, isRowSelectable)
            .Add(grid => grid.SelectionComparer, comparer)
            .Add(grid => grid.FilteredSelectionMode, filteredSelectionMode)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<SelectionColumn<TestProduct>>(0);
                builder.CloseComponent();
                builder.OpenComponent<PropertyColumn<TestProduct, string>>(1);
                builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, string>.Property), (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Name));
                builder.AddAttribute(3, nameof(PropertyColumn<TestProduct, string>.Key), "Name");
                builder.CloseComponent();
            }));
}
