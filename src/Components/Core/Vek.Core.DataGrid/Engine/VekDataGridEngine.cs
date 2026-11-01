using System.Collections.Concurrent;
using System.Globalization;

namespace Vek.Core.DataGrid;

/// <summary>
/// Platform-neutral DataGrid controller and computation engine.
/// Holds per-grid independent state and executes sorting, filtering, paging,
/// grouping, aggregates, selection, and edit transitions.
/// </summary>
/// <typeparam name="TItem">The type of row item.</typeparam>
/// <typeparam name="TTemplate">The native template type supplied by the UI adapter.</typeparam>
public class VekDataGridEngine<TItem, TTemplate>
{
    private readonly Dictionary<string, IVekDataGridColumn<TItem, TTemplate>> _columns = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<VekDataGridSort> _sorts = [];
    private readonly List<VekDataGridFilter> _filters = [];
    private readonly List<VekDataGridGroupDefinition> _groups = [];
    private readonly List<VekDataGridAggregateDefinition<TItem>> _aggregates = [];
    private readonly HashSet<string> _expandedGroupKeys = new(StringComparer.Ordinal);
    private readonly HashSet<string> _knownGroupKeys = new(StringComparer.Ordinal);
    private readonly HashSet<string> _selectedKeys = new(StringComparer.Ordinal);

    private Func<TItem, string>? _itemKeySelector;
    private int _page = 1;
    private int _pageSize = 10;
    private CultureInfo _culture = CultureInfo.InvariantCulture;

    // Edit state
    private TItem? _originalEditItem;
    private TItem? _draftEditItem;
    private bool _isEditing;
    private bool _groupStateImported;

    /// <summary>
    /// Initializes a new instance of the <see cref="VekDataGridEngine{TItem, TTemplate}"/> class.
    /// </summary>
    /// <param name="columns">Optional initial columns.</param>
    /// <param name="itemKeySelector">Optional selector to produce a stable unique key for each item.</param>
    public VekDataGridEngine(
        IEnumerable<IVekDataGridColumn<TItem, TTemplate>>? columns = null,
        Func<TItem, string>? itemKeySelector = null)
    {
        _itemKeySelector = itemKeySelector;
        if (columns != null)
        {
            foreach (var col in columns)
            {
                _columns[col.Key] = col;
            }
        }
    }

    /// <summary>Gets the registered columns.</summary>
    public IReadOnlyCollection<IVekDataGridColumn<TItem, TTemplate>> Columns => _columns.Values;

    /// <summary>Gets or sets the current 1-based page.</summary>
    public int Page
    {
        get => _page;
        set => _page = Math.Max(1, value);
    }

