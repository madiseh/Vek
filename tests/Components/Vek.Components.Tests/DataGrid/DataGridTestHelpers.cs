using Bunit;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

internal static class DataGridTestHelpers
{
    internal static readonly TestProduct[] Products =
    [
        new("Task Chair", 199.0m),
        new("Desk Lamp", 49.99m),
        new("Notebook Set", 12.9m)
    ];

    internal static IRenderedComponent<VekDataGrid<TestProduct>> RenderGrid(
        BunitContext context,
        int pageSize) => context.Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, Products)
            .Add(grid => grid.PageSize, pageSize)
            .AddChildContent(NameColumn()));

    internal static IRenderedComponent<VekDataGrid<TestProduct>> RenderPriceGrid(
        BunitContext context) => context.Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, Products)
            .AddChildContent(PriceColumn()));

    internal static IRenderedComponent<VekDataGrid<TestProduct>> RenderCustomPriceGrid(
        BunitContext context) => context.Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, Products)
            .AddChildContent(CustomPriceColumn()));

    internal static IRenderedComponent<VekDataGrid<TestProduct>> RenderTemplateGrid(
        BunitContext context) => context.Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, Products)
            .AddChildContent(TemplateProductColumn()));

    internal static string[] GetRowNames(IRenderedComponent<VekDataGrid<TestProduct>> component) =>
        component.FindAll("tbody tr")
            .Select(row => row.TextContent.Trim())
            .ToArray();

    private static RenderFragment NameColumn() => builder =>
    {
        builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
        builder.AddAttribute(
            1,
            nameof(PropertyColumn<TestProduct, string>.Property),
            (Expression<Func<TestProduct, string>>)(product => product.Name));
        builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, string>.Title), "Name");
        builder.CloseComponent();
    };

    private static RenderFragment PriceColumn() => builder =>
    {
        builder.OpenComponent<PropertyColumn<TestProduct, decimal?>>(0);
        builder.AddAttribute(
            1,
            nameof(PropertyColumn<TestProduct, decimal?>.Property),
            (Expression<Func<TestProduct, decimal?>>)(item => item.Price));
        builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, decimal?>.Title), "Price");
        builder.AddAttribute(3, nameof(PropertyColumn<TestProduct, decimal?>.Format), "0");
        builder.CloseComponent();
    };

    private static RenderFragment CustomPriceColumn() => builder =>
    {
        builder.OpenComponent<PropertyColumn<TestProduct, decimal?>>(0);
        builder.AddAttribute(
            1,
            nameof(PropertyColumn<TestProduct, decimal?>.Property),
            (Expression<Func<TestProduct, decimal?>>)(item => item.Price));
        builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, decimal?>.Title), "Price");
        builder.AddAttribute(
            3,
            nameof(PropertyColumn<TestProduct, decimal?>.CellTemplate),
            (RenderFragment<TestProduct>)(item => contentBuilder =>
                contentBuilder.AddContent(0, $"${item.Price:0.00}")));
        builder.CloseComponent();
    };

    private static RenderFragment TemplateProductColumn() => builder =>
    {
        builder.OpenComponent<TemplateColumn<TestProduct>>(0);
        builder.AddAttribute(1, nameof(TemplateColumn<TestProduct>.Title), "Product");
        builder.AddAttribute(
            2,
            nameof(TemplateColumn<TestProduct>.CellTemplate),
            (RenderFragment<TestProduct>)(item => contentBuilder =>
                contentBuilder.AddContent(0, $"Item: {item.Name}")));
        builder.CloseComponent();
    };
}

internal sealed record TestProduct(string Name, decimal? Price = null);
