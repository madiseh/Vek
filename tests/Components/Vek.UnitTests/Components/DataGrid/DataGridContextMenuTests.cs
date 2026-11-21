// Copyright (c) Vek 2026
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq.Expressions;
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
public class DataGridContextMenuTests : BunitTest
{
    public record Employee(int Id, string Name, string Department, decimal Salary);

    private static readonly Employee[] SampleEmployees =
    [
        new(1, "Alice Johnson", "Engineering", 95000m),
        new(2, "Bob Smith", "Marketing", 72000m),
        new(3, "Charlie Davis", "Engineering", 88000m),
    ];

    private static void RenderDefaultColumns(RenderTreeBuilder builder)
    {
        builder.OpenComponent<PropertyColumn<Employee, int>>(0);
        builder.AddAttribute(1, nameof(PropertyColumn<Employee, int>.Property), (Expression<Func<Employee, int>>)(x => x.Id));
        builder.AddAttribute(2, nameof(PropertyColumn<Employee, int>.Title), "ID");
        builder.AddAttribute(3, nameof(PropertyColumn<Employee, int>.Sortable), true);
        builder.AddAttribute(4, nameof(PropertyColumn<Employee, int>.Filterable), true);
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<Employee, string>>(5);
        builder.AddAttribute(6, nameof(PropertyColumn<Employee, string>.Property), (Expression<Func<Employee, string>>)(x => x.Name));
        builder.AddAttribute(7, nameof(PropertyColumn<Employee, string>.Title), "Name");
        builder.AddAttribute(8, nameof(PropertyColumn<Employee, string>.Sortable), true);
        builder.AddAttribute(9, nameof(PropertyColumn<Employee, string>.Filterable), true);
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<Employee, string>>(10);
        builder.AddAttribute(11, nameof(PropertyColumn<Employee, string>.Property), (Expression<Func<Employee, string>>)(x => x.Department));
        builder.AddAttribute(12, nameof(PropertyColumn<Employee, string>.Title), "Department");
        builder.AddAttribute(13, nameof(PropertyColumn<Employee, string>.Groupable), true);
        builder.AddAttribute(14, nameof(PropertyColumn<Employee, string>.Filterable), true);
        builder.CloseComponent();
    }

    [Test]
    public void ContextMenu_ClosedByDefault()
    {
        Context.Render<VekPopoverProvider>();
        var comp = Context.Render<VekDataGrid<Employee>>(parameters => parameters
            .Add(p => p.Items, SampleEmployees)
            .Add(p => p.Columns, RenderDefaultColumns));

        comp.Instance.ContextMenuEnabled.Should().BeFalse();
        comp.FindAll(".vek-datagrid-context-menu").Should().BeEmpty();
    }

    [Test]
    public async Task HeaderContextMenuTemplate_RendersOnHeaderRightClick()
    {
        var provider = Context.Render<VekPopoverProvider>();
        var comp = Context.Render<VekDataGrid<Employee>>(parameters => parameters
            .Add(p => p.Items, SampleEmployees)
            .Add(p => p.Columns, RenderDefaultColumns)
            .Add(p => p.HeaderContextMenuTemplate, args => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-header-menu");
                builder.AddContent(2, $"Header:{args.Column?.Title}");
                builder.CloseElement();
            }));

        var header = comp.Find("th.vek-table-cell");
        await header.ContextMenuAsync(new MouseEventArgs { ClientX = 100, ClientY = 50, Button = 2 });

