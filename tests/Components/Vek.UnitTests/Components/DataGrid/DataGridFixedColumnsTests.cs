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
using NUnit.Framework;
using Vek.Extensions;

namespace Vek.UnitTests.Components.DataGrid;

#nullable enable

[TestFixture]
public class DataGridFixedColumnsTests : BunitTest
{
    public record TestModel(int Id, string Name, string Department, string Role, decimal Salary);

    private static readonly TestModel[] SampleData =
    [
        new(1, "Alice Johnson", "Engineering", "Developer", 90000m),
        new(2, "Bob Smith", "Marketing", "Designer", 75000m),
        new(3, "Charlie Brown", "Finance", "Analyst", 85000m),
    ];

    [Test]
    public void FixedStyle_None_ShouldNotHaveStickyClassesOrOffsets()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.HorizontalScrollbar, true)
            .Add(p => p.Columns, builder =>
            {
                RenderColumn(builder, 0, x => x.Id, "ID", fixedStyle: DataGridFixedStyle.None);
                RenderColumn(builder, 1, x => x.Name, "Name", fixedStyle: DataGridFixedStyle.None);
                RenderColumn(builder, 2, x => x.Department, "Dept", fixedStyle: DataGridFixedStyle.None);
            }));

        var headers = comp.FindAll("th.vek-table-cell");
        headers.Should().NotBeEmpty();
        foreach (var th in headers)
        {
            th.ClassList.Should().NotContain("sticky-left");
            th.ClassList.Should().NotContain("sticky-right");
            th.ClassList.Should().NotContain("sticky-left-last");
            th.ClassList.Should().NotContain("sticky-right-first");
        }
    }

    [Test]
    public void FixedStyle_Left_SingleColumn_ShouldHaveStickyLeftAndLastClass()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.HorizontalScrollbar, true)
            .Add(p => p.Columns, builder =>
            {
                RenderColumn(builder, 0, x => x.Id, "ID", width: "100px", fixedStyle: DataGridFixedStyle.Left);
                RenderColumn(builder, 1, x => x.Name, "Name");
                RenderColumn(builder, 2, x => x.Department, "Dept");
            }));

        var headers = comp.FindAll("th.vek-table-cell");
        var firstHeader = headers[0];
        firstHeader.ClassList.Should().Contain("sticky-left");
        firstHeader.ClassList.Should().Contain("sticky-left-last");
        firstHeader.GetAttribute("style").Should().Contain("left:0px");

        var secondHeader = headers[1];
        secondHeader.ClassList.Should().NotContain("sticky-left");
        secondHeader.ClassList.Should().NotContain("sticky-left-last");
    }

    [Test]
    public void FixedStyle_Left_MultipleColumns_ShouldCalculateDynamicOffsets()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.HorizontalScrollbar, true)
            .Add(p => p.Columns, builder =>
            {
                RenderColumn(builder, 0, x => x.Id, "ID", width: "100px", fixedStyle: DataGridFixedStyle.Left);
                RenderColumn(builder, 1, x => x.Name, "Name", width: "150px", fixedStyle: DataGridFixedStyle.Left);
                RenderColumn(builder, 2, x => x.Department, "Dept", width: "200px");
            }));

        var headers = comp.FindAll("th.vek-table-cell");
        var firstHeader = headers[0];
        firstHeader.ClassList.Should().Contain("sticky-left");
        firstHeader.ClassList.Should().NotContain("sticky-left-last");
        firstHeader.GetAttribute("style").Should().Contain("left:0px");

        var secondHeader = headers[1];
        secondHeader.ClassList.Should().Contain("sticky-left");
        secondHeader.ClassList.Should().Contain("sticky-left-last");
        secondHeader.GetAttribute("style").Should().Contain("left:100px");

        var thirdHeader = headers[2];
        thirdHeader.ClassList.Should().NotContain("sticky-left");
        thirdHeader.ClassList.Should().NotContain("sticky-left-last");
    }

    [Test]
    public void FixedStyle_Right_MultipleColumns_ShouldCalculateDynamicOffsets()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.HorizontalScrollbar, true)
            .Add(p => p.Columns, builder =>
            {
                RenderColumn(builder, 0, x => x.Id, "ID", width: "100px");
                RenderColumn(builder, 1, x => x.Name, "Name", width: "150px");
                RenderColumn(builder, 2, x => x.Department, "Dept", width: "120px", fixedStyle: DataGridFixedStyle.Right);
                RenderColumn(builder, 3, x => x.Salary, "Salary", width: "80px", fixedStyle: DataGridFixedStyle.Right);
            }));

        var headers = comp.FindAll("th.vek-table-cell");
        var deptHeader = headers[2];
        deptHeader.ClassList.Should().Contain("sticky-right");
        deptHeader.ClassList.Should().Contain("sticky-right-first");
        deptHeader.GetAttribute("style").Should().Contain("right:80px");

        var salaryHeader = headers[3];
        salaryHeader.ClassList.Should().Contain("sticky-right");
        salaryHeader.ClassList.Should().NotContain("sticky-right-first");
        salaryHeader.GetAttribute("style").Should().Contain("right:0px");
    }

    [Test]
    public void BackwardCompatibility_StickyLeftAndRight_ShouldMapToFixedStyle()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.HorizontalScrollbar, true)
            .Add(p => p.Columns, builder =>
            {
                RenderLegacyColumn(builder, 0, x => x.Id, "ID", stickyLeft: true);
                RenderLegacyColumn(builder, 1, x => x.Name, "Name");
                RenderLegacyColumn(builder, 2, x => x.Department, "Dept", stickyRight: true);
            }));

        var grid = comp.Instance;
        grid.RenderedColumns[0].EffectiveFixedStyle.Should().Be(DataGridFixedStyle.Left);
        grid.RenderedColumns[0].IsPinnedLeft.Should().BeTrue();

        grid.RenderedColumns[1].EffectiveFixedStyle.Should().Be(DataGridFixedStyle.None);

        grid.RenderedColumns[2].EffectiveFixedStyle.Should().Be(DataGridFixedStyle.Right);
        grid.RenderedColumns[2].IsPinnedRight.Should().BeTrue();
    }

    [Test]
    public async Task PinColumnAsync_ShouldUpdateStateAndOffsets()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.HorizontalScrollbar, true)
            .Add(p => p.Columns, builder =>
            {
                RenderColumn(builder, 0, x => x.Id, "ID", width: "80px");
                RenderColumn(builder, 1, x => x.Name, "Name", width: "160px");
                RenderColumn(builder, 2, x => x.Department, "Dept", width: "140px");
            }));

        var grid = comp.Instance;
        var nameCol = grid.RenderedColumns[1];

        await comp.InvokeAsync(() => grid.PinColumnAsync(nameCol, DataGridFixedStyle.Left));

        nameCol.EffectiveFixedStyle.Should().Be(DataGridFixedStyle.Left);
        nameCol.IsPinnedLeft.Should().BeTrue();
        nameCol.StickyLeftOffset.Should().Be(0);

        // Unpin
        await comp.InvokeAsync(() => grid.PinColumnAsync(nameCol, DataGridFixedStyle.None));
        nameCol.EffectiveFixedStyle.Should().Be(DataGridFixedStyle.None);
        nameCol.IsPinnedLeft.Should().BeFalse();
        nameCol.StickyLeftOffset.Should().BeNull();
    }

    [Test]
    public async Task ColumnPinChanged_Callback_ShouldFireOnPinning()
    {
        DataGridColumnPinChangedEventArgs<TestModel>? eventArgs = null;

        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.HorizontalScrollbar, true)
            .Add(p => p.ColumnPinChanged, EventCallback.Factory.Create<DataGridColumnPinChangedEventArgs<TestModel>>(this, args => eventArgs = args))
            .Add(p => p.Columns, builder =>
            {
                RenderColumn(builder, 0, x => x.Id, "ID", width: "80px");
                RenderColumn(builder, 1, x => x.Name, "Name", width: "160px");
            }));

        var grid = comp.Instance;
        var idCol = grid.RenderedColumns[0];

        await comp.InvokeAsync(() => grid.PinColumnAsync(idCol, DataGridFixedStyle.Left));

        eventArgs.Should().NotBeNull();
        eventArgs!.Column.Should().BeSameAs(idCol);
        eventArgs.FixedStyle.Should().Be(DataGridFixedStyle.Left);
    }

    [Test]
    public void ContextMenu_PinningActions_RenderedWhenPinnable()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.AllowColumnPinning, true)
            .Add(p => p.Columns, builder =>
            {
                RenderColumn(builder, 0, x => x.Id, "ID", width: "80px", fixedStyle: DataGridFixedStyle.None);
            }));

        var headerCell = comp.FindComponent<HeaderCell<TestModel>>();
        headerCell.Should().NotBeNull();

        var menu = comp.FindComponent<VekMenu>();
        menu.Should().NotBeNull();
    }

    [Test]
    public void AllowPinning_False_DisablesPinning()
    {
        var comp = Context.Render<VekDataGrid<TestModel>>(parameters => parameters
            .Add(p => p.Items, SampleData)
            .Add(p => p.AllowColumnPinning, false)
            .Add(p => p.Columns, builder =>
            {
                RenderColumn(builder, 0, x => x.Id, "ID", width: "80px", allowPinning: false);
            }));

        var grid = comp.Instance;
        grid.RenderedColumns[0].AllowPinning.Should().BeFalse();
    }

    private static void RenderColumn<TProp>(
        RenderTreeBuilder builder,
        int seq,
        Expression<Func<TestModel, TProp>> property,
        string title,
        string? width = null,
        DataGridFixedStyle fixedStyle = DataGridFixedStyle.None,
        bool? allowPinning = null)
    {
        builder.OpenComponent<PropertyColumn<TestModel, TProp>>(seq * 10);
        builder.AddAttribute((seq * 10) + 1, nameof(PropertyColumn<TestModel, TProp>.Property), property);
        builder.AddAttribute((seq * 10) + 2, nameof(PropertyColumn<TestModel, TProp>.Title), title);
        if (width != null)
        {
            builder.AddAttribute((seq * 10) + 3, nameof(PropertyColumn<TestModel, TProp>.Width), width);
        }
        if (fixedStyle != DataGridFixedStyle.None)
        {
            builder.AddAttribute((seq * 10) + 4, nameof(PropertyColumn<TestModel, TProp>.FixedStyle), fixedStyle);
        }
        if (allowPinning.HasValue)
        {
            builder.AddAttribute((seq * 10) + 5, nameof(PropertyColumn<TestModel, TProp>.AllowPinning), allowPinning.Value);
        }
        builder.CloseComponent();
    }

    private static void RenderLegacyColumn<TProp>(
        RenderTreeBuilder builder,
        int seq,
        Expression<Func<TestModel, TProp>> property,
        string title,
        bool stickyLeft = false,
        bool stickyRight = false)
    {
        builder.OpenComponent<PropertyColumn<TestModel, TProp>>(seq * 10);
        builder.AddAttribute((seq * 10) + 1, nameof(PropertyColumn<TestModel, TProp>.Property), property);
        builder.AddAttribute((seq * 10) + 2, nameof(PropertyColumn<TestModel, TProp>.Title), title);
        if (stickyLeft)
        {
            builder.AddAttribute((seq * 10) + 3, nameof(PropertyColumn<TestModel, TProp>.StickyLeft), true);
        }
        if (stickyRight)
        {
            builder.AddAttribute((seq * 10) + 4, nameof(PropertyColumn<TestModel, TProp>.StickyRight), true);
        }
        builder.CloseComponent();
    }
}
