using System.Linq.Expressions;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;

namespace Vek.UnitTests.Components.DataGrid;

[TestFixture]
public class DataGridCellRangeTests : BunitTest
{
    private sealed record SalesRow(string Team, int January, int February);

    private static readonly SalesRow[] SampleRows =
    [
        new("North", 10, 20),
        new("South", 30, 40)
    ];

    private DataGridCellRange? _lastSelectedRange;

    /// <summary>
    /// Includes both endpoints and normalizes ranges selected from either direction.
    /// </summary>
    [Test]
    public void DataGridCellRange_ContainsCellsInReverseCoordinates()
    {
        var range = new DataGridCellRange(2, 3, 0, 1);

        range.MinRow.Should().Be(0);
        range.MaxRow.Should().Be(2);
        range.MinColumn.Should().Be(1);
        range.MaxColumn.Should().Be(3);
        range.CellCount.Should().Be(9);
        range.Contains(0, 1).Should().BeTrue();
        range.Contains(2, 3).Should().BeTrue();
        range.Contains(1, 2).Should().BeTrue();
        range.Contains(2, 4).Should().BeFalse();
    }

    /// <summary>
    /// Shift-click expands the selected cells and updates the numeric status summary.
    /// </summary>
    [Test]
    public async Task ShiftClick_ExpandsCellRangeAndUpdatesSummary()
    {
        var grid = RenderGrid();
        var firstCell = grid.FindAll("tbody td")[1];
        await firstCell.ClickAsync();

        var cells = grid.FindAll("tbody td");
        await cells[5].ClickAsync(new MouseEventArgs { ShiftKey = true });

        _lastSelectedRange.Should().Be(new DataGridCellRange(0, 1, 1, 2));
        grid.Find("table.vek-table-root").GetAttribute("role").Should().Be("grid");
        grid.FindAll(".vek-data-grid-cell-selected").Should().HaveCount(4);
        grid.FindAll("tbody td[aria-selected='true']").Should().HaveCount(4);
        grid.FindAll("tbody td[role='gridcell']").Should().HaveCount(6);
        grid.Find(".vek-data-grid-cell-range-summary").TextContent.Should().Contain("Sum: 100");
        grid.Find(".vek-data-grid-cell-range-summary").TextContent.Should().Contain("Average: 25");
        grid.Find(".vek-data-grid-cell-range-summary").TextContent.Should().Contain("Count: 4");
    }

    /// <summary>
    /// Dragging across cells selects every cell in the rectangular pointer range.
    /// </summary>
    [Test]
    public async Task PointerDrag_SelectsRectangularRange()
    {
        var grid = RenderGrid();
        await grid.FindAll("tbody td")[1].TriggerEventAsync("onmousedown", new MouseEventArgs());
        await grid.FindAll("tbody td")[5].TriggerEventAsync("onmouseover", new MouseEventArgs());
        await grid.FindAll("tbody td")[5].TriggerEventAsync("onmouseup", new MouseEventArgs());

        _lastSelectedRange.Should().Be(new DataGridCellRange(0, 1, 1, 2));
        grid.FindAll(".vek-data-grid-cell-selected").Should().HaveCount(4);
    }

    /// <summary>
    /// Shift plus an arrow key extends the active cell selection within grid bounds.
    /// </summary>
    [Test]
    public async Task ShiftArrow_ExtendsCellRange()
    {
        var grid = RenderGrid();
        await grid.FindAll("tbody td")[1].ClickAsync();

        await grid.Find("table.vek-table-root").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight", ShiftKey = true });

        _lastSelectedRange.Should().Be(new DataGridCellRange(0, 1, 0, 2));
        grid.FindAll(".vek-data-grid-cell-selected").Should().HaveCount(2);
    }

    private IRenderedComponent<VekDataGrid<SalesRow>> RenderGrid()
    {
        return Context.Render<VekDataGrid<SalesRow>>(parameters => parameters
            .Add(grid => grid.Items, SampleRows)
            .Add(grid => grid.SelectionMode, DataGridSelectionMode.CellRange)
            .Add(grid => grid.SelectedCellRangeChanged, range => _lastSelectedRange = range)
            .Add(grid => grid.Columns, RenderColumns));
    }

    private static void RenderColumns(RenderTreeBuilder builder)
    {
        builder.OpenComponent<PropertyColumn<SalesRow, string>>(0);
        builder.AddAttribute(1, nameof(PropertyColumn<SalesRow, string>.Property), (Expression<Func<SalesRow, string>>)(row => row.Team));
        builder.AddAttribute(2, nameof(PropertyColumn<SalesRow, string>.Title), "Team");
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<SalesRow, int>>(3);
        builder.AddAttribute(4, nameof(PropertyColumn<SalesRow, int>.Property), (Expression<Func<SalesRow, int>>)(row => row.January));
        builder.AddAttribute(5, nameof(PropertyColumn<SalesRow, int>.Title), "January");
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<SalesRow, int>>(6);
        builder.AddAttribute(7, nameof(PropertyColumn<SalesRow, int>.Property), (Expression<Func<SalesRow, int>>)(row => row.February));
        builder.AddAttribute(8, nameof(PropertyColumn<SalesRow, int>.Title), "February");
        builder.CloseComponent();
    }
}
