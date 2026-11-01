namespace Vek.Core.DataGrid.Tests;

public sealed class HostOperationsTests
{
    /// <summary>Applies and clears filters on an ordinary host without requiring any notification implementation.</summary>
    [Fact]
    public async Task FiltersWorkWithoutAHostHook()
    {
        var host = new Host();
        host.Engine.Page = 3;
        await host.ApplyFilterAsync("name", VekDataGridFilterOperator.Equals, "Ada");
        Assert.Equal(1, host.Engine.Page);
        Assert.Equal("Ada", Assert.Single(host.Engine.Filters).Value);
        Assert.Equal("Ada", Assert.Single(host.Engine.GetLocalData(["Ada", "Grace"]).Items));
        await host.ClearFilterAsync();
        Assert.Empty(host.Engine.Filters);
        Assert.Equal(2, host.Engine.GetLocalData(["Ada", "Grace"]).TotalItems);
    }

    /// <summary>Awaits a continuation after the mutation, keeping operation completion pending until the continuation completes.</summary>
    [Fact]
    public async Task ContinuationObservesUpdatedStateAndIsAwaited()
    {
        var host = new Host();
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        var operation = host.ApplyFilterAsync("name", VekDataGridFilterOperator.Equals, "Ada", afterApplied: () =>
        {
            Assert.Equal("Ada", Assert.Single(host.Engine.Filters).Value);
            calls++;
            return new ValueTask(completion.Task);
        });
        Assert.False(operation.IsCompleted);
        completion.SetResult();
        await operation;
        Assert.Equal(1, calls);
    }

    /// <summary>Propagates a continuation failure without rolling back or reapplying a successful state mutation.</summary>
    [Fact]
    public async Task ContinuationFailurePreservesTheCompletedMutation()
    {
        var host = new Host();
        var failure = new InvalidOperationException("continuation failed");
        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => host.ApplyFilterAsync(
            "name", VekDataGridFilterOperator.Equals, "Ada",
            afterApplied: () => ValueTask.FromException(failure)));
        Assert.Same(failure, actual);
        Assert.Equal("Ada", Assert.Single(host.Engine.Filters).Value);
    }

    /// <summary>Rejects invalid operations before invoking an optional continuation.</summary>
    [Fact]
    public async Task InvalidFilterDoesNotInvokeTheContinuation()
    {
        var host = new Host();
        var called = false;
        await Assert.ThrowsAsync<ArgumentException>(() => host.ApplyFilterAsync(
            " ", VekDataGridFilterOperator.Equals, "Ada", afterApplied: () =>
            {
                called = true;
                return ValueTask.CompletedTask;
            }));
        Assert.False(called);
        Assert.Empty(host.Engine.Filters);
    }

    private sealed class Host : IVekDataGridHost<string, string>
    {
        public VekDataGridEngine<string, string> Engine { get; } = new([
            new VekDataGridColumn<string, string>("name", value => value)
        ]);
    }
}
