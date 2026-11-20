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
using Microsoft.AspNetCore.Components.Rendering;
using NUnit.Framework;
using Vek.UnitTests;

namespace Vek.UnitTests.Components.DataGrid;

#nullable enable

[TestFixture]
public class DataGridAdvancedFilterRowTests : BunitTest
{
    public record TestItem(int Id, string Name, string? Department, double Salary, bool Active);

    private static readonly TestItem[] SampleItems =
    [
        new(1, "Alice Smith", "Engineering", 120000, true),
        new(2, "Bob Jones", "Marketing", 80000, true),
        new(3, "Charlie Brown", "Engineering", 140000, false),
        new(4, "Diana Prince", null, 95000, true),
        new(5, "Evan Wright", "Operations", 70000, false),
    ];

    private static void RenderColumns(RenderTreeBuilder builder)
    {
        builder.OpenComponent<PropertyColumn<TestItem, int>>(0);
        builder.AddAttribute(1, nameof(PropertyColumn<TestItem, int>.Property), (Expression<Func<TestItem, int>>)(x => x.Id));
        builder.AddAttribute(2, nameof(PropertyColumn<TestItem, int>.Title), "ID");
        builder.AddAttribute(3, nameof(PropertyColumn<TestItem, int>.AllowFiltering), false);
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<TestItem, string>>(4);
        builder.AddAttribute(5, nameof(PropertyColumn<TestItem, string>.Property), (Expression<Func<TestItem, string>>)(x => x.Name));
        builder.AddAttribute(6, nameof(PropertyColumn<TestItem, string>.Title), "Name");
        builder.AddAttribute(7, nameof(PropertyColumn<TestItem, string>.FilterOperator), DataGridFilterOperator.StartsWith);
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<TestItem, string?>>(8);
        builder.AddAttribute(9, nameof(PropertyColumn<TestItem, string?>.Property), (Expression<Func<TestItem, string?>>)(x => x.Department));
        builder.AddAttribute(10, nameof(PropertyColumn<TestItem, string?>.Title), "Department");
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<TestItem, double>>(11);
        builder.AddAttribute(12, nameof(PropertyColumn<TestItem, double>.Property), (Expression<Func<TestItem, double>>)(x => x.Salary));
        builder.AddAttribute(13, nameof(PropertyColumn<TestItem, double>.Title), "Salary");
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<TestItem, bool>>(14);
        builder.AddAttribute(15, nameof(PropertyColumn<TestItem, bool>.Property), (Expression<Func<TestItem, bool>>)(x => x.Active));
        builder.AddAttribute(16, nameof(PropertyColumn<TestItem, bool>.Title), "Active");
        builder.CloseComponent();
    }

    [Test]
    public void DataGrid_ShowFilterRow_FalseByDefault()
    {
        var comp = Context.Render<VekDataGrid<TestItem>>(parameters => parameters
            .Add(p => p.Items, SampleItems)
            .Add(p => p.Columns, RenderColumns));

        comp.Instance.ShowFilterRow.Should().BeFalse();
        comp.Instance.FilterRowInputDelay.Should().Be(300);
        comp.FindAll(".vek-datagrid-filter-row").Should().BeEmpty();
    }

    [Test]
    public void DataGrid_ShowFilterRow_True_RendersFilterRowAndCells()
    {
        var comp = Context.Render<VekDataGrid<TestItem>>(parameters => parameters
            .Add(p => p.Items, SampleItems)
            .Add(p => p.ShowFilterRow, true)
            .Add(p => p.Columns, RenderColumns));

        comp.FindAll(".vek-datagrid-filter-row").Should().HaveCount(1);
        var cells = comp.FindAll(".vek-datagrid-filter-cell");
        cells.Should().HaveCount(5);

        // ID column has AllowFiltering="false", so its cell should have no filter content
        var idCell = cells[0];
        idCell.QuerySelector(".vek-datagrid-filter-cell-content").Should().BeNull();

        // Name column has filtering allowed and StartsWith as operator
        var nameCell = cells[1];
        nameCell.QuerySelector(".vek-datagrid-filter-cell-content").Should().NotBeNull();
        nameCell.QuerySelector(".vek-datagrid-filter-operator-menu").Should().NotBeNull();
        nameCell.QuerySelector(".vek-datagrid-filter-input").Should().NotBeNull();
    }

