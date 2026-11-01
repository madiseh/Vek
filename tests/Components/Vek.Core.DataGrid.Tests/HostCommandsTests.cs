// Copyright (c) 2026 AMIN SABAGHI
// Licensed under the MIT License. See the LICENSE file in the project root for more information.
// Created: 2026-11-01

namespace Vek.Core.DataGrid.Tests;

public sealed class HostCommandsTests
{
    /// <summary>Filtering applies the shared keep-or-clear selection policy independently of any presentation callback.</summary>
    [Theory]
    [InlineData(VekDataGridFilteredSelectionMode.Keep, 1)]
    [InlineData(VekDataGridFilteredSelectionMode.Clear, 0)]
    public async Task FilteringReconcilesSelectionInCore(VekDataGridFilteredSelectionMode mode, int expectedCount)
    {
        var host = new Host();
        var state = host.Engine.Interaction;
        state.FilteredSelectionMode = mode;
        state.SelectedItems = [state.Items.ElementAt(1)];
        state.SelectedItem = state.SelectedItems[0];
        await host.ApplyFilterAsync("name", VekDataGridFilterOperator.Equals, "A");
        Assert.Equal(expectedCount, state.SelectedItems.Count);
    }

    /// <summary>Page commands clamp to available results and page-size changes restart at the first page.</summary>
    [Fact]
    public async Task PagingClampsAndResets()
    {
        var host = new Host();
        host.Engine.SetPage(1, 1);
        await host.SetCurrentPageAsync(100);
        Assert.Equal(3, host.Engine.Page);
        await host.SetPageSizeAsync(2);
        Assert.Equal(1, host.Engine.Page);
        Assert.Equal(2, host.Engine.PageSize);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => host.SetPageSizeAsync(0));
    }

    /// <summary>Column commands preserve identity, clamp widths, and apply visibility through the canonical contract.</summary>
    [Fact]
    public async Task ColumnsRetainIdentityAndEnforceBounds()
    {
        var host = new Host();
        var column = host.Engine.Columns.First();
        await host.ResizeColumnAsync("name", 1);
        Assert.Equal(column.MinWidth, column.Width);
        await host.SetColumnVisibilityAsync("name", false);
        Assert.False(column.Visible);
        await host.ReorderColumnAsync("name", 1);
        Assert.Same(column, host.Engine.Columns.Last());
        await Assert.ThrowsAsync<ArgumentException>(() => host.ReorderColumnAsync("missing", 1));
    }

    /// <summary>Select-all honors eligibility and page scope while preserving selections outside the current page.</summary>
    [Fact]
    public async Task SelectionHonorsEligibilityAndPageScope()
    {
        var host = new Host();
        var state = host.Engine.Interaction;
        state.SelectionMode = VekDataGridSelectionMode.Multiple;
        state.IsRowSelectable = row => row.Name != "B";
        state.VisibleItems = state.Items.Take(2).ToArray();
        await host.SelectAllAsync(true);
        Assert.Equal("A", Assert.Single(state.SelectedItems).Name);
        await host.SelectItemAsync(state.Items.Last());
        Assert.Equal(2, state.SelectedItems.Count);
        await host.SelectAllAsync(false);
        Assert.Equal("C", Assert.Single(state.SelectedItems).Name);
    }

    /// <summary>Single-row expansion and collapse share one state store and reconcile refreshed row instances.</summary>
    [Fact]
    public async Task ExpansionReconcilesAndHonorsSingleRowMode()
    {
        var host = new Host();
        var state = host.Engine.Interaction;
        state.SingleExpandedRow = true;
        await host.ExpandAllAsync();
        Assert.Equal("A", Assert.Single(state.ExpandedItems).Name);
        await host.ToggleRowDetailsAsync(state.Items.Last());
        Assert.Equal("C", Assert.Single(state.ExpandedItems).Name);
        state.DetailsComparer = EqualityComparer<Row>.Create((left, right) => left?.Name == right?.Name, row => row.Name.GetHashCode());
        var replacement = new Row("C");
        state.ReconcileExpandedItems([replacement]);
        Assert.Same(replacement, Assert.Single(state.ExpandedItems));
        await host.CollapseAllAsync();
        Assert.Empty(state.ExpandedItems);
    }

    /// <summary>Rejected drafts stay active, accepted drafts are isolated, and cancellation clears the same edit session.</summary>
    [Fact]
    public async Task EditingRetainsRejectedDraftAndClearsAcceptedSession()
    {
        var host = new Host();
        host.Engine.Interaction.EditMode = VekDataGridEditMode.Row;
        host.Engine.Interaction.CloneItem = row => new Row(row.Name);
        var row = host.Engine.Interaction.Items.First();
        await host.EditItemAsync(row);
        Assert.True(host.Engine.IsEditing);
        Assert.NotSame(row, host.Engine.DraftEditItem);
        host.Engine.DraftEditItem!.Name = "edited";
        Assert.Equal("A", row.Name);
        host.Valid = false;
        Assert.False(await host.CommitEditAsync());
        Assert.True(host.Engine.IsEditing);
        host.Valid = true;
        Assert.True(await host.CommitEditAsync());
        Assert.False(host.Engine.IsEditing);
        await host.EditItemAsync(row);
        await host.CancelEditAsync();
        Assert.False(host.Engine.IsEditing);
    }

    private sealed class Row(string name)
    {
        public string Name { get; set; } = name;
    }

    private sealed class Host : IVekDataGridHost<Row, string>
    {
        public Host()
        {
            Engine.Interaction.Items = [new Row("A"), new Row("B"), new Row("C")];
            Engine.Interaction.FilteredItems = Engine.Interaction.Items.ToArray();
            Engine.Interaction.VisibleItems = Engine.Interaction.FilteredItems;
        }
        public VekDataGridEngine<Row, string> Engine { get; } = new([
            new VekDataGridColumn<Row, string>("name", row => row.Name, editable: true, editTemplate: _ => "edit"),
            new VekDataGridColumn<Row, string>("other", row => row.Name)
        ]);
        public bool Valid { get; set; } = true;
        public ValueTask AfterCoreChangeAsync() => ValueTask.CompletedTask;
        public ValueTask<bool> ValidateEditDraftAsync(Row item) => ValueTask.FromResult(Valid);
    }
}
