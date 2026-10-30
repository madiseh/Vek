using Bunit;
using Microsoft.AspNetCore.Components.Web;
using Vek.Blazor.DataGrid.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class SortingTests : BunitContext
{
    /// <summary>
    /// Verifies that clicking anywhere in the column heading cycles through ascending, descending, and unsorted.
    /// </summary>
    [Fact]
    public void ClickingNameHeaderCyclesSortDirectionAndReturnsToOriginalOrder()
    {
        var cut = DataGridTestHelpers.RenderGrid(this, pageSize: 3);

        cut.Find("thead button.sort-button").Click();
        cut.WaitForAssertion(() =>
            Assert.Equal(
                new[] { "Desk Lamp", "Notebook Set", "Task Chair" },
                DataGridTestHelpers.GetRowNames(cut)));

        cut.Find("thead button.sort-button").Click();
        cut.WaitForAssertion(() =>
            Assert.Equal(
                new[] { "Task Chair", "Notebook Set", "Desk Lamp" },
                DataGridTestHelpers.GetRowNames(cut)));

        cut.Find("thead button.sort-button").Click();
        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                new[] { "Task Chair", "Desk Lamp", "Notebook Set" },
                DataGridTestHelpers.GetRowNames(cut));
            Assert.Equal("none", cut.Find("thead th").GetAttribute("aria-sort"));
        });
    }

    /// <summary>Verifies clicking the column heading outside its title button also sorts the column.</summary>
    [Fact]
    public void ClickingColumnHeaderAreaSortsColumn()
    {
        var cut = DataGridTestHelpers.RenderGrid(this, pageSize: 3);

        cut.Find("thead th").Click();

        Assert.Equal(
            new[] { "Desk Lamp", "Notebook Set", "Task Chair" },
            DataGridTestHelpers.GetRowNames(cut));
    }

    /// <summary>
    /// Verifies that disabling sorting leaves rows in their original order after a header click.
    /// </summary>
    [Fact]
    public void SortModeCanDisableSorting()
    {
        var cut = DataGridTestHelpers.RenderGrid(this, pageSize: 3, VekDataGridSortMode.None);

        cut.Find("thead th").Click();

        Assert.Equal(
            new[] { "Task Chair", "Desk Lamp", "Notebook Set" },
            DataGridTestHelpers.GetRowNames(cut));
        Assert.Equal("none", cut.Find("thead th").GetAttribute("aria-sort"));
    }

    /// <summary>
    /// Verifies that Ctrl-clicking a second heading adds a secondary sort after the first column.
    /// </summary>
    [Fact]
    public void MultipleSortUsesPriorityAndDirection()
    {
        var cut = DataGridTestHelpers.RenderNameAndCategoryGrid(this, VekDataGridSortMode.Multiple);

        cut.Find("thead th:nth-child(2) button").Click();
        cut.Find("thead th:nth-child(1) button").Click(new MouseEventArgs { CtrlKey = true });

        var names = cut.FindAll("tbody tr")
            .Select(row => row.Children[0].TextContent.Trim())
            .ToArray();
        Assert.Equal(new[] { "Desk Lamp", "Task Chair", "Notebook Set" }, names);
    }

    /// <summary>
    /// Verifies that a custom selector and comparer determine the resulting row order.
    /// </summary>
    [Fact]
    public void CustomSelectorAndComparerControlOrder()
    {
        var cut = DataGridTestHelpers.RenderCustomSortGrid(this);

        cut.Find("thead button").Click();

        Assert.Equal(
            new[] { "Notebook Set", "Task Chair", "Desk Lamp" },
            DataGridTestHelpers.GetRowNames(cut));
    }

    /// <summary>
    /// Verifies that the public state exposes ordered column keys after multi-column sorting.
    /// </summary>
    [Fact]
    public void SortChangeReportsOrderedState()
    {
        var cut = DataGridTestHelpers.RenderNameAndCategoryGrid(this, VekDataGridSortMode.Multiple);

        cut.Find("thead th:nth-child(2) button").Click();
        cut.Find("thead th:nth-child(1) button").Click(new MouseEventArgs { CtrlKey = true });

        Assert.Equal(
            new[] { "product => product.Category", "product => product.Name" },
            cut.Instance.CurrentState.Sorts.Select(sort => sort.ColumnKey));
    }
}
