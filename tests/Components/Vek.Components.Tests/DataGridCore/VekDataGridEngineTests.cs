using Vek.Core.DataGrid;

namespace Vek.Components.Tests.DataGridCore;

public sealed class VekDataGridEngineTests
{
    /// <summary>
    /// Applies sorting before paging when the engine produces local data.
    /// </summary>
    [Fact]
    public void LocalDataIsSortedBeforeTheRequestedPageIsSelected()
    {
        var engine = new VekDataGridEngine<Row>(
        [
            new VekDataGridColumn<Row>("rank", row => row.Rank)
        ]);
        engine.SetSorts([new VekDataGridSort("rank", VekSortDirection.Ascending, 0)]);
        engine.SetPage(2, 1);

        var result = engine.GetLocalData(
        [
            new Row("Charlie", 3),
            new Row("Alpha", 1),
            new Row("Bravo", 2)
        ]);

        Assert.Equal(3, result.TotalItems);
        Assert.Equal("Bravo", Assert.Single(result.Items).Name);
    }

    /// <summary>
    /// Filters data based on column predicates prior to total counting and paging.
    /// </summary>
    [Fact]
    public void LocalDataIsFilteredPriorToPaging()
    {
        var engine = new VekDataGridEngine<Row>(
        [
            new VekDataGridColumn<Row>("name", row => row.Name),
            new VekDataGridColumn<Row>("rank", row => row.Rank)
        ]);
        engine.SetFilters([new VekDataGridFilter("name", VekDataGridFilterOperator.Contains, "a")]);

        var result = engine.GetLocalData(
        [
            new Row("Charlie", 3),
            new Row("Echo", 5),
            new Row("Alpha", 1),
            new Row("Bravo", 2)
        ]);

        Assert.Equal(3, result.TotalItems);
        Assert.DoesNotContain(result.Items, r => r.Name == "Echo");
    }

    /// <summary>
    /// Applies the shared sort interaction rules for single, multiple, and removable sorts.
    /// </summary>
    [Fact]
    public void SortInteractionUpdatesSortStateInCore()
    {
        var engine = new VekDataGridEngine<Row>([
            new VekDataGridColumn<Row>("name", row => row.Name),
            new VekDataGridColumn<Row>("rank", row => row.Rank)
        ]);

        engine.ApplySortInteraction("name", VekDataGridSortMode.Single, allowUnsorted: true);
        Assert.Equal(VekSortDirection.Ascending, Assert.Single(engine.Sorts).Direction);

        engine.ApplySortInteraction("name", VekDataGridSortMode.Single, allowUnsorted: true);
        Assert.Equal(VekSortDirection.Descending, Assert.Single(engine.Sorts).Direction);

        engine.ApplySortInteraction("name", VekDataGridSortMode.Single, allowUnsorted: true);
        Assert.Empty(engine.Sorts);

        engine.ApplySortInteraction("name", VekDataGridSortMode.Multiple);
        engine.ApplySortInteraction("rank", VekDataGridSortMode.Multiple, preserveExistingSorts: true);
        Assert.Equal(["name", "rank"], engine.Sorts.Select(sort => sort.ColumnKey));
        Assert.Equal([1, 2], engine.Sorts.Select(sort => sort.Priority));

        engine.ApplySortInteraction("name", VekDataGridSortMode.Multiple, removeSort: true);
        Assert.Equal("rank", Assert.Single(engine.Sorts).ColumnKey);
        Assert.Equal(1, engine.Sorts[0].Priority);
    }

    /// <summary>
    /// Keeps template slots strongly typed through one generic Core column contract and engine.
    /// </summary>
    [Fact]
    public void GenericColumnContractCarriesTypedTemplatesThroughTheEngine()
    {
        VekDataGridTemplate<Row, string> cellTemplate = row => row.Name;
        VekDataGridTemplate<VekDataGridEditTemplateContext<Row>, string> editTemplate = _ => "edit";
        var column = new VekDataGridColumn<Row, string>(
            "name",
            row => row.Name,
            cellTemplate: cellTemplate,
            editTemplate: editTemplate,
            editable: true);
        var engine = new VekDataGridEngine<Row, string>([column]);

        var registeredColumn = Assert.Single(engine.Columns);

        Assert.True(registeredColumn.Editable);
        Assert.Equal("Ada", registeredColumn.CellTemplate!(new Row("Ada", 1)));
        Assert.Same(editTemplate, registeredColumn.EditTemplate);
    }

