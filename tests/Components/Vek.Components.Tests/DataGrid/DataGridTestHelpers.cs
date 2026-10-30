using Bunit;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using Vek.Blazor.DataGrid.Components.DataGrid;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

internal static class DataGridTestHelpers
{
    internal static readonly TestProduct[] Products =
    [
        new("Task Chair", 199.0m, "Furniture"),
        new("Desk Lamp", 49.99m, "Furniture"),
        new("Notebook Set", 12.9m, "Stationery")
    ];

    internal static IRenderedComponent<VekDataGrid<TestProduct>> RenderGrid(
        BunitContext context,
        int pageSize,
        VekDataGridSortMode sortMode = VekDataGridSortMode.Single) => context.Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, Products)
            .Add(grid => grid.PageSize, pageSize)
            .Add(grid => grid.SortMode, sortMode)
            .AddChildContent(NameColumn()));

    internal static IRenderedComponent<VekDataGrid<TestProduct>> RenderNameAndCategoryGrid(
        BunitContext context,
        VekDataGridSortMode sortMode) => context.Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, Products)
            .Add(grid => grid.PageSize, 3)
            .Add(grid => grid.SortMode, sortMode)
            .AddChildContent(NameAndCategoryColumns()));

    internal static IRenderedComponent<VekDataGrid<TestProduct>> RenderCustomSortGrid(
        BunitContext context) => context.Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, Products)
            .Add(grid => grid.PageSize, 3)
            .AddChildContent(CustomSortColumn()));

    internal static IRenderedComponent<VekDataGrid<TestProduct>> RenderQuickFilterGrid(
        BunitContext context,
        Func<TestProduct, bool> quickFilter) => context.Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, Products)
            .Add(grid => grid.PageSize, 3)
            .Add(grid => grid.QuickFilter, quickFilter)
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

    private static RenderFragment NameAndCategoryColumns() => builder =>
    {
        builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
        builder.AddAttribute(
            1,
            nameof(PropertyColumn<TestProduct, string>.Property),
            (Expression<Func<TestProduct, string>>)(product => product.Name));
        builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, string>.Title), "Name");
        builder.CloseComponent();
        builder.OpenComponent<PropertyColumn<TestProduct, string>>(3);
        builder.AddAttribute(
            4,
            nameof(PropertyColumn<TestProduct, string>.Property),
            (Expression<Func<TestProduct, string>>)(product => product.Category));
        builder.AddAttribute(5, nameof(PropertyColumn<TestProduct, string>.Title), "Category");
        builder.CloseComponent();
    };

    private static RenderFragment CustomSortColumn() => builder =>
    {
        builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
        builder.AddAttribute(
            1,
            nameof(PropertyColumn<TestProduct, string>.Property),
            (Expression<Func<TestProduct, string>>)(product => product.Name));
        builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, string>.Title), "Name");
        builder.AddAttribute(
            3,
            nameof(PropertyColumn<TestProduct, string>.SortBy),
            (Func<TestProduct, object?>)(product => product.Name.Length));
        builder.AddAttribute(
            4,
            nameof(PropertyColumn<TestProduct, string>.SortComparer),
            Comparer<object?>.Create((left, right) => ((int)right!).CompareTo((int)left!)));
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

internal sealed record TestProduct(string Name, decimal? Price = null, string Category = "");