        await provider.WaitForAssertionAsync(() =>
        {
            var menu = provider.Find(".custom-header-menu");
            menu.TextContent.Should().Contain("Header:ID");
        });
    }

    [Test]
    public async Task CellContextMenuTemplate_RendersOnCellRightClick()
    {
        var provider = Context.Render<VekPopoverProvider>();
        var comp = Context.Render<VekDataGrid<Employee>>(parameters => parameters
            .Add(p => p.Items, SampleEmployees)
            .Add(p => p.Columns, RenderDefaultColumns)
            .Add(p => p.CellContextMenuTemplate, args => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-cell-menu");
                builder.AddContent(2, $"Cell:{args.Item?.Name}:{args.FormattedValue}");
                builder.CloseElement();
            }));

        var cell = comp.Find("tbody tr.vek-table-row td.vek-table-cell");
        await cell.ContextMenuAsync(new MouseEventArgs { ClientX = 150, ClientY = 200, Button = 2 });

        await provider.WaitForAssertionAsync(() =>
        {
            var menu = provider.Find(".custom-cell-menu");
            menu.TextContent.Should().Contain("Cell:Alice Johnson:1");
        });
    }

    [Test]
    public async Task RowContextMenuTemplate_RendersOnRowRightClick()
    {
        var provider = Context.Render<VekPopoverProvider>();
        var comp = Context.Render<VekDataGrid<Employee>>(parameters => parameters
            .Add(p => p.Items, SampleEmployees)
            .Add(p => p.Columns, RenderDefaultColumns)
            .Add(p => p.RowContextMenuTemplate, args => builder =>
            {
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "custom-row-menu");
                builder.AddContent(2, $"Row:{args.Item?.Name}");
                builder.CloseElement();
            }));

        var row = comp.Find("tbody tr.vek-table-row");
        await row.ContextMenuAsync(new MouseEventArgs { ClientX = 200, ClientY = 300, Button = 2 });

        await provider.WaitForAssertionAsync(() =>
        {
            var menu = provider.Find(".custom-row-menu");
            menu.TextContent.Should().Contain("Row:Alice Johnson");
        });
    }

    [Test]
    public async Task OnContextMenu_Callback_FiresWithCoordinatesAndPayload()
    {
        Context.Render<VekPopoverProvider>();
        DataGridContextMenuArgs<Employee>? capturedArgs = null;

        var comp = Context.Render<VekDataGrid<Employee>>(parameters => parameters
            .Add(p => p.Items, SampleEmployees)
            .Add(p => p.Columns, RenderDefaultColumns)
            .Add(p => p.ContextMenuEnabled, true)
            .Add(p => p.OnContextMenu, EventCallback.Factory.Create<DataGridContextMenuArgs<Employee>>(this, args =>
            {
                capturedArgs = args;
            })));

        var cell = comp.Find("tbody tr.vek-table-row td.vek-table-cell");
        await cell.ContextMenuAsync(new MouseEventArgs { ClientX = 123, ClientY = 456, PageX = 123, PageY = 456, Button = 2 });

        capturedArgs.Should().NotBeNull();
        capturedArgs!.Target.Should().Be(DataGridContextMenuTarget.Cell);
        capturedArgs.Item.Should().Be(SampleEmployees[0]);
        capturedArgs.RowIndex.Should().Be(0);
        capturedArgs.MouseEventArgs.ClientX.Should().Be(123);
        capturedArgs.MouseEventArgs.ClientY.Should().Be(456);
    }

    [Test]
    public async Task OnContextMenu_Handled_PreventsMenuDisplay()
    {
        var provider = Context.Render<VekPopoverProvider>();
        var comp = Context.Render<VekDataGrid<Employee>>(parameters => parameters
            .Add(p => p.Items, SampleEmployees)
            .Add(p => p.Columns, RenderDefaultColumns)
            .Add(p => p.ContextMenuEnabled, true)
            .Add(p => p.OnContextMenu, EventCallback.Factory.Create<DataGridContextMenuArgs<Employee>>(this, args =>
            {
                args.Handled = true;
            })));

        var cell = comp.Find("tbody tr.vek-table-row td.vek-table-cell");
        await cell.ContextMenuAsync(new MouseEventArgs { ClientX = 100, ClientY = 100, Button = 2 });

        provider.FindAll("div.vek-popover-open").Should().BeEmpty();
    }

    [Test]
    public async Task BuiltInContextMenu_RendersStandardDataActions()
    {
        var provider = Context.Render<VekPopoverProvider>();
        var comp = Context.Render<VekDataGrid<Employee>>(parameters => parameters
            .Add(p => p.Items, SampleEmployees)
            .Add(p => p.Columns, RenderDefaultColumns)
            .Add(p => p.ContextMenuEnabled, true));

        var cell = comp.Find("tbody tr.vek-table-row td.vek-table-cell");
        await cell.ContextMenuAsync(new MouseEventArgs { ClientX = 100, ClientY = 100, Button = 2 });

        await provider.WaitForAssertionAsync(() =>
        {
            var menuItems = provider.FindAll(".vek-menu-item");
            var itemTexts = menuItems.Select(m => m.TextContent).ToList();

            itemTexts.Should().Contain(text => text.Contains("Copy Cell"));
            itemTexts.Should().Contain(text => text.Contains("Sort Ascending"));
            itemTexts.Should().Contain(text => text.Contains("Sort Descending"));
            itemTexts.Should().Contain(text => text.Contains("AutoFit"));
        });
    }

    [Test]
    public async Task BuiltInAction_Sort_UpdatesSortingState()
    {
        Context.Render<VekPopoverProvider>();
        DataGridContextMenuArgs<Employee>? capturedArgs = null;

        var comp = Context.Render<VekDataGrid<Employee>>(parameters => parameters
            .Add(p => p.Items, SampleEmployees)
            .Add(p => p.Columns, RenderDefaultColumns)
            .Add(p => p.ContextMenuEnabled, true)
            .Add(p => p.OnContextMenu, EventCallback.Factory.Create<DataGridContextMenuArgs<Employee>>(this, args =>
            {
                capturedArgs = args;
            })));

        var cell = comp.Find("tbody tr.vek-table-row td.vek-table-cell");
        await cell.ContextMenuAsync(new MouseEventArgs { ClientX = 50, ClientY = 50, Button = 2 });

        capturedArgs.Should().NotBeNull();
        var propName = capturedArgs!.Column!.PropertyName!;
        await comp.InvokeAsync(() => capturedArgs!.SortAscendingAsync());

        comp.Instance.GetColumnSortDirection(propName).Should().Be(SortDirection.Ascending);

        await comp.InvokeAsync(() => capturedArgs!.SortDescendingAsync());
        comp.Instance.GetColumnSortDirection(propName).Should().Be(SortDirection.Descending);

        await comp.InvokeAsync(() => capturedArgs!.ClearSortAsync());
        comp.Instance.GetColumnSortDirection(propName).Should().Be(SortDirection.None);
    }

    [Test]
    public async Task BuiltInAction_FilterByCellValue_AppliesFilter()
    {
        Context.Render<VekPopoverProvider>();
        DataGridContextMenuArgs<Employee>? capturedArgs = null;

        var comp = Context.Render<VekDataGrid<Employee>>(parameters => parameters
            .Add(p => p.Items, SampleEmployees)
            .Add(p => p.Columns, RenderDefaultColumns)
            .Add(p => p.ContextMenuEnabled, true)
            .Add(p => p.OnContextMenu, EventCallback.Factory.Create<DataGridContextMenuArgs<Employee>>(this, args =>
            {
                capturedArgs = args;
            })));

        var cell = comp.Find("tbody tr.vek-table-row td.vek-table-cell");
        await cell.ContextMenuAsync(new MouseEventArgs { ClientX = 50, ClientY = 50, Button = 2 });

        capturedArgs.Should().NotBeNull();
        await comp.InvokeAsync(() => capturedArgs!.FilterByCellValueAsync());

        comp.Instance.FilterDefinitions.Should().HaveCount(1);
        comp.Instance.FilterDefinitions[0].Value.Should().Be(1);

        await comp.InvokeAsync(() => capturedArgs!.ClearFilterAsync());
        comp.Instance.FilterDefinitions.Should().BeEmpty();
    }

    [Test]
    public async Task BuiltInAction_PinColumn_ExecutesPinning()
    {
        Context.Render<VekPopoverProvider>();
        DataGridContextMenuArgs<Employee>? capturedArgs = null;

        var comp = Context.Render<VekDataGrid<Employee>>(parameters => parameters
            .Add(p => p.Items, SampleEmployees)
            .Add(p => p.Columns, RenderDefaultColumns)
            .Add(p => p.AllowColumnPinning, true)
            .Add(p => p.ContextMenuEnabled, true)
            .Add(p => p.OnContextMenu, EventCallback.Factory.Create<DataGridContextMenuArgs<Employee>>(this, args =>
            {
                capturedArgs = args;
            })));

        var cell = comp.Find("tbody tr.vek-table-row td.vek-table-cell");
        await cell.ContextMenuAsync(new MouseEventArgs { ClientX = 50, ClientY = 50, Button = 2 });

        capturedArgs.Should().NotBeNull();
        capturedArgs!.CanPin.Should().BeTrue();

        await comp.InvokeAsync(() => capturedArgs!.PinLeftAsync());
        capturedArgs!.Column!.EffectiveFixedStyle.Should().Be(DataGridFixedStyle.Left);

        await comp.InvokeAsync(() => capturedArgs!.PinRightAsync());
        capturedArgs!.Column!.EffectiveFixedStyle.Should().Be(DataGridFixedStyle.Right);

        await comp.InvokeAsync(() => capturedArgs!.UnpinAsync());
        capturedArgs!.Column!.EffectiveFixedStyle.Should().Be(DataGridFixedStyle.None);
    }
}