    /// <summary>Gets or sets the page size.</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Max(1, value);
    }

    /// <summary>Gets or sets the culture used for formatting and comparisons.</summary>
    public CultureInfo Culture
    {
        get => _culture;
        set => _culture = value ?? CultureInfo.InvariantCulture;
    }

    /// <summary>Gets active sorts ordered by priority.</summary>
    public IReadOnlyList<VekDataGridSort> Sorts => _sorts;

    /// <summary>Gets active filters.</summary>
    public IReadOnlyList<VekDataGridFilter> Filters => _filters;

    /// <summary>Gets or sets the default case sensitivity for new filter interactions.</summary>
    public bool FilterCaseSensitive { get; set; }

    /// <summary>Gets active group definitions.</summary>
    public IReadOnlyList<VekDataGridGroupDefinition> Groups => _groups;

    /// <summary>Gets configured aggregate definitions.</summary>
    public IReadOnlyList<VekDataGridAggregateDefinition<TItem>> Aggregates => _aggregates;

    /// <summary>Gets the selected item keys.</summary>
    public IReadOnlySet<string> SelectedKeys => _selectedKeys;

    /// <summary>Gets the keys of currently expanded groups.</summary>
    public IReadOnlySet<string> ExpandedGroupKeys => _expandedGroupKeys;

    /// <summary>Gets whether an edit session is currently active.</summary>
    public bool IsEditing => _isEditing;

    /// <summary>Gets the original item being edited, if any.</summary>
    public TItem? OriginalEditItem => _originalEditItem;

    /// <summary>Gets the current draft item being edited, if any.</summary>
    public TItem? DraftEditItem => _draftEditItem;

    /// <summary>
    /// Registers or updates a column.
    /// </summary>
    public void AddOrUpdateColumn(IVekDataGridColumn<TItem, TTemplate> column)
    {
        ArgumentNullException.ThrowIfNull(column);
        _columns[column.Key] = column;
    }

    /// <summary>Removes a column and its active operations from this engine.</summary>
    public void RemoveColumn(string columnKey)
    {
        if (!_columns.Remove(columnKey)) return;
        _sorts.RemoveAll(sort => string.Equals(sort.ColumnKey, columnKey, StringComparison.OrdinalIgnoreCase));
        _filters.RemoveAll(filter => string.Equals(filter.ColumnKey, columnKey, StringComparison.OrdinalIgnoreCase));
        _groups.RemoveAll(group => string.Equals(group.ColumnKey, columnKey, StringComparison.OrdinalIgnoreCase));
        _aggregates.RemoveAll(aggregate => string.Equals(aggregate.ColumnKey, columnKey, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Sets the item key selector function.
    /// </summary>
    public void SetItemKeySelector(Func<TItem, string>? keySelector)
    {
        _itemKeySelector = keySelector;
    }

    /// <summary>
    /// Sets current page and page size.
    /// </summary>
    public void SetPage(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }

    /// <summary>
    /// Replaces active sort descriptors.
    /// </summary>
    public void SetSorts(IEnumerable<VekDataGridSort> sorts)
    {
        var nextSorts = sorts?.OrderBy(sort => sort.Priority).ToArray() ?? [];
        _sorts.Clear();
        _sorts.AddRange(nextSorts);
    }

    /// <summary>Applies a sort-header interaction using the shared sorting rules.</summary>
    /// <param name="columnKey">The key of the column whose header was activated.</param>
    /// <param name="mode">The grid sorting mode.</param>
    /// <param name="allowUnsorted">Whether a descending sort can be removed by the next activation.</param>
    /// <param name="removeSort">Whether this interaction explicitly removes the column sort.</param>
    /// <param name="preserveExistingSorts">Whether adding a new sort preserves other active sorts.</param>
    /// <returns>True when the interaction is accepted; false when sorting is disabled or the column cannot be sorted.</returns>
    public bool ApplySortInteraction(
        string columnKey,
        VekDataGridSortMode mode,
        bool allowUnsorted = false,
        bool removeSort = false,
        bool preserveExistingSorts = false)
    {
        if (mode == VekDataGridSortMode.None
            || !_columns.TryGetValue(columnKey, out var column)
            || !column.Sortable)
        {
            return false;
        }

        var sorts = _sorts.ToList();
        var index = sorts.FindIndex(sort => string.Equals(sort.ColumnKey, columnKey, StringComparison.Ordinal));
        if (removeSort && mode == VekDataGridSortMode.Multiple)
        {
            if (index >= 0) sorts.RemoveAt(index);
        }
        else if (index >= 0)
        {
            var current = sorts[index];
            if (allowUnsorted && current.Direction == VekSortDirection.Descending)
            {
                sorts.RemoveAt(index);
            }
            else
            {
                sorts[index] = current with
                {
                    Direction = current.Direction == VekSortDirection.Ascending
                        ? VekSortDirection.Descending
                        : VekSortDirection.Ascending
                };
            }
        }
        else
        {
            if (mode == VekDataGridSortMode.Single || !preserveExistingSorts) sorts.Clear();
            sorts.Add(new VekDataGridSort(columnKey, VekSortDirection.Ascending));
        }

        SetSorts(sorts.Select((sort, sortIndex) => sort with { Priority = sortIndex + 1 }));
        return true;
    }

    /// <summary>
    /// Replaces active filter descriptors.
    /// </summary>
    public void SetFilters(IEnumerable<VekDataGridFilter> filters)
    {
        var nextFilters = filters?.ToArray() ?? [];
        _filters.Clear();
        _filters.AddRange(nextFilters);
    }

    /// <summary>Applies or clears a filter for one column and resets paging.</summary>
    public void ApplyFilter(string columnKey, VekDataGridFilterOperator op, object? value, bool caseSensitive = false)
    {
        var filters = _filters.Where(filter => !string.Equals(filter.ColumnKey, columnKey, StringComparison.Ordinal)).ToList();
        var unaryOperator = op is VekDataGridFilterOperator.IsNull
            or VekDataGridFilterOperator.IsNotNull
            or VekDataGridFilterOperator.IsEmpty
            or VekDataGridFilterOperator.IsNotEmpty;
        if (unaryOperator || value is not null and not "")
        {
            filters.Add(new VekDataGridFilter(columnKey, op, value, caseSensitive));
        }

        SetFilters(filters);
        Page = 1;
    }

    /// <summary>Removes one filter or all filters and resets paging.</summary>
    public void RemoveFilter(string? columnKey = null)
    {
        SetFilters(columnKey is null
            ? Array.Empty<VekDataGridFilter>()
            : _filters.Where(filter => !string.Equals(filter.ColumnKey, columnKey, StringComparison.Ordinal)));
        Page = 1;
    }

    /// <summary>
    /// Replaces active group definitions.
    /// </summary>
    public void SetGroups(IEnumerable<VekDataGridGroupDefinition> groups)
    {
        var nextGroups = groups?.ToArray() ?? [];
        _groups.Clear();
        _groups.AddRange(nextGroups);
    }

    /// <summary>Reconciles the current expanded state with the groups present in the latest data.</summary>
    public void SetKnownGroupKeys(IEnumerable<string> keys)
    {
        var nextKeys = new HashSet<string>(keys, StringComparer.Ordinal);
        foreach (var key in nextKeys)
        {
            if (!_knownGroupKeys.Contains(key) && !_groupStateImported)
            {
                _expandedGroupKeys.Add(key);
            }
        }
        _expandedGroupKeys.RemoveWhere(key => !nextKeys.Contains(key));
        _knownGroupKeys.Clear();
        _knownGroupKeys.UnionWith(nextKeys);
        _groupStateImported = false;
    }

    /// <summary>Sets the expanded state for one group key.</summary>
    public void SetGroupExpanded(string key, bool expanded)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        _knownGroupKeys.Add(key);
        if (expanded) _expandedGroupKeys.Add(key);
        else _expandedGroupKeys.Remove(key);
    }

    /// <summary>Gets whether a group is expanded.</summary>
    public bool IsGroupExpanded(string key) => _expandedGroupKeys.Contains(key);

    /// <summary>
    /// Replaces configured aggregates.
    /// </summary>
    public void SetAggregates(IEnumerable<VekDataGridAggregateDefinition<TItem>> aggregates)
    {
        var nextAggregates = aggregates?.ToArray() ?? [];
        _aggregates.Clear();
        _aggregates.AddRange(nextAggregates);
    }

    /// <summary>
    /// Sets selected keys.
    /// </summary>
    public void SetSelectedKeys(IEnumerable<string> keys)
    {
        var nextKeys = keys?.ToArray() ?? [];
        _selectedKeys.Clear();
        foreach (var key in nextKeys)
        {
            _selectedKeys.Add(key);
        }
    }

    /// <summary>
    /// Toggles selection of an item.
    /// </summary>
    public void ToggleSelection(TItem item, VekDataGridSelectionMode mode)
    {
        if (mode == VekDataGridSelectionMode.None) return;
        var key = GetItemKey(item);

        if (mode == VekDataGridSelectionMode.Single)
        {
            var isSelected = _selectedKeys.Contains(key);
            _selectedKeys.Clear();
            if (!isSelected)
            {
                _selectedKeys.Add(key);
            }
            return;
        }

        if (!_selectedKeys.Add(key))
        {
            _selectedKeys.Remove(key);
        }
    }

    /// <summary>
    /// Checks whether an item is selected.
    /// </summary>
    public bool IsSelected(TItem item)
    {
        return _selectedKeys.Contains(GetItemKey(item));
    }

    /// <summary>
    /// Starts editing an item.
    /// </summary>
    /// <param name="item">The item to edit.</param>
    /// <param name="draftClone">Optional cloned draft; if null, the item itself is used.</param>
    public void BeginEdit(TItem item, TItem? draftClone = default)
    {
        _originalEditItem = item;
        _draftEditItem = draftClone != null ? draftClone : item;
        _isEditing = true;
    }

    /// <summary>
    /// Updates the draft item during editing.
    /// </summary>
    public void UpdateDraft(TItem draft)
    {
        _draftEditItem = draft;
    }

    /// <summary>
    /// Cancels the current edit session.
    /// </summary>
    public void CancelEdit()
    {
        _originalEditItem = default;
        _draftEditItem = default;
        _isEditing = false;
    }

    /// <summary>
    /// Commits the edit session using an optional validator and commit handler.
    /// </summary>
    public async ValueTask<bool> CommitEditAsync(
        IVekDataGridValidator<TItem>? validator = null,
        IVekDataGridCommitHandler<TItem>? commitHandler = null,
        CancellationToken cancellationToken = default)
    {
        if (!_isEditing || _draftEditItem == null || _originalEditItem == null)
        {
            return false;
        }

        if (validator != null)
        {
            var validation = await validator.ValidateAsync(_draftEditItem, cancellationToken).ConfigureAwait(false);
            if (!validation.IsValid)
            {
                return false;
            }
        }

        if (commitHandler != null)
        {
            var committed = await commitHandler.CommitAsync(_originalEditItem, _draftEditItem, cancellationToken).ConfigureAwait(false);
            if (!committed)
            {
                return false;
            }
        }

        CancelEdit();
        return true;
    }

    /// <summary>
    /// Exports the current state snapshot.
    /// </summary>
    public VekDataGridSnapshot ExportSnapshot()
    {
        return new VekDataGridSnapshot(
            _page,
            _pageSize,
            _sorts.ToArray(),
            _filters.ToArray(),
            _groups.ToArray(),
            _expandedGroupKeys.ToArray(),
            _selectedKeys.ToArray());
    }

    /// <summary>
    /// Imports a state snapshot.
    /// </summary>
    public void ImportSnapshot(VekDataGridSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        _page = snapshot.Page;
        _pageSize = snapshot.PageSize;

        _sorts.Clear();
        if (snapshot.Sorts != null) _sorts.AddRange(snapshot.Sorts);

        _filters.Clear();
        if (snapshot.Filters != null) _filters.AddRange(snapshot.Filters);

        _groups.Clear();
        if (snapshot.Groups != null) _groups.AddRange(snapshot.Groups);

        _expandedGroupKeys.Clear();
        _knownGroupKeys.Clear();
        if (snapshot.ExpandedGroupKeys != null)
        {
            foreach (var k in snapshot.ExpandedGroupKeys) _expandedGroupKeys.Add(k);
        }
        _groupStateImported = true;

        _selectedKeys.Clear();
        if (snapshot.SelectedKeys != null)
        {
            foreach (var k in snapshot.SelectedKeys) _selectedKeys.Add(k);
        }
    }

    /// <summary>
    /// Creates a neutral server load request descriptor reflecting current state.
    /// </summary>
    public VekDataGridServerRequest CreateServerRequest()
    {
        return new VekDataGridServerRequest(
            _page,
            _pageSize,
            _sorts.ToArray(),
            _filters.ToArray());
    }

    /// <summary>
    /// Processes a local in-memory dataset according to current filter, sort, and page parameters.
    /// </summary>
    /// <param name="source">The full local source collection.</param>
    /// <returns>Filtered, sorted, and paged results.</returns>
    public VekDataGridData<TItem> GetLocalData(IEnumerable<TItem> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var rows = GetFilteredAndSortedItems(source);
        var skip = Math.Max(0, (_page - 1) * _pageSize);
        return new VekDataGridData<TItem>(rows.Skip(skip).Take(_pageSize).ToArray(), rows.Count);
    }

    /// <summary>Calculates the number of pages for a result count and page size.</summary>
    public int GetPageCount(int totalItems, int? pageSize = null)
    {
        var effectivePageSize = Math.Max(1, pageSize ?? PageSize);
        return totalItems <= 0 ? 1 : (totalItems - 1) / effectivePageSize + 1;
    }

    /// <summary>Filters and sorts a local data source without applying page slicing.</summary>
    public IReadOnlyList<TItem> GetFilteredAndSortedItems(IEnumerable<TItem> source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var query = source.AsQueryable();
        if (_filters.Count > 0) query = ApplyFiltering(query);
        if (_sorts.Count > 0) query = ApplySorting(query);
        return query.ToArray();
    }

    /// <summary>Checks whether an item satisfies all active filters.</summary>
    public bool MatchesFilters(TItem item)
    {
        foreach (var filter in _filters)
        {
            if (!_columns.TryGetValue(filter.ColumnKey, out var column)) return false;
            if (!MatchesFilter((column.FilterBy ?? column.ValueAccessor)(item), filter)) return false;
        }
        return true;
    }

    /// <summary>
    /// Computes nested groups and aggregates for an item collection.
    /// </summary>
    public IReadOnlyList<VekDataGridGroup<TItem>> ComputeGroups(IEnumerable<TItem> items)
    {
        if (_groups.Count == 0) return Array.Empty<VekDataGridGroup<TItem>>();

        return GroupRecursive(items.ToList(), 0, string.Empty);
    }

    /// <summary>
    /// Computes configured aggregates over the provided items.
    /// </summary>
    public IReadOnlyList<VekDataGridAggregateResult> ComputeAggregates(
        IReadOnlyList<TItem> items,
        VekDataGridAggregateScope? scopeFilter = null)
    {
        if (_aggregates.Count == 0) return Array.Empty<VekDataGridAggregateResult>();

        var results = new List<VekDataGridAggregateResult>();

        foreach (var def in _aggregates)
        {
            if (scopeFilter.HasValue && def.Scope != VekDataGridAggregateScope.Both && def.Scope != scopeFilter.Value)
            {
                continue;
            }

            var col = _columns.GetValueOrDefault(def.ColumnKey);
            var title = def.Key;
            var selector = def.Selector ?? (col != null ? col.AggregateBy ?? col.ValueAccessor : null);

            var values = items.Select(i => selector != null ? selector(i) : null).ToList();
            var value = CalculateAggregate(def, values);
            var formatted = FormatAggregate(value, def.Format);

            results.Add(new VekDataGridAggregateResult(def.Key, title, value, formatted));
        }

        return results;
    }

    /// <summary>Gets the stable selection key for an item using the configured selector or the default hash.</summary>
    /// <param name="item">The item whose selection key is returned.</param>
    /// <returns>The configured key, or the item's hash code formatted with invariant culture.</returns>
    public string GetItemKey(TItem item)
    {
        if (_itemKeySelector != null) return _itemKeySelector(item);
        return item?.GetHashCode().ToString(CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private IQueryable<TItem> ApplyFiltering(IQueryable<TItem> query)
    {
        return query.AsEnumerable().Where(MatchesFilters).AsQueryable();
    }

    private bool MatchesFilter(object? cellValue, VekDataGridFilter filter)
    {
        var op = filter.Operator;
        var filterVal = filter.Value;

        if (op == VekDataGridFilterOperator.IsNull) return cellValue is null;
        if (op == VekDataGridFilterOperator.IsNotNull) return cellValue is not null;
        if (op == VekDataGridFilterOperator.IsEmpty) return cellValue is null || cellValue is string { Length: 0 };
        if (op == VekDataGridFilterOperator.IsNotEmpty) return cellValue is not null && cellValue is not string { Length: 0 };

        if (cellValue is null || filterVal is null)
        {
            return op switch
            {
                VekDataGridFilterOperator.Equals => cellValue is null && filterVal is null,
                VekDataGridFilterOperator.NotEquals => (cellValue is null) != (filterVal is null),
                _ => false
            };
        }

        var compareOptions = filter.CaseSensitive ? CompareOptions.None : CompareOptions.IgnoreCase;
        var compareInfo = Culture.CompareInfo;

        if (cellValue is string sVal && filterVal is string fVal)
        {
            return op switch
            {
                VekDataGridFilterOperator.Equals => compareInfo.Compare(sVal, fVal, compareOptions) == 0,
                VekDataGridFilterOperator.NotEquals => compareInfo.Compare(sVal, fVal, compareOptions) != 0,
                VekDataGridFilterOperator.Contains => compareInfo.IndexOf(sVal, fVal, compareOptions) >= 0,
                VekDataGridFilterOperator.StartsWith => compareInfo.IsPrefix(sVal, fVal, compareOptions),
                VekDataGridFilterOperator.EndsWith => compareInfo.IsSuffix(sVal, fVal, compareOptions),
                _ => CompareObjects(sVal, fVal, op, filter.CaseSensitive)
            };
        }

        return CompareObjects(cellValue, filterVal, op, filter.CaseSensitive);
    }

    private bool CompareObjects(object left, object right, VekDataGridFilterOperator op, bool caseSensitive)
    {
        if (left is IComparable compLeft)
        {
            try
            {
                var convertedRight = Convert.ChangeType(right, left.GetType(), Culture);
                var comp = compLeft.CompareTo(convertedRight);
                return op switch
                {
                    VekDataGridFilterOperator.Equals => comp == 0,
                    VekDataGridFilterOperator.NotEquals => comp != 0,
                    VekDataGridFilterOperator.GreaterThan => comp > 0,
                    VekDataGridFilterOperator.GreaterThanOrEqual => comp >= 0,
                    VekDataGridFilterOperator.LessThan => comp < 0,
                    VekDataGridFilterOperator.LessThanOrEqual => comp <= 0,
                    _ => false
                };
            }
            catch
            {
                // If conversion fails, compare as strings
            }
        }

        var lStr = left.ToString() ?? "";
        var rStr = right.ToString() ?? "";
        var compareOptions = caseSensitive ? CompareOptions.None : CompareOptions.IgnoreCase;
        var stringComp = Culture.CompareInfo.Compare(lStr, rStr, compareOptions);

        return op switch
        {
            VekDataGridFilterOperator.Equals => stringComp == 0,
            VekDataGridFilterOperator.NotEquals => stringComp != 0,
            VekDataGridFilterOperator.GreaterThan => stringComp > 0,
            VekDataGridFilterOperator.GreaterThanOrEqual => stringComp >= 0,
            VekDataGridFilterOperator.LessThan => stringComp < 0,
            VekDataGridFilterOperator.LessThanOrEqual => stringComp <= 0,
            _ => false
        };
    }

    private IQueryable<TItem> ApplySorting(IQueryable<TItem> query)
    {
        var list = query.ToList();
        var activeSorts = _sorts
            .Where(sort => _columns.ContainsKey(sort.ColumnKey))
            .Select(sort => (Sort: sort, Column: _columns[sort.ColumnKey]))
            .ToArray();
        if (activeSorts.Length == 0) return list.AsQueryable();

        var comparer = Comparer<TItem>.Create((left, right) =>
        {
            foreach (var (sort, column) in activeSorts)
            {
                var comparison = column.CompareItems(left, right);
                if (comparison != 0)
                {
                    return sort.Direction == VekSortDirection.Ascending ? comparison : -comparison;
                }
            }
            return 0;
        });

        return list.OrderBy(item => item, comparer).AsQueryable();
    }

    private IReadOnlyList<VekDataGridGroup<TItem>> GroupRecursive(List<TItem> items, int level, string parentKey)
    {
        if (level >= _groups.Count) return Array.Empty<VekDataGridGroup<TItem>>();

        var groupDef = _groups[level];
        _columns.TryGetValue(groupDef.ColumnKey, out var col);
        var title = col?.Title ?? groupDef.ColumnKey;
        var accessor = col?.GroupBy ?? col?.ValueAccessor ?? (_ => null);

        var grouped = items.GroupBy(accessor);

        if (groupDef.SortOrder == VekDataGridGroupSortOrder.Ascending)
        {
            grouped = grouped.OrderBy(g => g.Key, ObjectComparer.Instance);
        }
        else
        {
            grouped = grouped.OrderByDescending(g => g.Key, ObjectComparer.Instance);
        }

        var result = new List<VekDataGridGroup<TItem>>();

        foreach (var g in grouped)
        {
            var groupItems = g.ToList();
            var aggregates = ComputeAggregates(groupItems, VekDataGridAggregateScope.Group);
            var valueKey = Convert.ToString(g.Key, CultureInfo.InvariantCulture) ?? string.Empty;
            var groupKey = $"{parentKey}/{groupDef.ColumnKey}:{valueKey.Length}:{valueKey}";
            result.Add(new VekDataGridGroup<TItem>(
                groupDef.ColumnKey,
                title,
                g.Key,
                level,
                groupItems,
                aggregates,
                GroupRecursive(groupItems, level + 1, groupKey),
                groupKey));
        }

        return result;
    }

    private static object? CalculateAggregate(VekDataGridAggregateDefinition<TItem> def, IReadOnlyList<object?> values)
    {
        var nonNull = values.Where(v => v != null).ToList();

        return def.Type switch
        {
            VekDataGridAggregateType.Count => nonNull.Count,
            VekDataGridAggregateType.Custom when def.CustomAggregate != null => def.CustomAggregate(values),
            VekDataGridAggregateType.Sum => CalculateSum(nonNull),
            VekDataGridAggregateType.Average => CalculateAverage(nonNull),
            VekDataGridAggregateType.Min => nonNull.OrderBy(x => x, ObjectComparer.Instance).FirstOrDefault(),
            VekDataGridAggregateType.Max => nonNull.OrderByDescending(x => x, ObjectComparer.Instance).FirstOrDefault(),
            _ => null
        };
    }

    private static object? CalculateSum(IReadOnlyList<object?> values)
    {
        decimal sum = 0;
        foreach (var v in values)
        {
            if (v is null) continue;
            try
            {
                sum += Convert.ToDecimal(v, CultureInfo.InvariantCulture);
            }
            catch { }
        }
        return sum;
    }

    private static object? CalculateAverage(IReadOnlyList<object?> values)
    {
        if (values.Count == 0) return 0.0;
        decimal sum = 0;
        int count = 0;
        foreach (var v in values)
        {
            if (v is null) continue;
            try
            {
                sum += Convert.ToDecimal(v, CultureInfo.InvariantCulture);
                count++;
            }
            catch { }
        }
        return count > 0 ? (double)(sum / count) : 0.0;
    }

    private string FormatAggregate(object? value, string? format)
    {
        if (value == null) return string.Empty;
        if (!string.IsNullOrWhiteSpace(format) && value is IFormattable formattable)
        {
            return formattable.ToString(format, _culture);
        }
        return value.ToString() ?? string.Empty;
    }

    private sealed class ObjectComparer : IComparer<object?>
    {
        public static readonly ObjectComparer Instance = new();

        public int Compare(object? x, object? y)
        {
            if (x is null && y is null) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            if (x is IComparable compX)
            {
                try
                {
                    var convertedY = Convert.ChangeType(y, x.GetType(), CultureInfo.InvariantCulture);
                    return compX.CompareTo(convertedY);
                }
                catch { }
            }

            return string.Compare(x.ToString(), y.ToString(), StringComparison.OrdinalIgnoreCase);
        }
    }
}

