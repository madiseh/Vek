using System.Linq.Expressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;
using Vek;
using Vek.UnitTests.Shared;

namespace Vek.UnitTests.Components.DataGrid;

[TestFixture]
public class DataGridRowReorderTests : BunitTest
{
    private sealed record TestRow(int Id, string Name);

    private static readonly TestRow[] Rows =
    [
        new(1, "Northwind"),
        new(2, "Contoso"),
        new(3, "Fabrikam")
    ];

    /// <summary>
    /// Dropping a row on another row reports the source, target, and final insertion index.
    /// </summary>
    [Test]
    public async Task PointerDrop_ReportsFinalInsertionPosition()
    {
        var events = new List<DataGridRowReorderedEventArgs<TestRow>>();
        var grid = RenderGrid(events);

        await grid.Find(".vek-data-grid-row-drag-handle").TriggerEventAsync("ondragstart", new DragEventArgs());
        await grid.FindAll("tbody tr.vek-table-row")[2].TriggerEventAsync("ondrop", new DragEventArgs());

        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0].SourceItem, Is.EqualTo(Rows[0]));
        Assert.That(events[0].TargetItem, Is.EqualTo(Rows[2]));
        Assert.That(events[0].InsertPosition, Is.EqualTo(1));
    }

    /// <summary>
    /// Space, arrow, and Enter keys complete the same row move as pointer dragging.
    /// </summary>
    [Test]
    public async Task KeyboardMove_ReportsDestinationPosition()
    {
        var events = new List<DataGridRowReorderedEventArgs<TestRow>>();
        var grid = RenderGrid(events);
        var handle = grid.Find(".vek-data-grid-row-drag-handle");

        await handle.KeyDownAsync(new KeyboardEventArgs { Key = " " });
        await grid.Find(".vek-data-grid-row-drag-handle").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        await grid.Find(".vek-data-grid-row-drag-handle").KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });

        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0].SourceItem, Is.EqualTo(Rows[0]));
        Assert.That(events[0].TargetItem, Is.EqualTo(Rows[1]));
        Assert.That(events[0].InsertPosition, Is.EqualTo(1));
    }

    /// <summary>
    /// Reordering controls stay disabled until row reorder is enabled and a callback is supplied.
    /// </summary>
    [Test]
    public void Handle_IsDisabledWithoutEnabledCallback()
    {
        var grid = Context.Render<VekDataGrid<TestRow>>(parameters => parameters
            .Add(p => p.Items, Rows)
            .Add(p => p.AllowRowReorder, true)
            .Add(p => p.Columns, RenderColumns));

        Assert.That(grid.Find(".vek-data-grid-row-drag-handle").HasAttribute("disabled"), Is.True);
    }

    /// <summary>
    /// Dropping a row on itself does not request a move.
    /// </summary>
    [Test]
    public async Task PointerDropOnSource_DoesNotRaiseCallback()
    {
        var events = new List<DataGridRowReorderedEventArgs<TestRow>>();
        var grid = RenderGrid(events);

        await grid.Find(".vek-data-grid-row-drag-handle").TriggerEventAsync("ondragstart", new DragEventArgs());
        await grid.Find("tbody tr.vek-table-row").TriggerEventAsync("ondrop", new DragEventArgs());

        Assert.That(events, Is.Empty);
    }

    /// <summary>
    /// Escape cancels a keyboard reorder without invoking the callback.
    /// </summary>
    [Test]
    public async Task KeyboardMove_EscapeCancels()
    {
        var events = new List<DataGridRowReorderedEventArgs<TestRow>>();
        var grid = RenderGrid(events);
        var handle = grid.Find(".vek-data-grid-row-drag-handle");

        await handle.KeyDownAsync(new KeyboardEventArgs { Key = " " });
        await grid.Find(".vek-data-grid-row-drag-handle").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        await grid.Find(".vek-data-grid-row-drag-handle").KeyDownAsync(new KeyboardEventArgs { Key = "Escape" });

        Assert.That(events, Is.Empty);
        Assert.That(grid.Find(".vek-data-grid-row-drag-handle").GetAttribute("aria-pressed"), Is.EqualTo("false"));
    }

    /// <summary>
    /// Keyboard movement at the last row stays within the visible item range.
    /// </summary>
    [Test]
    public async Task KeyboardMove_AtLastRowStaysInRange()
    {
        var events = new List<DataGridRowReorderedEventArgs<TestRow>>();
        var grid = RenderGrid(events);
        var handle = grid.FindAll(".vek-data-grid-row-drag-handle")[^1];

        await handle.KeyDownAsync(new KeyboardEventArgs { Key = " " });
        await grid.FindAll(".vek-data-grid-row-drag-handle")[^1].KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        await grid.FindAll(".vek-data-grid-row-drag-handle")[^1].KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });

        Assert.That(events, Is.Empty);
    }

    /// <summary>
    /// Reversing keyboard movement still reports the selected final insertion position.
    /// </summary>
    [Test]
    public async Task KeyboardMove_ReversingDirectionKeepsDestination()
    {
        var events = new List<DataGridRowReorderedEventArgs<TestRow>>();
        var grid = RenderGrid(events);

        await grid.Find(".vek-data-grid-row-drag-handle").KeyDownAsync(new KeyboardEventArgs { Key = " " });
        await grid.Find(".vek-data-grid-row-drag-handle").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        await grid.Find(".vek-data-grid-row-drag-handle").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        await grid.Find(".vek-data-grid-row-drag-handle").KeyDownAsync(new KeyboardEventArgs { Key = "ArrowUp" });
        await grid.Find(".vek-data-grid-row-drag-handle").KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });

        Assert.That(events, Has.Count.EqualTo(1));
        Assert.That(events[0].TargetItem, Is.EqualTo(Rows[1]));
        Assert.That(events[0].InsertPosition, Is.EqualTo(1));
    }

    private IRenderedComponent<VekDataGrid<TestRow>> RenderGrid(List<DataGridRowReorderedEventArgs<TestRow>> events)
    {
        return Context.Render<VekDataGrid<TestRow>>(parameters => parameters
            .Add(p => p.Items, Rows)
            .Add(p => p.AllowRowReorder, true)
            .Add(p => p.OnRowReordered, EventCallback.Factory.Create<DataGridRowReorderedEventArgs<TestRow>>(this, events.Add))
            .Add(p => p.Columns, RenderColumns));
    }

    private static void RenderColumns(RenderTreeBuilder builder)
    {
        builder.OpenComponent<DataGridDragHandleColumn<TestRow>>(0);
        builder.CloseComponent();
        builder.OpenComponent<PropertyColumn<TestRow, string>>(1);
        builder.AddAttribute(2, nameof(PropertyColumn<TestRow, string>.Property), (Expression<Func<TestRow, string>>)(row => row.Name));
        builder.CloseComponent();
    }
}
