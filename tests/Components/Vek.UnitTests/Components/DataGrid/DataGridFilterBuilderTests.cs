// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components.Rendering;
using NUnit.Framework;

namespace Vek.UnitTests.Components.DataGrid;

#nullable enable

[TestFixture]
public class DataGridFilterBuilderTests : BunitTest
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
        builder.AddAttribute(3, nameof(PropertyColumn<Person, string>.Filterable), true);
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<Person, string>>(4);
        builder.AddAttribute(5, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Role));
        builder.AddAttribute(6, nameof(PropertyColumn<Person, string>.Title), "Role");
        builder.AddAttribute(7, nameof(PropertyColumn<Person, string>.Filterable), true);
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<Person, string>>(8);
        builder.AddAttribute(9, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(x => x.Department));
        builder.AddAttribute(10, nameof(PropertyColumn<Person, string>.Title), "Department");
        builder.AddAttribute(11, nameof(PropertyColumn<Person, string>.Filterable), true);
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<Person, int>>(12);
        builder.AddAttribute(13, nameof(PropertyColumn<Person, int>.Property), (Expression<Func<Person, int>>)(x => x.Age));
        builder.AddAttribute(14, nameof(PropertyColumn<Person, int>.Title), "Age");
        builder.AddAttribute(15, nameof(PropertyColumn<Person, int>.Filterable), true);
        builder.CloseComponent();
    }

    [Test]
    public void DataGrid_FilterBuilder_ClosedByDefault()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Filterable, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        comp.Instance.IsFilterBuilderVisible.Should().BeFalse();
        comp.FindAll(".vek-datagrid-filter-builder-dialog").Should().BeEmpty();
        comp.FindAll(".vek-filter-builder").Should().BeEmpty();
    }

    [Test]
    public async Task DataGrid_ShowAndHideFilterBuilderAsync_ControlsVisibility()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Filterable, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        await comp.InvokeAsync(() => comp.Instance.ShowFilterBuilderAsync());

        comp.Instance.IsFilterBuilderVisible.Should().BeTrue();
        comp.FindAll(".vek-datagrid-filter-builder-dialog").Count.Should().Be(1);
        comp.FindAll(".vek-filter-builder").Count.Should().Be(1);

        await comp.InvokeAsync(() => comp.Instance.HideFilterBuilderAsync());

        comp.Instance.IsFilterBuilderVisible.Should().BeFalse();
        comp.FindAll(".vek-datagrid-filter-builder-dialog").Should().BeEmpty();
        comp.FindAll(".vek-filter-builder").Should().BeEmpty();
    }

    [Test]
    public async Task DataGrid_FilterBuilder_CancelButton_ClosesDialog()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Filterable, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        await comp.InvokeAsync(() => comp.Instance.ShowFilterBuilderAsync());
        comp.FindAll(".vek-datagrid-filter-builder-dialog").Count.Should().Be(1);

        var cancelBtn = comp.Find(".vek-filter-builder__cancel");
        await cancelBtn.ClickAsync();

        comp.Instance.IsFilterBuilderVisible.Should().BeFalse();
        comp.FindAll(".vek-datagrid-filter-builder-dialog").Should().BeEmpty();
    }

    [Test]
    public async Task DataGrid_FilterBuilder_CloseButton_ClosesDialog()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Filterable, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        await comp.InvokeAsync(() => comp.Instance.ShowFilterBuilderAsync());
        comp.FindAll(".vek-datagrid-filter-builder-dialog").Count.Should().Be(1);

        var closeBtn = comp.Find(".vek-filter-builder__close-button");
        await closeBtn.ClickAsync();

        comp.Instance.IsFilterBuilderVisible.Should().BeFalse();
        comp.FindAll(".vek-datagrid-filter-builder-dialog").Should().BeEmpty();
    }

    [Test]
    public async Task FilterBuilder_AddConditionAndAddGroup_BuildsNestedTree()
    {
        var gridComp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Filterable, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        var builderComp = Context.Render<VekFilterBuilder<Person>>(parameters => parameters
            .Add(p => p.DataGrid, gridComp.Instance)
            .Add(p => p.Title, "Custom Filter"));

        builderComp.Find(".vek-filter-builder__title").TextContent.Should().Be("Custom Filter");

        // Initially 1 condition
        builderComp.FindAll(".vek-filter-builder-condition").Count.Should().Be(1);

        // Add condition
        var addConditionBtn = builderComp.FindAll(".vek-filter-builder-group__header button")[0];
        await addConditionBtn.ClickAsync();
        builderComp.FindAll(".vek-filter-builder-condition").Count.Should().Be(2);

        // Add group
        var addGroupBtn = builderComp.FindAll(".vek-filter-builder-group__header button")[1];
        await addGroupBtn.ClickAsync();

        // One root group + one nested group
        builderComp.FindAll(".vek-filter-builder-group").Count.Should().Be(2);

        // Remove condition
        var removeConditionBtn = builderComp.Find(".vek-filter-builder-group__remove-condition");
        await removeConditionBtn.ClickAsync();
        builderComp.FindAll(".vek-filter-builder-condition").Count.Should().Be(2); // 1 in root, 1 in nested group

        // Remove group
        var removeGroupBtn = builderComp.Find(".vek-filter-builder-group__remove-group");
        await removeGroupBtn.ClickAsync();
        builderComp.FindAll(".vek-filter-builder-group").Count.Should().Be(1);
    }

    [Test]
    public async Task FilterBuilder_LogicalOperator_ToggleAndOr()
    {
        var gridComp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Filterable, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        var builderComp = Context.Render<VekFilterBuilder<Person>>(parameters => parameters
            .Add(p => p.DataGrid, gridComp.Instance));

        var logicSelect = builderComp.Find(".vek-filter-builder-group__logic-select");
        logicSelect.Change("Or");

        builderComp.Instance.ToFilterGroupNode().LogicalOperator.Should().Be(FilterGroupOperator.Or);
    }

    [Test]
    public async Task FilterBuilder_Validation_EmptyValue_ShowsValidationError()
    {
        var gridComp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Filterable, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        var builderComp = Context.Render<VekFilterBuilder<Person>>(parameters => parameters
            .Add(p => p.DataGrid, gridComp.Instance));

        var applyBtn = builderComp.Find(".vek-filter-builder__apply");
        await applyBtn.ClickAsync();

        var alert = builderComp.Find(".vek-filter-builder__validation");
        alert.TextContent.Should().Contain("Enter a value for");
    }

    [Test]
    public async Task FilterBuilder_Validation_InvalidType_ShowsValidationError()
    {
        var gridComp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Filterable, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        var builderComp = Context.Render<VekFilterBuilder<Person>>(parameters => parameters
            .Add(p => p.DataGrid, gridComp.Instance));

        // Column index 3 is Age (int)
        var colSelect = builderComp.Find(".vek-filter-builder-condition__field-select");
        colSelect.Change("3");

        var valInput = builderComp.Find(".vek-filter-builder-condition__value-input");
        valInput.Change("not-a-number");

        var applyBtn = builderComp.Find(".vek-filter-builder__apply");
        await applyBtn.ClickAsync();

        var alert = builderComp.Find(".vek-filter-builder__validation");
        alert.TextContent.Should().Contain("Enter a valid value for Age.");
    }

    [Test]
    public async Task DataGrid_FilterBuilder_ApplyFilter_FiltersGridRows()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Filterable, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        await comp.InvokeAsync(() => comp.Instance.ShowFilterBuilderAsync());

        // Field 2 is Department (string)
        var colSelect = comp.Find(".vek-filter-builder-condition__field-select");
        colSelect.Change("2");

        var opSelect = comp.Find(".vek-filter-builder-condition__operator-select");
        opSelect.Change("equals");

        var valInput = comp.Find(".vek-filter-builder-condition__value-input");
        valInput.Change("R&D");

        var applyBtn = comp.Find(".vek-filter-builder__apply");
        await applyBtn.ClickAsync();

        comp.Instance.IsFilterBuilderVisible.Should().BeFalse();
        comp.Instance.FilteredItems.Count().Should().Be(2);
        comp.Instance.FilteredItems.Select(x => x.Name).Should().BeEquivalentTo(["Alice Johnson", "Charlie Brown"]);
    }

    [Test]
    public async Task DataGrid_FilterBuilder_Clear_RemovesAppliedFilters()
    {
        var comp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Filterable, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        await comp.InvokeAsync(() => comp.Instance.ShowFilterBuilderAsync());

        var colSelect = comp.Find(".vek-filter-builder-condition__field-select");
        colSelect.Change("2");

        var valInput = comp.Find(".vek-filter-builder-condition__value-input");
        valInput.Change("Product");

        var applyBtn = comp.Find(".vek-filter-builder__apply");
        await applyBtn.ClickAsync();

        comp.Instance.FilteredItems.Count().Should().Be(1);

        // Open again and click Clear
        await comp.InvokeAsync(() => comp.Instance.ShowFilterBuilderAsync());
        var clearBtn = comp.Find(".vek-filter-builder__clear");
        await clearBtn.ClickAsync();

        comp.Instance.FilteredItems.Count().Should().Be(4);
    }

    [Test]
    public void FilterGroupNode_NestedLogic_EvaluatesComplexExpression()
    {
        var gridComp = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(p => p.Items, SamplePeople)
            .Add(p => p.Filterable, true)
            .Add(p => p.Columns, RenderDefaultColumns));

        var nameCol = gridComp.Instance.RenderedColumns[0];
        var deptCol = gridComp.Instance.RenderedColumns[2];
        var ageCol = gridComp.Instance.RenderedColumns[3];

        // Root: Department == "R&D" AND (Age > 35 OR Name contains "Alice")
        var root = new FilterGroupNode<Person>
        {
            LogicalOperator = FilterGroupOperator.And
        };

        root.Children.Add(new FilterNode<Person>
        {
            Column = deptCol,
            Operator = "equals",
            Value = "R&D"
        });

        var nested = new FilterGroupNode<Person>
        {
            LogicalOperator = FilterGroupOperator.Or
        };

        nested.Children.Add(new FilterNode<Person>
        {
            Column = ageCol,
            Operator = ">",
            Value = 35
        });

        nested.Children.Add(new FilterNode<Person>
        {
            Column = nameCol,
            Operator = "contains",
            Value = "Alice"
        });

        root.Children.Add(nested);

        var expr = root.ToExpression();
        expr.Should().NotBeNull();

        var func = root.GenerateFilterFunction();
        var matching = SamplePeople.Where(func).ToList();

        // Alice: R&D, Age 30 (not >35, but name contains Alice) -> Match
        // Charlie: R&D, Age 40 (>35) -> Match
        // David: Operations (not R&D) -> No match
        // Bob: Product -> No match
        matching.Count.Should().Be(2);
        matching.Select(x => x.Name).Should().BeEquivalentTo(["Alice Johnson", "Charlie Brown"]);
    }

    [Test]
    public void FilterGroupNode_EmptyChildren_ReturnsTrueForAll()
    {
        var group = new FilterGroupNode<Person>();
        var func = group.GenerateFilterFunction();

        SamplePeople.All(func).Should().BeTrue();
    }
}
