// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;

namespace Vek.UnitTests.Components.DataGrid;

#nullable enable

[TestFixture]
public class DataGridKeyboardNavigationTests : BunitTest
{
    public class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public bool IsSelectionDisabled { get; set; }
    }

    private static List<TestModel> CreateSampleData() =>
    [
        new() { Id = 1, Name = "Alice Johnson", Department = "Engineering" },
        new() { Id = 2, Name = "Bob Smith", Department = "Marketing" },
        new() { Id = 3, Name = "Charlie Brown", Department = "Finance" },
    ];

    [Test]
    public void KeyboardNavigation_DisabledByDefault_ShouldNotRenderGridRoleOrCellTabIndex()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, CreateSampleData())
            .Add(p => p.Columns, RenderColumns));

        comp.Instance.KeyboardNavigationEnabled.Should().BeFalse();

        var table = comp.Find("table.vek-table-root");
        table.GetAttribute("role").Should().BeNull();
        table.GetAttribute("aria-activedescendant").Should().BeNull();
        table.GetAttribute("tabindex").Should().BeNull();

        var cells = comp.FindAll("tbody td");
        cells.Count.Should().Be(9);
        foreach (var cell in cells)
        {
            cell.GetAttribute("tabindex").Should().BeNull();
            cell.GetAttribute("role").Should().BeNull();
            cell.ClassList.Should().NotContain("vek-table-cell-focused");
        }
    }

    [Test]
    public void KeyboardNavigation_WhenEnabled_SetsGridRoleAndRovingTabIndexAcrossCells()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, CreateSampleData())
            .Add(p => p.KeyboardNavigationEnabled, true)
            .Add(p => p.FocusedRowEnabled, true)
            .Add(p => p.Columns, RenderColumns));

        var table = comp.Find("table.vek-table-root");
        table.GetAttribute("role").Should().Be("grid");
        table.GetAttribute("tabindex").Should().BeNull();

        var rows = comp.FindAll("tbody tr.vek-table-row");
        foreach (var row in rows)
        {
            row.GetAttribute("tabindex").Should().BeNull();
        }

        var cells = comp.FindAll("tbody td");
        cells.Count.Should().Be(9);

        cells[0].GetAttribute("role").Should().Be("gridcell");
        cells[0].GetAttribute("tabindex").Should().Be("0");
        cells[0].ClassList.Should().Contain("vek-table-cell-focused");
        table.GetAttribute("aria-activedescendant").Should().Be(cells[0].GetAttribute("id"));

        for (var i = 1; i < cells.Count; i++)
        {
            cells[i].GetAttribute("role").Should().Be("gridcell");
            cells[i].GetAttribute("tabindex").Should().Be("-1");
            cells[i].ClassList.Should().NotContain("vek-table-cell-focused");
        }
    }

    [Test]
    public async Task KeyboardNavigation_ArrowKeys_NavigateCellsWithBoundaryClamping()
    {
        var focusedRow = 0;
        var focusedCol = 0;

        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, CreateSampleData())
            .Add(p => p.KeyboardNavigationEnabled, true)
            .Add(p => p.FocusedRowIndex, focusedRow)
            .Add(p => p.FocusedRowIndexChanged, EventCallback.Factory.Create<int>(this, v => focusedRow = v))
            .Add(p => p.FocusedColumnIndex, focusedCol)
            .Add(p => p.FocusedColumnIndexChanged, EventCallback.Factory.Create<int>(this, v => focusedCol = v))
            .Add(p => p.Columns, RenderColumns));

        var table = comp.Find("table.vek-table-root");

        // At (0, 0), ArrowLeft and ArrowUp clamp at 0
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowLeft" });
        focusedRow.Should().Be(0);
        focusedCol.Should().Be(0);

        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowUp" });
        focusedRow.Should().Be(0);
        focusedCol.Should().Be(0);

        // Move right -> (0, 1)
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });
        focusedRow.Should().Be(0);
        focusedCol.Should().Be(1);

        // Move down -> (1, 1)
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        focusedRow.Should().Be(1);
        focusedCol.Should().Be(1);

        // Move right -> (1, 2) and clamp right
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });
        focusedRow.Should().Be(1);
        focusedCol.Should().Be(2);

        // Move down -> (2, 2) and clamp bottom
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        focusedRow.Should().Be(2);
        focusedCol.Should().Be(2);

        var cells = comp.FindAll("tbody td");
        cells[8].GetAttribute("tabindex").Should().Be("0");
        cells[8].ClassList.Should().Contain("vek-table-cell-focused");
        cells[0].GetAttribute("tabindex").Should().Be("-1");
        comp.Find("table.vek-table-root").GetAttribute("aria-activedescendant").Should().Be(cells[8].GetAttribute("id"));
    }

    [Test]
    public async Task KeyboardNavigation_HomeEndAndPageKeys_NavigateMatrix()
    {
        var items = Enumerable.Range(0, 12)
            .Select(i => new TestModel { Id = i + 1, Name = $"User {i + 1}", Department = "Ops" })
            .ToList();

        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.KeyboardNavigationEnabled, true)
            .Add(p => p.RowsPerPage, 5)
            .Add(p => p.FocusedRowIndex, 1)
            .Add(p => p.FocusedColumnIndex, 1)
            .Add(p => p.Columns, RenderColumns));

        var table = comp.Find("table.vek-table-root");

        // End -> end of current row (1, 2)
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "End" });
        comp.Instance.KeyboardHandler.ActiveRowIndex.Should().Be(1);
        comp.Instance.KeyboardHandler.ActiveColumnIndex.Should().Be(2);

        // Home -> start of current row (1, 0)
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "Home" });
        comp.Instance.KeyboardHandler.ActiveRowIndex.Should().Be(1);
        comp.Instance.KeyboardHandler.ActiveColumnIndex.Should().Be(0);

        // Ctrl+End -> bottom-right cell (11, 2)
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "End", CtrlKey = true });
        comp.Instance.KeyboardHandler.ActiveRowIndex.Should().Be(11);
        comp.Instance.KeyboardHandler.ActiveColumnIndex.Should().Be(2);

        // Ctrl+Home -> top-left cell (0, 0)
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "Home", CtrlKey = true });
        comp.Instance.KeyboardHandler.ActiveRowIndex.Should().Be(0);
        comp.Instance.KeyboardHandler.ActiveColumnIndex.Should().Be(0);

        // PageDown -> jumps by RowsPerPage (5, 0)
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "PageDown" });
        comp.Instance.KeyboardHandler.ActiveRowIndex.Should().Be(5);
        comp.Instance.KeyboardHandler.ActiveColumnIndex.Should().Be(0);

        // PageUp -> returns to (0, 0)
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "PageUp" });
        comp.Instance.KeyboardHandler.ActiveRowIndex.Should().Be(0);
        comp.Instance.KeyboardHandler.ActiveColumnIndex.Should().Be(0);
    }

    [Test]
    public async Task KeyboardNavigation_HiddenColumns_NavigatesVisibleColumnsOnly()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, CreateSampleData())
            .Add(p => p.KeyboardNavigationEnabled, true)
            .Add(p => p.Columns, builder =>
            {
                RenderColumn(builder, 0, x => x.Id, "ID");
                builder.OpenComponent<PropertyColumn<TestModel, string>>(10);
                builder.AddAttribute(11, nameof(PropertyColumn<TestModel, string>.Property), (Expression<Func<TestModel, string>>)(x => x.Name));
                builder.AddAttribute(12, nameof(PropertyColumn<TestModel, string>.Title), "Name");
                builder.AddAttribute(13, nameof(PropertyColumn<TestModel, string>.Hidden), true);
                builder.CloseComponent();
                RenderColumn(builder, 2, x => x.Department, "Department");
            }));

        var table = comp.Find("table.vek-table-root");
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "End" });

        comp.Instance.KeyboardHandler.ActiveColumnIndex.Should().Be(1);
        comp.FindAll("tbody tr")[0].QuerySelectorAll("td")[1].GetAttribute("tabindex").Should().Be("0");
    }

    [Test]
    public async Task KeyboardNavigation_F2EnterAndEscape_ControlInlineEditLifecycle()
    {
        var items = CreateSampleData();
        var committedItems = new List<TestModel>();
        var canceledItems = new List<TestModel>();

        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.KeyboardNavigationEnabled, true)
            .Add(p => p.ReadOnly, false)
            .Add(p => p.EditMode, DataGridEditMode.Inline)
            .Add(p => p.FocusedRowIndex, 0)
            .Add(p => p.FocusedColumnIndex, 1)
            .Add(p => p.CommittedItemChanged, EventCallback.Factory.Create<TestModel>(this, item => committedItems.Add(item)))
            .Add(p => p.CanceledEditingItem, EventCallback.Factory.Create<TestModel>(this, item => canceledItems.Add(item)))
            .Add(p => p.Columns, RenderColumns));

        var table = comp.Find("table.vek-table-root");

        // F2 activates inline editing on row 0
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "F2" });
        comp.Instance.IsEditingItem(items[0]).Should().BeTrue();
        comp.FindAll("tbody tr")[0].QuerySelectorAll("input").Count.Should().BeGreaterThan(0);

        // Escape cancels editing without committing
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "Escape" });
        comp.Instance.IsEditingItem(items[0]).Should().BeFalse();
        canceledItems.Should().ContainSingle();
        committedItems.Should().BeEmpty();

        // Enter starts editing when not editing, then second Enter commits inline edits
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
        comp.Instance.IsEditingItem(items[0]).Should().BeTrue();

        var nameInput = comp.FindAll("tbody tr")[0].QuerySelectorAll("input")[1];
        await nameInput.InputAsync("Alice Updated");

        await table.KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
        comp.Instance.IsEditingItem(items[0]).Should().BeFalse();
        items[0].Name.Should().Be("Alice Updated");
        committedItems.Should().ContainSingle();
    }

    [Test]
    public async Task KeyboardNavigation_ReadOnlyGrid_IgnoresEditShortcuts()
    {
        var items = CreateSampleData();
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.KeyboardNavigationEnabled, true)
            .Add(p => p.ReadOnly, true)
            .Add(p => p.EditMode, DataGridEditMode.Inline)
            .Add(p => p.Columns, RenderColumns));

        var table = comp.Find("table.vek-table-root");
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "F2" });
        comp.Instance.IsEditingItem(items[0]).Should().BeFalse();

        await table.KeyDownAsync(new KeyboardEventArgs { Key = "Enter" });
        comp.Instance.IsEditingItem(items[0]).Should().BeFalse();
    }

    [Test]
    public async Task KeyboardNavigation_F2AndEscape_ControlFormAndCellEditModes()
    {
        var items = CreateSampleData();

        var formComp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.KeyboardNavigationEnabled, true)
            .Add(p => p.ReadOnly, false)
            .Add(p => p.EditMode, DataGridEditMode.Form)
            .Add(p => p.Columns, RenderColumns));

        var formTable = formComp.Find("table.vek-table-root");
        await formTable.KeyDownAsync(new KeyboardEventArgs { Key = "F2" });
        formComp.Instance._isEditFormOpen.Should().BeTrue();

        await formTable.KeyDownAsync(new KeyboardEventArgs { Key = "Escape" });
        formComp.Instance._isEditFormOpen.Should().BeFalse();

        var canceledCellItem = default(TestModel);
        var cellComp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.KeyboardNavigationEnabled, true)
            .Add(p => p.ReadOnly, false)
            .Add(p => p.EditMode, DataGridEditMode.Cell)
            .Add(p => p.CanceledEditingItem, EventCallback.Factory.Create<TestModel>(this, item => canceledCellItem = item))
            .Add(p => p.Columns, RenderColumns));

        var cellTable = cellComp.Find("table.vek-table-root");
        await cellTable.KeyDownAsync(new KeyboardEventArgs { Key = "F2" });
        cellComp.Instance.HasActiveEdit.Should().BeTrue();

        await cellTable.KeyDownAsync(new KeyboardEventArgs { Key = "Escape" });
        cellComp.Instance.HasActiveEdit.Should().BeFalse();
        canceledCellItem.Should().Be(items[0]);
    }

    [Test]
    public async Task KeyboardNavigation_Space_TogglesSelectionAndRespectsGuards()
    {
        var items = CreateSampleData();
        items[1].IsSelectionDisabled = true;

        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, items)
            .Add(p => p.KeyboardNavigationEnabled, true)
            .Add(p => p.MultiSelection, true)
            .Add(p => p.ReadOnly, false)
            .Add(p => p.EditMode, DataGridEditMode.Inline)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<SelectColumn<TestModel>>(0);
                builder.AddAttribute(1, nameof(SelectColumn<TestModel>.DisabledFunc), (Func<TestModel, bool>)(x => x.IsSelectionDisabled));
                builder.CloseComponent();
                RenderColumn(builder, 1, x => x.Id, "ID");
                RenderColumn(builder, 2, x => x.Name, "Name");
            }));

        var table = comp.Find("table.vek-table-root");

        // Space selects row 0
        await table.KeyDownAsync(new KeyboardEventArgs { Key = " " });
        comp.Instance.Selection.Should().Contain(items[0]);

        // Space again deselects row 0
        await table.KeyDownAsync(new KeyboardEventArgs { Key = " " });
        comp.Instance.Selection.Should().NotContain(items[0]);

        // Space does not toggle while inline-editing row 0
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "F2" });
        await table.KeyDownAsync(new KeyboardEventArgs { Key = " " });
        comp.Instance.Selection.Should().NotContain(items[0]);
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "Escape" });

        // Move to row 1 (disabled for selection) -> Space does nothing
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        await table.KeyDownAsync(new KeyboardEventArgs { Key = " " });
        comp.Instance.Selection.Should().NotContain(items[1]);

        // Move to row 2 with SelectionChangeable=false -> Space does nothing
        await comp.SetParametersAndRenderAsync(p => p.Add(x => x.SelectionChangeable, false));
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "ArrowDown" });
        await table.KeyDownAsync(new KeyboardEventArgs { Key = " " });
        comp.Instance.Selection.Should().NotContain(items[2]);
    }

    [Test]
    public async Task KeyboardNavigation_Tab_MovesOutWithoutTrappingAndHasSingleTabStop()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, CreateSampleData())
            .Add(p => p.KeyboardNavigationEnabled, true)
            .Add(p => p.FocusedRowEnabled, true)
            .Add(p => p.Columns, RenderColumns));

        // Exactly one tabbable element (tabindex="0") in the grid matrix so Tab exits immediately
        comp.FindAll("[tabindex='0']").Count.Should().Be(1);

        var handled = await comp.Instance.KeyboardHandler.HandleKeyDownAsync(new KeyboardEventArgs { Key = "Tab" });
        handled.Should().BeFalse();

        // Active cell coordinates remain unchanged after Tab
        comp.Instance.KeyboardHandler.ActiveRowIndex.Should().Be(0);
        comp.Instance.KeyboardHandler.ActiveColumnIndex.Should().Be(0);
    }

    [Test]
    public async Task KeyboardNavigation_ClickingCell_UpdatesFocusedCellAndRovingTabIndex()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, CreateSampleData())
            .Add(p => p.KeyboardNavigationEnabled, true)
            .Add(p => p.Columns, RenderColumns));

        var cells = comp.FindAll("tbody td");
        await cells[4].ClickAsync(new MouseEventArgs());

        comp.Instance.KeyboardHandler.ActiveRowIndex.Should().Be(1);
        comp.Instance.KeyboardHandler.ActiveColumnIndex.Should().Be(1);

        var updatedCells = comp.FindAll("tbody td");
        updatedCells[4].GetAttribute("tabindex").Should().Be("0");
        updatedCells[4].ClassList.Should().Contain("vek-table-cell-focused");
        updatedCells[0].GetAttribute("tabindex").Should().Be("-1");
        comp.Find("table.vek-table-root").GetAttribute("aria-activedescendant").Should().Be(updatedCells[4].GetAttribute("id"));
    }

    [Test]
    public async Task KeyboardNavigation_EmptyGrid_DoesNotThrowAndHasNoActiveDescendant()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, Array.Empty<TestModel>())
            .Add(p => p.KeyboardNavigationEnabled, true)
            .Add(p => p.Columns, RenderColumns));

        comp.Find("table.vek-table-root").GetAttribute("aria-activedescendant").Should().BeNull();
        comp.Instance.KeyboardHandler.ActiveRowIndex.Should().Be(-1);

        var handled = await comp.Instance.KeyboardHandler.HandleKeyDownAsync(new KeyboardEventArgs { Key = "ArrowRight" });
        handled.Should().BeFalse();
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
