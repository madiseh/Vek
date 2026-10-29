using Bunit;
using Microsoft.AspNetCore.Components;
using System.Linq.Expressions;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

internal static class DataGridTestHelpers
{
    internal static readonly TestProduct[] Products =
    [
        new("Task Chair"),
        new("Desk Lamp"),
        new("Notebook Set")
    ];

    internal static IRenderedComponent<VekDataGrid<TestProduct>> RenderGrid(
        BunitContext context,
        int pageSize) => context.Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, Products)
            .Add(grid => grid.PageSize, pageSize)
            .AddChildContent(NameColumn()));

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
}

internal sealed record TestProduct(string Name);
