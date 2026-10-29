using Bunit;
using Microsoft.AspNetCore.Components;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class AggregationTests : BunitContext
{
    private static readonly TestProduct[] Products =
    [
        new("Desk Lamp", 49.99m, "Furniture"),
        new("Task Chair", 199m, "Furniture"),
        new("Notebook Set", 12.9m, "Stationery"),
        new("Gift Box", null, "Gifts")
    ];

    /// <summary>Verifies built-in sum, average, count, minimum, and maximum skip nullable values as defined.</summary>
    [Fact]
    public void BuiltInAggregatesProduceExpectedValues()
    {
        var cut = RenderAggregateGrid();

        var footer = cut.Find("tfoot").TextContent;
        Assert.Contains("Sum: 261.89", footer);
        Assert.Contains("Average: 87.30", footer);
        Assert.Contains("Count: 3", footer);
        Assert.Contains("Min: 12.90", footer);
        Assert.Contains("Max: 199.00", footer);
    }

    /// <summary>Verifies a custom aggregate receives the selected values and determines its result.</summary>
    [Fact]
    public void CustomAggregateUsesProvidedSelector()
    {
        var cut = RenderAggregateGrid(
        [
            new("Spread", "Price", VekDataGridAggregateType.Custom, values =>
                values.OfType<decimal>().Max() - values.OfType<decimal>().Min(), "0.00",
                Selector: product => product.Price)
        ]);

        Assert.Contains("Spread: 186.10", cut.Find("tfoot").TextContent);
    }

    /// <summary>Verifies a footer template receives the complete filtered item scope and computed results.</summary>
    [Fact]
    public void FooterTemplateReceivesCurrentItems()
    {
        VekDataGridFooterContext<TestProduct>? received = null;
        var cut = RenderAggregateGrid(footerTemplate: context => builder =>
        {
            received = context;
            builder.AddContent(0, $"Footer rows {context.Items.Count}");
        });

        Assert.Contains("Footer rows 4", cut.Find("tfoot").TextContent);
        Assert.NotNull(received);
        Assert.Equal(4, received!.Items.Count);
        Assert.Equal(261.89m, received.Aggregates.Single(result => result.Key == "Sum").Value);
    }

    /// <summary>Verifies an aggregate template receives its raw and formatted result values.</summary>
    [Fact]
    public void AggregateTemplateReceivesCalculatedValue()
    {
        VekDataGridAggregateContext<TestProduct>? received = null;
        var cut = RenderAggregateGrid(
        [
            new("Sum", "Price", VekDataGridAggregateType.Sum, Format: "0.00",
                Template: context => builder =>
                {
                    received = context;
                    builder.AddContent(0, $"Custom total {context.FormattedValue}");
                })
        ]);

        Assert.Contains("Custom total 261.89", cut.Find("tfoot").TextContent);
        Assert.Equal(261.89m, received!.Value);
    }

    /// <summary>Verifies each group template receives aggregate values computed only from that group.</summary>
    [Fact]
    public void GroupedRowsShowTheirOwnAggregates()
    {
        var cut = RenderAggregateGrid(
            groups: [new("Category")],
            groupTemplate: context => builder => builder.AddContent(0,
                $"{context.Value}: {context.Aggregates.Single(result => result.Key == "Sum").Value}"));

        Assert.Contains("Furniture: 248.99", cut.Markup);
        Assert.Contains("Stationery: 12.9", cut.Markup);
        Assert.DoesNotContain("Furniture: 261.89", cut.Markup);
    }

    private IRenderedComponent<VekDataGrid<TestProduct>> RenderAggregateGrid(
        IReadOnlyList<VekDataGridAggregateDefinition<TestProduct>>? aggregates = null,
        IReadOnlyList<VekDataGridGroupDefinition>? groups = null,
        RenderFragment<VekDataGridFooterContext<TestProduct>>? footerTemplate = null,
        RenderFragment<VekDataGridGroupContext<TestProduct>>? groupTemplate = null) => Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, Products)
            .Add(grid => grid.PageSize, 10)
            .Add(grid => grid.Aggregates, aggregates ?? BuiltInDefinitions())
            .Add(grid => grid.GroupBy, groups ?? Array.Empty<VekDataGridGroupDefinition>())
            .Add(grid => grid.FooterTemplate, footerTemplate)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
                builder.AddAttribute(1, nameof(PropertyColumn<TestProduct, string>.Property), (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Name));
                builder.CloseComponent();
                builder.OpenComponent<PropertyColumn<TestProduct, decimal?>>(2);
                builder.AddAttribute(3, nameof(PropertyColumn<TestProduct, decimal?>.Property), (System.Linq.Expressions.Expression<Func<TestProduct, decimal?>>)(product => product.Price));
                builder.AddAttribute(4, nameof(PropertyColumn<TestProduct, decimal?>.Key), "Price");
                builder.AddAttribute(5, nameof(PropertyColumn<TestProduct, decimal?>.GroupTemplate), groupTemplate);
                builder.CloseComponent();
                builder.OpenComponent<PropertyColumn<TestProduct, string>>(6);
                builder.AddAttribute(7, nameof(PropertyColumn<TestProduct, string>.Property), (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Category));
                builder.AddAttribute(8, nameof(PropertyColumn<TestProduct, string>.Key), "Category");
                builder.AddAttribute(9, nameof(PropertyColumn<TestProduct, string>.GroupTemplate), groupTemplate);
                builder.CloseComponent();
            }));

    private static IReadOnlyList<VekDataGridAggregateDefinition<TestProduct>> BuiltInDefinitions() =>
    [
        new("Sum", "Price", VekDataGridAggregateType.Sum, Format: "0.00"),
        new("Average", "Price", VekDataGridAggregateType.Average, Format: "0.00"),
        new("Count", "Price", VekDataGridAggregateType.Count),
        new("Min", "Price", VekDataGridAggregateType.Min, Format: "0.00"),
        new("Max", "Price", VekDataGridAggregateType.Max, Format: "0.00")
    ];
}
