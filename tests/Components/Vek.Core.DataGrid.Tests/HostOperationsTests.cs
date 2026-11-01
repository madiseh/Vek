namespace Vek.Core.DataGrid.Tests;

public sealed class HostOperationsTests
{
    /// <summary>Sorting resets paging and preserves filters without invoking the filtering continuation.</summary>
    [Fact]
    public async Task SortResetsPageAndPreservesFilters()
    {
        var host = new Host { AfterChange = () => throw new InvalidOperationException("Unexpected filtering notification") };
        host.Engine.ApplyFilter("name", VekDataGridFilterOperator.Contains, "a");
        host.Engine.SetPage(3, 1);

        await host.SortAsync("name", VekDataGridSortMode.Single);

        Assert.Equal(1, host.Engine.Page);
        Assert.Equal(1, host.Engine.PageSize);
        Assert.Equal("a", Assert.Single(host.Engine.Filters).Value);
        Assert.Equal(VekSortDirection.Ascending, Assert.Single(host.Engine.Sorts).Direction);
        Assert.Equal("Ada", Assert.Single(host.Engine.GetLocalData(["Grace", "Ada"]).Items));
    }

    /// <summary>Disabled, non-sortable, and unknown columns leave the query unchanged and do not notify the host.</summary>
    [Theory]
    [InlineData(VekDataGridSortMode.None, "name", true)]
    [InlineData(VekDataGridSortMode.Single, "missing", true)]
    [InlineData(VekDataGridSortMode.Single, "name", false)]
    public async Task RejectedSortDoesNotResetPageOrNotify(VekDataGridSortMode mode, string key, bool sortable)
    {
        var host = new SortHost { AfterSortChange = () => throw new InvalidOperationException("Unexpected sorting notification") };
        host.Engine.AddOrUpdateColumn(new VekDataGridColumn<string, string>("name", value => value, sortable: sortable));
        host.Engine.SetPage(3, 1);
        await host.SortAsync(key, mode);
        Assert.Equal(3, host.Engine.Page);
        Assert.Empty(host.Engine.Sorts);
    }

    /// <summary>The sorting operation awaits its host continuation after descriptors and page state have changed.</summary>
    [Fact]
    public async Task SortAwaitsTheSortingContinuation()
    {
        var host = new SortHost();
        host.Engine.AddOrUpdateColumn(new VekDataGridColumn<string, string>("name", value => value));
        host.Engine.Page = 3;
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        host.AfterSortChange = () =>
        {
            Assert.Equal(1, host.Engine.Page);
            Assert.Equal(VekSortDirection.Ascending, Assert.Single(host.Engine.Sorts).Direction);
            calls++;
            return new ValueTask(completion.Task);
        };
        var operation = host.SortAsync("name", VekDataGridSortMode.Single);
        Assert.False(operation.IsCompleted);
        completion.SetResult();
        await operation;
        Assert.Equal(1, calls);
    }

    /// <summary>Clears sorting without changing filters, paging, or invoking the filtering continuation.</summary>
    [Fact]
    public async Task ClearSortPreservesTheOtherQueryState()
    {
        var host = new Host { AfterChange = () => throw new InvalidOperationException("Unexpected filtering notification") };
        host.Engine.SetSorts([new VekDataGridSort("name", VekSortDirection.Ascending)]);
        host.Engine.ApplyFilter("name", VekDataGridFilterOperator.Contains, "a");
        host.Engine.SetPage(2, 1);

        await host.ClearSortAsync();

        Assert.Empty(host.Engine.Sorts);
        Assert.Equal("a", Assert.Single(host.Engine.Filters).Value);
        Assert.Equal(2, host.Engine.Page);
        Assert.Equal(1, host.Engine.PageSize);
        Assert.Equal("Ada", Assert.Single(host.Engine.GetLocalData(["Grace", "Ada"]).Items));
    }

    /// <summary>Awaits the sort continuation after clearing descriptors and propagates failures without rollback.</summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ClearSortAwaitsItsOwnContinuation(bool fail)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var calls = 0;
        var host = new SortHost();
        host.Engine.SetSorts([new VekDataGridSort("name", VekSortDirection.Ascending)]);
        host.AfterSortChange = () =>
        {
            Assert.Empty(host.Engine.Sorts);
            calls++;
            return new ValueTask(completion.Task);
        };

        var operation = host.ClearSortAsync();
        Assert.False(operation.IsCompleted);
        if (fail)
        {
            var failure = new InvalidOperationException("sort continuation failed");
            completion.SetException(failure);
            Assert.Same(failure, await Assert.ThrowsAsync<InvalidOperationException>(() => operation.AsTask()));
        }
        else
        {
            completion.SetResult();
            await operation;
        }
        Assert.Equal(1, calls);
        Assert.Empty(host.Engine.Sorts);
    }

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

    private sealed class SortHost : IVekDataGridHost<string, string>
    {
        public VekDataGridEngine<string, string> Engine { get; } = new();
        public Func<ValueTask> AfterSortChange { get; set; } = () => ValueTask.CompletedTask;
        public ValueTask AfterCoreChangeAsync() => throw new InvalidOperationException("Unexpected filtering notification");
        public ValueTask AfterSortChangeAsync() => AfterSortChange();
    }
}
