// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Linq.Expressions;
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
public class DataGridColumnChooserTests : BunitTest
{
    public record Person(string Name, string Role, string Department, int Age);

    private static readonly Person[] SamplePeople =
    [
        new("Alice Johnson", "Engineer", "R&D", 30),
        new("Bob Smith", "Designer", "Product", 25),
        new("Charlie Brown", "Manager", "R&D", 40),
        new("David Lee", "Engineer", "Operations", 35),
    ];

    private static void RenderDefaultColumns(RenderTreeBuilder builder)
    {
        builder.OpenComponent<PropertyColumn<Person, string>>(0);
        builder.AddAttribute(1, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Name));
        builder.AddAttribute(2, nameof(PropertyColumn<Person, string>.Title), "Name");
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<Person, string>>(3);
        builder.AddAttribute(4, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Role));
        builder.AddAttribute(5, nameof(PropertyColumn<Person, string>.Title), "Role");
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<Person, string>>(6);
        builder.AddAttribute(7, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Department));
        builder.AddAttribute(8, nameof(PropertyColumn<Person, string>.Title), "Department");
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<Person, int>>(9);
        builder.AddAttribute(10, nameof(PropertyColumn<Person, int>.Property), (Expression<Func<Person, int>>)(x => x.Age));
        builder.AddAttribute(11, nameof(PropertyColumn<Person, int>.Title), "Age");
        builder.CloseComponent();
    }

    [Test]
    public void DataGrid_ColumnChooser_ClosedByDefault()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Columns, RenderDefaultColumns));

        comp.Instance.IsColumnChooserVisible.Should().BeFalse();
        comp.Instance.ColumnChooserAnchorId.Should().BeNull();
        comp.FindAll(".vek-datagrid-column-chooser-dialog").Should().BeEmpty();
        comp.FindAll(".vek-column-chooser").Should().BeEmpty();
    }

    [Test]
    public async Task DataGrid_ShowAndHideColumnChooserAsync_ControlsVisibility()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Columns, RenderDefaultColumns));

        await comp.InvokeAsync(() => comp.Instance.ShowColumnChooserAsync("test-anchor"));

        comp.Instance.IsColumnChooserVisible.Should().BeTrue();
        comp.Instance.ColumnChooserAnchorId.Should().Be("test-anchor");
        comp.FindAll(".vek-datagrid-column-chooser-dialog").Count.Should().Be(1);
        comp.FindAll(".vek-column-chooser").Count.Should().Be(1);

        var items = comp.FindAll(".vek-column-chooser__item");
        items.Count.Should().Be(4);

        await comp.InvokeAsync(() => comp.Instance.HideColumnChooserAsync());

        comp.Instance.IsColumnChooserVisible.Should().BeFalse();
        comp.Instance.ColumnChooserAnchorId.Should().BeNull();
        comp.FindAll(".vek-datagrid-column-chooser-dialog").Should().BeEmpty();
    }

    [Test]
    public async Task DataGrid_ColumnChooser_CloseButton_ClosesChooser()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Columns, RenderDefaultColumns));

        await comp.InvokeAsync(() => comp.Instance.ShowColumnChooserAsync());
        comp.FindAll(".vek-datagrid-column-chooser-dialog").Count.Should().Be(1);

        var closeBtn = comp.Find(".vek-column-chooser__close-button");
        await closeBtn.ClickAsync();

        comp.Instance.IsColumnChooserVisible.Should().BeFalse();
        comp.FindAll(".vek-datagrid-column-chooser-dialog").Should().BeEmpty();
    }

    [Test]
    public async Task DataGrid_ColumnChooser_TogglingVisibility_UpdatesHiddenAndTableCells()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Columns, RenderDefaultColumns));

        comp.FindAll(".vek-table-head th").Count.Should().Be(4);

        await comp.InvokeAsync(() => comp.Instance.ShowColumnChooserAsync());

        var checkboxes = comp.FindComponents<VekCheckBox<bool>>();
        checkboxes.Count.Should().Be(4);

        var deptCheckbox = checkboxes.First(c => c.Instance.Label == "Department");
        deptCheckbox.Instance.Value.Should().BeTrue();

        await comp.InvokeAsync(async () =>
        {
            await deptCheckbox.Instance.ValueChanged.InvokeAsync(false);
        });

        var deptColumn = comp.Instance.RenderedColumns.First(c => c.Title == "Department");
        deptColumn.GetState(x => x.Hidden).Should().BeTrue();

        comp.FindAll(".vek-table-head th").Count.Should().Be(3);
        comp.FindAll(".vek-table-head th").Any(th => th.TextContent.Contains("Department")).Should().BeFalse();

        var updatedCheckboxes = comp.FindComponents<VekCheckBox<bool>>();
        var deptCheckboxUpdated = updatedCheckboxes.First(c => c.Instance.Label == "Department");
        deptCheckboxUpdated.Instance.Value.Should().BeFalse();

        await comp.InvokeAsync(async () =>
        {
            await deptCheckboxUpdated.Instance.ValueChanged.InvokeAsync(true);
        });

        deptColumn.GetState(x => x.Hidden).Should().BeFalse();
        comp.FindAll(".vek-table-head th").Count.Should().Be(4);
        comp.FindAll(".vek-table-head th").Any(th => th.TextContent.Contains("Department")).Should().BeTrue();
    }

    [Test]
    public async Task DataGrid_ColumnChooser_AllowHidingFalse_PreventsColumnConcealment()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Columns, builder =>
            {
                builder.OpenComponent<PropertyColumn<Person, string>>(0);
                builder.AddAttribute(1, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Name));
                builder.AddAttribute(2, nameof(PropertyColumn<Person, string>.Title), "Name");
                builder.AddAttribute(3, nameof(PropertyColumn<Person, string>.AllowHiding), false);
                builder.CloseComponent();

                builder.OpenComponent<PropertyColumn<Person, string>>(4);
                builder.AddAttribute(5, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Role));
                builder.AddAttribute(6, nameof(PropertyColumn<Person, string>.Title), "Role");
                builder.AddAttribute(7, nameof(PropertyColumn<Person, string>.AllowHiding), true);
                builder.CloseComponent();
            }));

        var nameColumn = comp.Instance.RenderedColumns[0];
        nameColumn.GetState(x => x.AllowHiding).Should().BeFalse();

        await comp.InvokeAsync(() => comp.Instance.ShowColumnChooserAsync());

        var checkboxes = comp.FindComponents<VekCheckBox<bool>>();
        var nameCheckbox = checkboxes.First(c => c.Instance.Label == "Name");
        nameCheckbox.Instance.Disabled.Should().BeTrue();

        await comp.InvokeAsync(() => nameColumn.HideAsync());
        nameColumn.GetState(x => x.Hidden).Should().BeFalse();

        await comp.InvokeAsync(() => nameColumn.ToggleAsync());
        nameColumn.GetState(x => x.Hidden).Should().BeFalse();

        var hideAllBtn = comp.Find(".vek-column-chooser__hide-all");
        await hideAllBtn.ClickAsync();

        nameColumn.GetState(x => x.Hidden).Should().BeFalse();
        var roleColumn = comp.Instance.RenderedColumns[1];
        roleColumn.GetState(x => x.Hidden).Should().BeTrue();

        comp.FindAll(".vek-table-head th").Count.Should().Be(1);
        comp.Find(".vek-table-head th").TextContent.Should().Contain("Name");
    }

    [Test]
    public async Task DataGrid_ColumnChooser_SearchFilter_FiltersColumnsList()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Columns, RenderDefaultColumns));

        await comp.InvokeAsync(() => comp.Instance.ShowColumnChooserAsync());

        comp.FindAll(".vek-column-chooser__item").Count.Should().Be(4);

        var searchInput = comp.FindComponent<VekTextField<string>>();
        await comp.InvokeAsync(() => searchInput.Instance.ValueChanged.InvokeAsync("Age"));

        var filteredItems = comp.FindAll(".vek-column-chooser__item");
        filteredItems.Count.Should().Be(1);
        filteredItems[0].TextContent.Should().Contain("Age");

        // Filter by letter 'm' (Name, Department contain 'm'; Role, Age do not)
        await comp.InvokeAsync(() => searchInput.Instance.ValueChanged.InvokeAsync("m"));
        var mItems = comp.FindAll(".vek-column-chooser__item");
        mItems.Count.Should().Be(2);

        // Filter by letter 'a' (Name, Department, Age contain 'a'; Role does not)
        await comp.InvokeAsync(() => searchInput.Instance.ValueChanged.InvokeAsync("a"));
        var aItems = comp.FindAll(".vek-column-chooser__item");
        aItems.Count.Should().Be(3);

        await comp.InvokeAsync(() => searchInput.Instance.ValueChanged.InvokeAsync(string.Empty));
        comp.FindAll(".vek-column-chooser__item").Count.Should().Be(4);
    }

    [Test]
    public async Task DataGrid_ColumnChooser_ShowAllAndHideAll_WorksCorrectly()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Columns, RenderDefaultColumns));

        await comp.InvokeAsync(() => comp.Instance.ShowColumnChooserAsync());

        var hideAllBtn = comp.Find(".vek-column-chooser__hide-all");
        await hideAllBtn.ClickAsync();

        comp.Instance.RenderedColumns.All(c => c.GetState(x => x.Hidden)).Should().BeTrue();
        comp.FindAll(".vek-table-head th").Count.Should().Be(0);

        var showAllBtn = comp.Find(".vek-column-chooser__show-all");
        await showAllBtn.ClickAsync();

        comp.Instance.RenderedColumns.All(c => !c.GetState(x => x.Hidden)).Should().BeTrue();
        comp.FindAll(".vek-table-head th").Count.Should().Be(4);
    }

    [Test]
    public async Task DataGrid_ColumnChooser_ReorderColumns_MovesColumnPositions()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Columns, RenderDefaultColumns));

        await comp.InvokeAsync(() => comp.Instance.ShowColumnChooserAsync());

        comp.Instance.RenderedColumns[0].Title.Should().Be("Name");
        comp.Instance.RenderedColumns[1].Title.Should().Be("Role");

        var downButtons = comp.FindAll(".vek-column-chooser__reorder-down");
        await downButtons[0].ClickAsync();

        comp.Instance.RenderedColumns[0].Title.Should().Be("Role");
        comp.Instance.RenderedColumns[1].Title.Should().Be("Name");

        var headers = comp.FindAll(".vek-table-head th");
        headers[0].TextContent.Should().Contain("Role");
        headers[1].TextContent.Should().Contain("Name");
    }

    [Test]
    public async Task DataGrid_ColumnChooserTemplate_RendersCustomUI()
    {
        DataGridColumnChooserContext<Person>? capturedContext = null;

        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Columns, RenderDefaultColumns)
            .Add(p => p.ColumnChooserTemplate, ctx => builder =>
            {
                capturedContext = ctx;
                builder.OpenElement(0, "div");
                builder.AddAttribute(1, "class", "my-custom-chooser");
                builder.AddContent(2, $"Columns: {ctx.Columns.Count}");

                builder.OpenElement(3, "button");
                builder.AddAttribute(4, "class", "custom-close-btn");
                builder.AddAttribute(5, "onclick", EventCallback.Factory.Create(this, () => ctx.CloseAsync()));
                builder.AddContent(6, "Custom Close");
                builder.CloseElement();

                builder.CloseElement();
            }));

        await comp.InvokeAsync(() => comp.Instance.ShowColumnChooserAsync("anchor-123"));

        comp.FindAll(".my-custom-chooser").Count.Should().Be(1);
        comp.Find(".my-custom-chooser").TextContent.Should().Contain("Columns: 4");

        capturedContext.Should().NotBeNull();
        capturedContext!.AnchorId.Should().Be("anchor-123");
        capturedContext.IsOpen.Should().BeTrue();
        capturedContext.Items.Count.Should().Be(4);

        var customClose = comp.Find(".custom-close-btn");
        await customClose.ClickAsync();

        comp.Instance.IsColumnChooserVisible.Should().BeFalse();
        comp.FindAll(".my-custom-chooser").Should().BeEmpty();
    }

    [Test]
    public async Task DataGridColumnChooser_StandaloneComponent_WorksWithGridReference()
    {
        var gridComp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Columns, RenderDefaultColumns));

        var chooserComp = Context.Render<DataGridColumnChooser<Person>>(parameters => parameters
            .Add(p => p.DataGrid, gridComp.Instance)
            .Add(p => p.Title, "Manage Columns"));

        chooserComp.Find(".vek-column-chooser__title").TextContent.Should().Be("Manage Columns");
        chooserComp.FindAll(".vek-column-chooser__item").Count.Should().Be(4);

        var hideAllBtn = chooserComp.Find(".vek-column-chooser__hide-all");
        await hideAllBtn.ClickAsync();

        gridComp.Instance.RenderedColumns.All(c => c.GetState(x => x.Hidden)).Should().BeTrue();
    }
}
