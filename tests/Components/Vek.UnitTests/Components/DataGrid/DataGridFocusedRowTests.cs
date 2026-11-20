// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;

namespace Vek.UnitTests.Components.DataGrid;

#nullable enable

[TestFixture]
public class DataGridFocusedRowTests : BunitTest
{
    public record TestModel(int Id, string Name, string Department, decimal Salary);

    private static readonly TestModel[] SampleData =
    [
        new(1, "Alice Johnson", "Engineering", 90000m),
        new(2, "Bob Smith", "Marketing", 75000m),
        new(3, "Charlie Brown", "Finance", 85000m),
    ];

    [Test]
    public void FocusedRow_DisabledByDefault_ShouldNotRenderFocusAttributesOrClasses()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.Columns, RenderColumns));

        comp.Instance.FocusedRowEnabled.Should().BeFalse();
        comp.Instance.FocusedRowIndex.Should().Be(-1);
        comp.Instance.FocusedItem.Should().BeNull();

        var table = comp.Find("table.vek-table-root");
        table.GetAttribute("tabindex").Should().BeNull();

        var rows = comp.FindAll("tbody tr.vek-table-row");
        rows.Count.Should().Be(3);
        foreach (var row in rows)
        {
            row.ClassList.Should().NotContain("vek-table-row-focused");
            row.GetAttribute("tabindex").Should().BeNull();
            row.GetAttribute("aria-current").Should().BeNull();
        }
    }

    [Test]
    public async Task FocusedRow_WhenDisabled_ClickingRowDoesNotSetFocus()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.FocusedRowEnabled, false)
            .Add(p => p.Columns, RenderColumns));

        var rows = comp.FindAll("tbody tr.vek-table-row");
        await rows[1].ClickAsync(new MouseEventArgs());

        comp.Instance.FocusedRowIndex.Should().Be(-1);
        comp.Instance.FocusedItem.Should().BeNull();

        var updatedRows = comp.FindAll("tbody tr.vek-table-row");
        updatedRows[1].ClassList.Should().NotContain("vek-table-row-focused");
    }

    [Test]
    public void FocusedRow_WhenEnabled_InitialIndexAppliesFocusedClassAndAria()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.FocusedRowEnabled, true)
            .Add(p => p.FocusedRowIndex, 1)
            .Add(p => p.Columns, RenderColumns));

        comp.Instance.CurrentFocusedIndex.Should().Be(1);

        var table = comp.Find("table.vek-table-root");
        table.GetAttribute("tabindex").Should().Be("0");

        var rows = comp.FindAll("tbody tr.vek-table-row");
        rows[0].ClassList.Should().NotContain("vek-table-row-focused");
        rows[0].GetAttribute("tabindex").Should().Be("-1");
        rows[0].GetAttribute("aria-current").Should().BeNull();

        rows[1].ClassList.Should().Contain("vek-table-row-focused");
        rows[1].GetAttribute("tabindex").Should().Be("0");
        rows[1].GetAttribute("aria-current").Should().Be("true");

        rows[2].ClassList.Should().NotContain("vek-table-row-focused");
        rows[2].GetAttribute("tabindex").Should().Be("-1");
        rows[2].GetAttribute("aria-current").Should().BeNull();
    }

    [Test]
    public void FocusedRow_WhenEnabled_InitialItemResolvesIndex()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.FocusedRowEnabled, true)
            .Add(p => p.FocusedItem, SampleData[2])
            .Add(p => p.Columns, RenderColumns));

        comp.Instance.CurrentFocusedIndex.Should().Be(2);

        var rows = comp.FindAll("tbody tr.vek-table-row");
        rows[2].ClassList.Should().Contain("vek-table-row-focused");
        rows[2].GetAttribute("tabindex").Should().Be("0");
        rows[2].GetAttribute("aria-current").Should().Be("true");
    }

    [Test]
    public async Task FocusedRow_ClickingRowSetsFocusAndFiresCallback()
    {
        var eventLog = new List<DataGridFocusedRowChangedEventArgs<TestModel>>();

        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.FocusedRowEnabled, true)
            .Add(p => p.FocusedRowChanged, EventCallback.Factory.Create<DataGridFocusedRowChangedEventArgs<TestModel>>(this, e => eventLog.Add(e)))
            .Add(p => p.Columns, RenderColumns));

        var rows = comp.FindAll("tbody tr.vek-table-row");
        await rows[1].ClickAsync(new MouseEventArgs());

        comp.Instance.CurrentFocusedIndex.Should().Be(1);
        comp.Instance.CurrentFocusedItem.Should().Be(SampleData[1]);

        eventLog.Count.Should().Be(1);
        eventLog[0].RowIndex.Should().Be(1);
        eventLog[0].Item.Should().Be(SampleData[1]);
        eventLog[0].PreviousRowIndex.Should().Be(-1);
        eventLog[0].PreviousItem.Should().BeNull();

        var updatedRows = comp.FindAll("tbody tr.vek-table-row");
        updatedRows[1].ClassList.Should().Contain("vek-table-row-focused");

        // Click another row
        await updatedRows[0].ClickAsync(new MouseEventArgs());

        comp.Instance.CurrentFocusedIndex.Should().Be(0);
        comp.Instance.CurrentFocusedItem.Should().Be(SampleData[0]);

        eventLog.Count.Should().Be(2);
        eventLog[1].RowIndex.Should().Be(0);
        eventLog[1].Item.Should().Be(SampleData[0]);
        eventLog[1].PreviousRowIndex.Should().Be(1);
        eventLog[1].PreviousItem.Should().Be(SampleData[1]);
    }

    [Test]
    public async Task FocusedRow_KeyboardNavigation_ArrowDownAndArrowUp()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.FocusedRowEnabled, true)
            .Add(p => p.FocusedRowIndex, 0)
            .Add(p => p.Columns, RenderColumns));

        var table = comp.Find("table.vek-table-root");

        // Navigate Down -> row 1
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        comp.Instance.CurrentFocusedIndex.Should().Be(1);
        comp.Instance.CurrentFocusedItem.Should().Be(SampleData[1]);

        // Navigate Down -> row 2 (last)
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        comp.Instance.CurrentFocusedIndex.Should().Be(2);

        // Navigate Down at boundary -> remains at row 2
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        comp.Instance.CurrentFocusedIndex.Should().Be(2);

        // Navigate Up -> row 1
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowUp" });
        comp.Instance.CurrentFocusedIndex.Should().Be(1);

        // Navigate Up -> row 0
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowUp" });
        comp.Instance.CurrentFocusedIndex.Should().Be(0);

        // Navigate Up at boundary -> remains at row 0
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowUp" });
        comp.Instance.CurrentFocusedIndex.Should().Be(0);
    }

    [Test]
    public async Task FocusedRow_KeyboardNavigation_HomeAndEnd()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.FocusedRowEnabled, true)
            .Add(p => p.FocusedRowIndex, 1)
            .Add(p => p.Columns, RenderColumns));

        var table = comp.Find("table.vek-table-root");

        // End -> row 2
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "End" });
        comp.Instance.CurrentFocusedIndex.Should().Be(2);

        // Home -> row 0
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "Home" });
        comp.Instance.CurrentFocusedIndex.Should().Be(0);
    }

    [Test]
    public async Task FocusedRow_KeyboardNavigation_PageDownAndPageUp()
    {
        var largeData = Enumerable.Range(0, 20)
            .Select(i => new TestModel(i, $"Person {i}", "Dept", 50000m + (i * 1000m)))
            .ToArray();

        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, largeData)
            .Add(p => p.FocusedRowEnabled, true)
            .Add(p => p.RowsPerPage, 5)
            .Add(p => p.FocusedRowIndex, 0)
            .Add(p => p.Columns, RenderColumns));

        var table = comp.Find("table.vek-table-root");

        // PageDown jumps by rows per page (5)
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "PageDown" });
        comp.Instance.CurrentFocusedIndex.Should().Be(5);

        // PageUp jumps up
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "PageUp" });
        comp.Instance.CurrentFocusedIndex.Should().Be(0);
    }

    [Test]
    public async Task FocusedRow_FocusManager_MethodsDirectlyTested()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.FocusedRowEnabled, true)
            .Add(p => p.Columns, RenderColumns));

        var focusManager = comp.Instance.FocusManager;

        // Start from -1, MoveNext sets to 0
        var moved = await focusManager.MoveNextAsync();
        moved.Should().BeTrue();
        comp.Instance.CurrentFocusedIndex.Should().Be(0);

        // MoveNext -> 1
        moved = await focusManager.MoveNextAsync();
        moved.Should().BeTrue();
        comp.Instance.CurrentFocusedIndex.Should().Be(1);

        // MoveLast -> 2
        moved = await focusManager.MoveLastAsync();
        moved.Should().BeTrue();
        comp.Instance.CurrentFocusedIndex.Should().Be(2);

        // MoveFirst -> 0
        moved = await focusManager.MoveFirstAsync();
        moved.Should().BeTrue();
        comp.Instance.CurrentFocusedIndex.Should().Be(0);

        // Clear focus
        await focusManager.ClearFocusAsync();
        comp.Instance.CurrentFocusedIndex.Should().Be(-1);
        comp.Instance.CurrentFocusedItem.Should().BeNull();
    }

    [Test]
    public async Task FocusedRow_EmptyGrid_DoesNotThrow()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, Array.Empty<TestModel>())
            .Add(p => p.FocusedRowEnabled, true)
            .Add(p => p.Columns, RenderColumns));

        var focusManager = comp.Instance.FocusManager;
        var moved = await focusManager.MoveNextAsync();
        moved.Should().BeFalse();

        var handled = await focusManager.HandleKeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        handled.Should().BeFalse();
    }

    [Test]
    public async Task FocusedRow_TwoWayBinding_UpdatesConsumerState()
    {
        var focusedIndex = 0;
        TestModel? focusedItem = null;

        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.FocusedRowEnabled, true)
            .Add(p => p.FocusedRowIndex, focusedIndex)
            .Add(p => p.FocusedRowIndexChanged, EventCallback.Factory.Create<int>(this, val => focusedIndex = val))
            .Add(p => p.FocusedItem, focusedItem)
            .Add(p => p.FocusedItemChanged, EventCallback.Factory.Create<TestModel?>(this, val => focusedItem = val))
            .Add(p => p.Columns, RenderColumns));

        var rows = comp.FindAll("tbody tr.vek-table-row");
        await rows[2].ClickAsync(new MouseEventArgs());

        focusedIndex.Should().Be(2);
        focusedItem.Should().Be(SampleData[2]);
    }

    [Test]
    public async Task FocusedRow_IsIndependentFromSelection()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.FocusedRowEnabled, true)
            .Add(p => p.Columns, RenderColumns));

        // Select row 0 via Selection API
        await comp.Instance.SetSelectedItemAsync(true, SampleData[0]);

        // Focus row 2
        await comp.Instance.SetFocusedRowIndexAsync(2);

        comp.Instance.Selection.Should().Contain(SampleData[0]);
        comp.Instance.Selection.Should().NotContain(SampleData[2]);

        comp.Instance.CurrentFocusedIndex.Should().Be(2);
        comp.Instance.CurrentFocusedItem.Should().Be(SampleData[2]);

        var rows = comp.FindAll("tbody tr.vek-table-row");
        // Row 0 is selected but not focused
        rows[0].ClassList.Should().NotContain("vek-table-row-focused");

        // Row 2 is focused
        rows[2].ClassList.Should().Contain("vek-table-row-focused");
        rows[2].GetAttribute("tabindex").Should().Be("0");
        rows[2].GetAttribute("aria-current").Should().Be("true");
    }

    private static void RenderColumns(RenderTreeBuilder builder)
    {
        RenderColumn(builder, 0, x => x.Id, "ID");
        RenderColumn(builder, 1, x => x.Name, "Name");
        RenderColumn(builder, 2, x => x.Department, "Department");
    }

    private static void RenderColumn<TProp>(
        RenderTreeBuilder builder,
        int seq,
        Expression<Func<TestModel, TProp>> property,
        string title)
    {
        builder.OpenComponent<PropertyColumn<TestModel, TProp>>(seq * 10);
        builder.AddAttribute((seq * 10) + 1, nameof(PropertyColumn<TestModel, TProp>.Property), property);
        builder.AddAttribute((seq * 10) + 2, nameof(PropertyColumn<TestModel, TProp>.Title), title);
        builder.CloseComponent();
    }
}