    [Test]
    public async Task DataGrid_FilterRow_StringFiltering_FiltersRows()
    {
        var comp = Context.Render<VekDataGrid<TestItem>>(parameters => parameters
            .Add(p => p.Items, SampleItems)
            .Add(p => p.ShowFilterRow, true)
            .Add(p => p.FilterRowInputDelay, 0)
            .Add(p => p.Columns, RenderColumns));

        comp.Instance.FilteredItems.Should().HaveCount(5);

        var filterCells = comp.FindComponents<DataGridFilterCell<TestItem>>();
        var nameFilterCell = filterCells.First(c => c.Instance.Column.Title == "Name");

        // Apply filter "Alice"
        await comp.InvokeAsync(async () =>
        {
            var filterDef = comp.Instance.CreateFilterDefinitionInstance();
            filterDef.Column = nameFilterCell.Instance.Column;
            filterDef.Operator = nameof(DataGridFilterOperator.StartsWith);
            filterDef.Value = "Alice";
            await nameFilterCell.Instance.CallApplyFilterAsync(filterDef);
        });

        comp.Instance.FilteredItems.Should().HaveCount(1);
        comp.Instance.FilteredItems.First().Name.Should().Be("Alice Smith");

        // Clear filter
        await comp.InvokeAsync(async () =>
        {
            await nameFilterCell.Instance.CallClearFilterAsync();
        });

        comp.Instance.FilteredItems.Should().HaveCount(5);
    }

    [Test]
    public async Task DataGrid_FilterRow_BetweenFiltering_FiltersRange()
    {
        var comp = Context.Render<VekDataGrid<TestItem>>(parameters => parameters
            .Add(p => p.Items, SampleItems)
            .Add(p => p.ShowFilterRow, true)
            .Add(p => p.FilterRowInputDelay, 0)
            .Add(p => p.Columns, RenderColumns));

        var filterCells = comp.FindComponents<DataGridFilterCell<TestItem>>();
        var salaryCell = filterCells.First(c => c.Instance.Column.Title == "Salary");

        // Filter Salary between 75000 and 125000 (Bob: 80k, Diana: 95k, Alice: 120k)
        await comp.InvokeAsync(async () =>
        {
            var filterDef = comp.Instance.CreateFilterDefinitionInstance();
            filterDef.Column = salaryCell.Instance.Column;
            filterDef.Operator = nameof(DataGridFilterOperator.Between);
            filterDef.Value = new DataGridFilterRange(75000.0, 125000.0);
            await salaryCell.Instance.CallApplyFilterAsync(filterDef);
        });

        comp.Instance.FilteredItems.Should().HaveCount(3);
        var names = comp.Instance.FilteredItems.Select(x => x.Name).ToArray();
        names.Should().Contain("Alice Smith");
        names.Should().Contain("Bob Jones");
        names.Should().Contain("Diana Prince");
    }

    [Test]
    public async Task DataGrid_FilterRow_IsNullFiltering_FiltersNulls()
    {
        var comp = Context.Render<VekDataGrid<TestItem>>(parameters => parameters
            .Add(p => p.Items, SampleItems)
            .Add(p => p.ShowFilterRow, true)
            .Add(p => p.FilterRowInputDelay, 0)
            .Add(p => p.Columns, RenderColumns));

        var filterCells = comp.FindComponents<DataGridFilterCell<TestItem>>();
        var deptCell = filterCells.First(c => c.Instance.Column.Title == "Department");

        // Filter Department IsNull (Diana has null Department)
        await comp.InvokeAsync(async () =>
        {
            var filterDef = comp.Instance.CreateFilterDefinitionInstance();
            filterDef.Column = deptCell.Instance.Column;
            filterDef.Operator = nameof(DataGridFilterOperator.IsNull);
            filterDef.Value = null;
            await deptCell.Instance.CallApplyFilterAsync(filterDef);
        });

        comp.Instance.FilteredItems.Should().HaveCount(1);
        comp.Instance.FilteredItems.First().Name.Should().Be("Diana Prince");
    }

    [Test]
    public void DataGridFilterOperator_DisplayStringsAndSymbols_AreConsistent()
    {
        DataGridFilterOperator.Equals.ToDisplayString().Should().Be("Equals");
        DataGridFilterOperator.NotEquals.ToDisplayString().Should().Be("Does Not Equal");
        DataGridFilterOperator.Contains.ToDisplayString().Should().Be("Contains");
        DataGridFilterOperator.Between.ToDisplayString().Should().Be("Between");
        DataGridFilterOperator.IsNull.ToDisplayString().Should().Be("Is Null");
        DataGridFilterOperator.IsNotNull.ToDisplayString().Should().Be("Is Not Null");

        DataGridFilterOperator.Equals.ToSymbol().Should().Be("=");
        DataGridFilterOperator.NotEquals.ToSymbol().Should().Be("≠");
        DataGridFilterOperator.GreaterThan.ToSymbol().Should().Be(">");
        DataGridFilterOperator.LessThan.ToSymbol().Should().Be("<");
    }
}
