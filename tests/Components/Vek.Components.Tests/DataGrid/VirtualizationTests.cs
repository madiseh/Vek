using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class VirtualizationTests : BunitContext
{
    /// <summary>Verifies client virtualization gives the virtualizer the full filtered result set while rendering a bounded table window.</summary>
    [Fact]
    public void ClientVirtualizationRendersVisibleRange()
    {
        var products = Enumerable.Range(0, 500)
            .Select(index => new TestProduct($"Product {index:D3}"))
            .ToArray();
        var cut = Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, products)
            .Add(grid => grid.VirtualizeRows, true)
            .Add(grid => grid.VirtualItemSize, 36)
            .Add(grid => grid.VirtualOverscanCount, 5)
            .Add(grid => grid.VirtualMaxItemCount, 80)
            .AddChildContent(NameColumn()));

        var virtualizer = cut.FindComponent<Virtualize<TestProduct>>().Instance;

        Assert.Equal(500, virtualizer.Items!.Count);
        Assert.Equal(36, virtualizer.ItemSize);
        Assert.Equal(5, virtualizer.OverscanCount);
        Assert.Equal(80, virtualizer.MaxItemCount);
        Assert.True(cut.FindAll("tbody tr").Count < 500);
    }

    /// <summary>Verifies server virtualization forwards the requested range and current grid state to its data source.</summary>
    [Fact]
    public async Task ServerVirtualizationRequestsRequestedRange()
    {
        VekDataGridState<TestProduct>? receivedState = null;
        var receivedStart = -1;
        var receivedCount = -1;
        var receivedToken = CancellationToken.None;
        using var cancellation = new CancellationTokenSource();
        var products = Enumerable.Range(0, 20)
            .Select(index => new TestProduct($"Product {index:D2}"))
            .ToArray();
        var cut = Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.VirtualizeRows, true)
            .Add(grid => grid.ServerVirtualData, (state, start, count, token) =>
            {
                receivedState = state;
                receivedStart = start;
                receivedCount = count;
                receivedToken = token;
                return Task.FromResult(new VekDataGridData<TestProduct>(products.Skip(start).Take(count).ToArray(), products.Length));
            })
            .AddChildContent(NameColumn()));

        var provider = cut.FindComponent<Virtualize<TestProduct>>().Instance.ItemsProvider!;
        var result = await provider(new ItemsProviderRequest(7, 4, cancellation.Token));

        Assert.NotNull(receivedState);
        Assert.Equal(1, receivedState!.Page);
        Assert.Equal(7, receivedStart);
        Assert.Equal(4, receivedCount);
        Assert.Equal(cancellation.Token, receivedToken);
        Assert.Equal(20, result.TotalItemCount);
        Assert.Equal(new[] { "Product 07", "Product 08", "Product 09", "Product 10" }, result.Items.Select(item => item.Name));
    }

    /// <summary>Verifies the server virtual range is truncated when it reaches the end of the result set.</summary>
    [Fact]
    public async Task VirtualRangeClampsAtEnd()
    {
        var products = Enumerable.Range(0, 10)
            .Select(index => new TestProduct($"Product {index:D3}"))
            .ToArray();
        var cut = Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.VirtualizeRows, true)
            .Add(grid => grid.ServerVirtualData, (_, _, _, _) => Task.FromResult(
                new VekDataGridData<TestProduct>(products[2..6], 5)))
            .AddChildContent(NameColumn()));

        var provider = cut.FindComponent<Virtualize<TestProduct>>().Instance.ItemsProvider!;
        var result = await provider(new ItemsProviderRequest(2, 10, CancellationToken.None));

        Assert.Equal(5, result.TotalItemCount);
        Assert.Equal(new[] { "Product 002", "Product 003", "Product 004" }, result.Items.Select(item => item.Name));
    }

    /// <summary>Verifies virtualized rows reject grouping because group headers cannot preserve fixed row sizing.</summary>
    [Fact]
    public void VirtualizationRejectsUnsupportedGroupingCombination()
    {
        var exception = Assert.Throws<InvalidOperationException>(() => Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, DataGridTestHelpers.Products)
            .Add(grid => grid.VirtualizeRows, true)
            .Add(grid => grid.GroupBy, [new VekDataGridGroupDefinition("Category")])
            .AddChildContent(NameColumn())));

        Assert.Contains("cannot be combined with grouping", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static RenderFragment NameColumn() => builder =>
    {
        builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
        builder.AddAttribute(1, nameof(PropertyColumn<TestProduct, string>.Property),
            (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Name));
        builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, string>.Title), "Name");
        builder.CloseComponent();
    };
}
