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

namespace Vek.UnitTests.Components.DataGrid;

#nullable enable

[TestFixture]
public class DataGridCellTemplateSelectorTests : BunitTest
{
    private static readonly Position[] Positions =
    [
        new("ALP", 12),
        new("BRT", -3),
        new("CRN", 2),
    ];

    /// <summary>
    /// Selects the first matching template and then the fallback template for each row.
    /// </summary>
    [Test]
    public void Selector_RendersFirstMatchingTemplateAndFallback()
    {
        var selector = new DataGridCellTemplateSelector<Position>()
            .When(position => position.Change >= 10, Template("strong"))
            .When(position => position.Change < 0, Template("loss"))
            .Otherwise(Template("watch"))
            .Build();

        var grid = RenderGrid(selector);

        grid.FindAll("tbody td").Select(cell => cell.TextContent.Trim()).Should().Equal(
            "strong 12",
            "loss -3",
            "watch 2");
    }

    /// <summary>
    /// Uses the default cell value when no selector rule matches.
    /// </summary>
    [Test]
    public void Selector_UnmatchedItemUsesDefaultCellValue()
    {
        var selector = new DataGridCellTemplateSelector<Position>()
            .When(position => position.Change >= 10, Template("strong"))
            .Build();

        var grid = RenderGrid(selector);

        grid.FindAll("tbody td").Select(cell => cell.TextContent.Trim()).Should().Equal(
            "strong 12",
            "-3",
            "2");
    }

    /// <summary>
    /// Gives an explicit cell template precedence over the display selector.
    /// </summary>
    [Test]
    public void CellTemplate_TakesPrecedenceOverSelector()
    {
        var selector = new DataGridCellTemplateSelector<Position>()
            .Otherwise(Template("selected"))
            .Build();
        RenderFragment<CellContext<Position>> cellTemplate = context => builder =>
            builder.AddContent(0, $"fixed {context.Item.Change}");

        var grid = RenderGrid(selector, cellTemplate);

        grid.FindAll("tbody td").Select(cell => cell.TextContent.Trim()).Should().Equal(
            "fixed 12",
            "fixed -3",
            "fixed 2");
    }

    private IRenderedComponent<VekDataGrid<Position>> RenderGrid(
        Func<Position, RenderFragment<Position>?> selector,
        RenderFragment<CellContext<Position>>? cellTemplate = null)
    {
        return Context.Render<VekDataGrid<Position>>(parameters => parameters
            .Add(grid => grid.Items, Positions)
            .Add(grid => grid.Columns, builder => RenderChangeColumn(builder, selector, cellTemplate)));
    }

    private static void RenderChangeColumn(
        RenderTreeBuilder builder,
        Func<Position, RenderFragment<Position>?> selector,
        RenderFragment<CellContext<Position>>? cellTemplate)
    {
        builder.OpenComponent<PropertyColumn<Position, int>>(0);
        builder.AddAttribute(1, nameof(PropertyColumn<Position, int>.Property), (Expression<Func<Position, int>>)(position => position.Change));
        builder.AddAttribute(2, nameof(Column<Position>.CellDisplayTemplateSelector), selector);
        if (cellTemplate is not null)
        {
            builder.AddAttribute(3, nameof(Column<Position>.CellTemplate), cellTemplate);
        }

        builder.CloseComponent();
    }

    private static RenderFragment<Position> Template(string label) => position => builder =>
        builder.AddContent(0, $"{label} {position.Change}");

    private sealed record Position(string Symbol, int Change);
}