    /// <summary>
    /// Keeps null comparisons and empty-string filters distinct from whitespace values.
    /// </summary>
    [Fact]
    public void NullAndEmptyFiltersUseExplicitComparisonRules()
    {
        var engine = new VekDataGridEngine<Row>([
            new VekDataGridColumn<Row>("name", row => row.Name)
        ]);

        engine.SetFilters([new VekDataGridFilter("name", VekDataGridFilterOperator.NotEquals, null)]);
        Assert.False(engine.MatchesFilters(new Row(null!, 0)));
        Assert.True(engine.MatchesFilters(new Row("Alpha", 0)));

        engine.SetFilters([new VekDataGridFilter("name", VekDataGridFilterOperator.IsEmpty, null)]);
        Assert.False(engine.MatchesFilters(new Row(" ", 0)));
        Assert.True(engine.MatchesFilters(new Row(string.Empty, 0)));
    }

    /// <summary>
    /// Replaces or removes a column filter and resets paging through the Core contract.
    /// </summary>
    [Fact]
    public void FilterInteractionsUpdateCoreStateAndResetPage()
    {
        var engine = new VekDataGridEngine<Row>([
            new VekDataGridColumn<Row>("name", row => row.Name)
        ]);
        engine.Page = 4;

        engine.ApplyFilter("name", VekDataGridFilterOperator.Contains, "Alpha");

        Assert.Equal(1, engine.Page);
        Assert.Equal("Alpha", Assert.Single(engine.Filters).Value);

        engine.Page = 3;
        engine.ApplyFilter("name", VekDataGridFilterOperator.Contains, string.Empty);
        Assert.Equal(1, engine.Page);
        Assert.Empty(engine.Filters);

        engine.ApplyFilter("name", VekDataGridFilterOperator.IsEmpty, null);
        Assert.Equal(VekDataGridFilterOperator.IsEmpty, Assert.Single(engine.Filters).Operator);

        engine.Page = 2;
        engine.RemoveFilter("name");
        Assert.Equal(1, engine.Page);
        Assert.Empty(engine.Filters);
    }

    /// <summary>
    /// Ensures multiple grid engine instances maintain independent state.
    /// </summary>
    [Fact]
    public void MultipleGridEnginesMaintainIndependentState()
    {
        var engine1 = new VekDataGridEngine<Row>();
        var engine2 = new VekDataGridEngine<Row>();

        engine1.SetPage(3, 20);
        engine1.SetSorts([new VekDataGridSort("col1", VekSortDirection.Ascending)]);
        engine1.SetSorts(engine1.Sorts);

        Assert.Equal(3, engine1.Page);
        Assert.Equal(20, engine1.PageSize);
        Assert.Single(engine1.Sorts);

        Assert.Equal(1, engine2.Page);
        Assert.Equal(10, engine2.PageSize);
        Assert.Empty(engine2.Sorts);
    }

    /// <summary>
    /// Exports state snapshot and accurately restores it into a new engine instance.
    /// </summary>
    [Fact]
    public void StateSnapshotExportsAndRestoresAccurately()
    {
        var original = new VekDataGridEngine<Row>(itemKeySelector: r => r.Name);
        original.SetPage(4, 25);
        original.SetSorts([new VekDataGridSort("rank", VekSortDirection.Descending, 1)]);
        original.SetFilters([new VekDataGridFilter("name", VekDataGridFilterOperator.StartsWith, "A")]);
        original.SetGroups([new VekDataGridGroupDefinition("rank", VekDataGridGroupSortOrder.Ascending)]);
        original.SetGroupExpanded("/rank:1:1", expanded: true);
        original.SetSelectedKeys(["Alpha", "Bravo"]);

        var snapshot = original.ExportSnapshot();

        var restored = new VekDataGridEngine<Row>(itemKeySelector: r => r.Name);
        restored.ImportSnapshot(snapshot);

        Assert.Equal(4, restored.Page);
        Assert.Equal(25, restored.PageSize);
        Assert.Single(restored.Sorts);
        Assert.Equal("rank", restored.Sorts[0].ColumnKey);
        Assert.Equal(VekSortDirection.Descending, restored.Sorts[0].Direction);
        Assert.Single(restored.Filters);
        Assert.Equal(VekDataGridFilterOperator.StartsWith, restored.Filters[0].Operator);
        Assert.Single(restored.Groups);
        Assert.True(restored.IsGroupExpanded("/rank:1:1"));
        Assert.Contains("Alpha", restored.SelectedKeys);
        Assert.Contains("Bravo", restored.SelectedKeys);
    }

