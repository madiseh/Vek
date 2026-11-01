namespace Vek.Core.DataGrid.Tests;

public sealed class HostOperationsTests
{
    /// <summary>Applies and removes filters through the shared host surface and resets paging before notifying the host.</summary>
    [Fact]
    public async Task FiltersUseTheSharedHostSurface()
    {
        var host = new Host();
        host.Engine.Page = 3;
        await host.ApplyFilterAsync("name", VekDataGridFilterOperator.Equals, "Ada");
        Assert.Equal(1, host.Engine.Page);
        Assert.Equal("Ada", Assert.Single(host.Engine.Filters).Value);
        Assert.Equal("Ada", Assert.Single(host.Engine.GetLocalData(["Ada", "Grace"]).Items));
        host.Engine.Page = 2;
        await host.RemoveFilterAsync("name");
        Assert.Equal(1, host.Engine.Page);
        Assert.Empty(host.Engine.Filters);
        Assert.Equal(2, host.Engine.GetLocalData(["Ada", "Grace"]).TotalItems);
    }

    /// <summary>Awaits a continuation after the mutation, keeping operation completion pending until the continuation completes.</summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContinuationObservesUpdatedStateAndIsAwaited(bool remove)
    {
        var host = new Host();
        if (remove) host.Engine.ApplyFilter("name", VekDataGridFilterOperator.Equals, "Ada");
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        host.AfterChange = () =>
        {
            if (remove) Assert.Empty(host.Engine.Filters);
            else Assert.Equal("Ada", Assert.Single(host.Engine.Filters).Value);
            calls++;
            return new ValueTask(completion.Task);
        };
        var operation = remove
            ? host.RemoveFilterAsync("name")
            : host.ApplyFilterAsync("name", VekDataGridFilterOperator.Equals, "Ada");
        Assert.False(operation.IsCompleted);
        completion.SetResult();
        await operation;
        Assert.Equal(1, calls);
    }

    /// <summary>Propagates a continuation failure without rolling back or reapplying a successful state mutation.</summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContinuationFailurePreservesTheCompletedMutation(bool remove)
    {
        var host = new Host();
        if (remove) host.Engine.ApplyFilter("name", VekDataGridFilterOperator.Equals, "Ada");
        var failure = new InvalidOperationException("continuation failed");
        host.AfterChange = () => ValueTask.FromException(failure);
        var actual = await Assert.ThrowsAsync<InvalidOperationException>(() => remove
            ? host.RemoveFilterAsync("name").AsTask()
            : host.ApplyFilterAsync("name", VekDataGridFilterOperator.Equals, "Ada").AsTask());
        Assert.Same(failure, actual);
        if (remove) Assert.Empty(host.Engine.Filters);
        else Assert.Equal("Ada", Assert.Single(host.Engine.Filters).Value);
    }

    /// <summary>Rejects invalid operations before invoking the host continuation.</summary>
    [Fact]
    public async Task InvalidFilterDoesNotInvokeTheContinuation()
    {
        var host = new Host();
        var called = false;
        host.AfterChange = () =>
        {
            called = true;
            return ValueTask.CompletedTask;
        };
        await Assert.ThrowsAsync<ArgumentException>(() => host.ApplyFilterAsync(
            " ", VekDataGridFilterOperator.Equals, "Ada").AsTask());
        Assert.False(called);
        Assert.Empty(host.Engine.Filters);
    }

    private sealed class Host : IVekDataGridHost<string, string>
    {
        public Func<ValueTask>? AfterChange { get; set; }
        public ValueTask AfterCoreChangeAsync() => AfterChange?.Invoke() ?? ValueTask.CompletedTask;
        public VekDataGridEngine<string, string> Engine { get; } = new([
            new VekDataGridColumn<string, string>("name", value => value)
        ]);
    }
}
