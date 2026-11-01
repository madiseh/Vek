using Bunit;
using Microsoft.AspNetCore.Components;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class ServerDataTests : BunitContext
{
    /// <summary>Verifies server callbacks receive the current page, page size, sorts, and filters.</summary>
    [Fact]
    public void ServerDataReceivesCurrentState()
    {
        VekDataGridState<TestProduct>? received = null;
        var cut = RenderServerGrid((state, _) =>
        {
            received = state;
            return Task.FromResult(new VekDataGridData<TestProduct>(DataGridTestHelpers.Products, 4));
        });

        cut.Find(".pager button[aria-label='Next page']").Click();

        Assert.NotNull(received);
        Assert.Equal(2, received!.Page);
        Assert.Equal(1, received.PageSize);
        Assert.Empty(received.Filters);
    }

    /// <summary>Verifies the grid displays the returned page and uses the server total for pagination.</summary>
    [Fact]
    public void ServerDataReturnsPageAndTotalCount()
    {
        var cut = RenderServerGrid((_, _) => Task.FromResult(
            new VekDataGridData<TestProduct>([DataGridTestHelpers.Products[2]], 12)));

        Assert.Equal(new[] { "Notebook Set" }, DataGridTestHelpers.GetRowNames(cut));
        Assert.Contains("Page 1 of 12", cut.Markup);
    }

    /// <summary>Verifies starting a newer server request cancels the previous request token.</summary>
    [Fact]
    public void NewRequestCancelsPreviousRequest()
    {
        var requests = new List<CancellationToken>();
        var pending = new TaskCompletionSource<VekDataGridData<TestProduct>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var cut = RenderServerGrid((_, token) =>
        {
            requests.Add(token);
            return requests.Count == 1
                ? pending.Task
                : Task.FromResult(new VekDataGridData<TestProduct>(DataGridTestHelpers.Products, 3));
        });

        cut.Find("thead button").Click();

        Assert.Equal(2, requests.Count);
        Assert.True(requests[0].IsCancellationRequested);
        Assert.False(requests[1].IsCancellationRequested);
    }

    /// <summary>Verifies a late response cannot replace the data returned by a newer request.</summary>
    [Fact]
    public async Task StaleResponseCannotReplaceNewerData()
    {
        var first = new TaskCompletionSource<VekDataGridData<TestProduct>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var second = new TaskCompletionSource<VekDataGridData<TestProduct>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var requestCount = 0;
        var cut = RenderServerGrid((_, _) => ++requestCount == 1 ? first.Task : second.Task);

        cut.Find("thead button").Click();
        second.SetResult(new VekDataGridData<TestProduct>([DataGridTestHelpers.Products[1]], 3));
        cut.WaitForAssertion(() => Assert.Contains("Desk Lamp", cut.Find("tbody").TextContent));
        first.SetResult(new VekDataGridData<TestProduct>([DataGridTestHelpers.Products[0]], 3));
        await Task.Yield();

        Assert.Contains("Desk Lamp", cut.Find("tbody").TextContent);
        Assert.DoesNotContain("Task Chair", cut.Find("tbody").TextContent);
    }

    /// <summary>Verifies server errors can be rendered through the configured error template.</summary>
    [Fact]
    public void ServerFailureCanRenderErrorContent()
    {
        var cut = RenderServerGrid((_, _) => Task.FromException<VekDataGridData<TestProduct>>(
            new InvalidOperationException("Product service is unavailable.")),
            errorContent: exception => builder => builder.AddContent(0, $"Error: {exception.Message}"));

        cut.WaitForAssertion(() => Assert.Contains("Error: Product service is unavailable.", cut.Markup));
    }

    /// <summary>Verifies local and server item sources cannot be supplied simultaneously.</summary>
    [Fact]
    public void LocalAndServerSourcesCannotBeCombined()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            Render<VekDataGrid<TestProduct>>(parameters => parameters
                .Add(grid => grid.Items, DataGridTestHelpers.Products)
                .Add(grid => grid.ServerData, (state, _) => Task.FromResult(new VekDataGridData<TestProduct>([], 0)))));

        Assert.Contains("Items and ServerData are mutually exclusive", exception.Message);
    }

    private IRenderedComponent<VekDataGrid<TestProduct>> RenderServerGrid(
        Func<VekDataGridState<TestProduct>, CancellationToken, Task<VekDataGridData<TestProduct>>> serverData,
        RenderFragment<Exception>? errorContent = null) => Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.PageSize, 1)
            .Add(grid => grid.ServerData, serverData)
            .Add(grid => grid.ErrorContent, errorContent)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
                builder.AddAttribute(1, nameof(PropertyColumn<TestProduct, string>.Property), (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Name));
                builder.CloseComponent();
            }));
}