    /// <summary>
    /// Computes nested groups and aggregates correctly.
    /// </summary>
    [Fact]
    public void ComputesGroupingAndAggregatesAccurately()
    {
        var engine = new VekDataGridEngine<Row>(
        [
            new VekDataGridColumn<Row>("name", row => row.Name),
            new VekDataGridColumn<Row>("category", row => row.Category),
            new VekDataGridColumn<Row>("rank", row => row.Rank)
        ]);

        engine.SetGroups([
            new VekDataGridGroupDefinition("category", VekDataGridGroupSortOrder.Ascending),
            new VekDataGridGroupDefinition("rank", VekDataGridGroupSortOrder.Ascending)
        ]);
        engine.SetAggregates([
            new Vek.Core.DataGrid.VekDataGridAggregateDefinition<Row>("rankSum", "rank", VekDataGridAggregateType.Sum),
            new Vek.Core.DataGrid.VekDataGridAggregateDefinition<Row>("rowCount", "name", VekDataGridAggregateType.Count)
        ]);

        var items = new List<Row>
        {
            new("A", 10, "Cat1"),
            new("B", 20, "Cat1"),
            new("C", 30, "Cat2")
        };

        var groups = engine.ComputeGroups(items);
        Assert.Equal(2, groups.Count);

        var firstGroup = groups.First(g => (string?)g.Value == "Cat1");
        Assert.Equal(2, firstGroup.Items.Count);
        Assert.Equal(2, firstGroup.Children?.Count);
        var sumAgg = Assert.Single(firstGroup.Aggregates, a => a.Key == "rankSum");
        Assert.Equal(30m, Convert.ToDecimal(sumAgg.Value));

        var overallAggregates = engine.ComputeAggregates(items, VekDataGridAggregateScope.Footer);
        var overallCount = Assert.Single(overallAggregates, a => a.Key == "rowCount");
        Assert.Equal(3, overallCount.Value);
    }

    /// <summary>
    /// Manages the edit session lifecycle including commit validation and cancellation.
    /// </summary>
    [Fact]
    public async Task ManagesEditSessionLifecycleAndCommit()
    {
        var engine = new VekDataGridEngine<Row>();
        var row = new Row("Item1", 100);

        Assert.False(engine.IsEditing);

        engine.BeginEdit(row, new Row("Item1-Modified", 200));
        Assert.True(engine.IsEditing);
        Assert.Equal(row, engine.OriginalEditItem);
        Assert.Equal(200, engine.DraftEditItem?.Rank);

        var committedDraft = false;
        var fakeHandler = new FakeCommitHandler((orig, draft) =>
        {
            committedDraft = draft.Rank == 200;
            return ValueTask.FromResult(true);
        });

        var success = await engine.CommitEditAsync(commitHandler: fakeHandler);

        Assert.True(success);
        Assert.True(committedDraft);
        Assert.False(engine.IsEditing);
        Assert.Null(engine.DraftEditItem);
    }

    private sealed record Row(string Name, int Rank, string Category = "Default");

    private sealed class FakeCommitHandler(Func<Row, Row, ValueTask<bool>> onCommit) : IVekDataGridCommitHandler<Row>
    {
        public ValueTask<bool> CommitAsync(Row originalItem, Row draftItem, CancellationToken cancellationToken = default)
            => onCommit(originalItem, draftItem);
    }
}
