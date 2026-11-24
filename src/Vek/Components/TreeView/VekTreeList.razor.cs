// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Vek.Extensions;
using Vek.State;
using Vek.Utilities;

namespace Vek;

/// <summary>
/// A multi-column hierarchical tree list displaying expandable tree nodes alongside aligned tabular columns with sorting and resizable headers.
/// </summary>
/// <typeparam name="T">The type of data item displayed in the tree list.</typeparam>
/// <seealso cref="VekTreeListColumn{T}"/>
/// <seealso cref="VekTreeView{T}"/>
public partial class VekTreeList<T> : VekComponentBase
{
    private readonly ParameterState<IReadOnlyCollection<ITreeItemData<T>>?> _itemsState;
    private readonly ParameterState<SelectionMode> _selectionModeState;
    private readonly ParameterState<T?> _selectedValueState;
    private readonly ParameterState<IReadOnlyCollection<T>?> _selectedValuesState;
    private readonly ParameterState<ITreeItemData<T>?> _selectedItemState;
    private readonly ParameterState<bool> _disabledState;
    private readonly ParameterState<bool> _readOnlyState;
    private readonly ParameterState<bool> _sortableState;
    private readonly ParameterState<SortMode> _sortModeState;
    private readonly ParameterState<bool> _resizableState;
    private readonly ParameterState<ResizeMode> _columnResizeModeState;
    private readonly ParameterState<int> _indentSizeState;
    private readonly ParameterState<Func<ITreeItemData<T>, Task<IEnumerable<ITreeItemData<T>>>>?> _loadChildrenAsyncState;

    private readonly List<VekTreeListColumn<T>> _columns = [];
    private readonly List<VekTreeListColumn<T>> _sortColumns = [];
    private readonly Dictionary<ITreeItemData<T>, ElementReference> _rowElements = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<ITreeItemData<T>> _loadingNodes = new(ReferenceEqualityComparer.Instance);
    private readonly ConditionalWeakTable<ITreeItemData<T>, NodeLoadState> _nodeLoadStates = new();

    private HashSet<T> _selection;
    private ITreeItemData<T>? _focusedItem;
    private ITreeItemData<T>? _pendingFocusItem;
    private bool _isFirstRender = true;

    // Active column resize drag state
    private VekTreeListColumn<T>? _resizingColumn;
    private VekTreeListColumn<T>? _resizeNextColumn;
    private double _resizeStartX;
    private double _resizeStartWidth;
    private double _resizeNextStartWidth;

    public VekTreeList()
    {
        using var registerScope = CreateRegisterScope();
        _itemsState = registerScope.RegisterParameter<IReadOnlyCollection<ITreeItemData<T>>?>(nameof(Items))
            .WithParameter(() => Items)
            .WithChangeHandler(OnItemsChangedAsync);
        _selectionModeState = registerScope.RegisterParameter<SelectionMode>(nameof(SelectionMode))
            .WithParameter(() => SelectionMode)
            .WithChangeHandler(OnSelectionOrBehaviorChangedAsync);
        _selectedValueState = registerScope.RegisterParameter<T?>(nameof(SelectedValue))
            .WithParameter(() => SelectedValue)
            .WithEventCallback(() => SelectedValueChanged)
            .WithChangeHandler(OnSelectedValueChangedAsync)
            .WithComparer(() => Comparer);
        _selectedValuesState = registerScope.RegisterParameter<IReadOnlyCollection<T>?>(nameof(SelectedValues))
            .WithParameter(() => SelectedValues)
            .WithEventCallback(() => SelectedValuesChanged)
            .WithChangeHandler(OnSelectedValuesChangedAsync)
            .WithComparer(() => Comparer, comparer => new CollectionComparer<T>(comparer));
        _selectedItemState = registerScope.RegisterParameter<ITreeItemData<T>?>(nameof(SelectedItem))
            .WithParameter(() => SelectedItem)
            .WithEventCallback(() => SelectedItemChanged);
        _disabledState = registerScope.RegisterParameter<bool>(nameof(Disabled))
            .WithParameter(() => Disabled)
            .WithChangeHandler(OnSelectionOrBehaviorChangedAsync);
        _readOnlyState = registerScope.RegisterParameter<bool>(nameof(ReadOnly))
            .WithParameter(() => ReadOnly)
            .WithChangeHandler(OnSelectionOrBehaviorChangedAsync);
        _sortableState = registerScope.RegisterParameter<bool>(nameof(Sortable))
            .WithParameter(() => Sortable)
            .WithChangeHandler(OnSortConfigurationChangedAsync);
        _sortModeState = registerScope.RegisterParameter<SortMode>(nameof(SortMode))
            .WithParameter(() => SortMode)
            .WithChangeHandler(OnSortConfigurationChangedAsync);
        _resizableState = registerScope.RegisterParameter<bool>(nameof(Resizable))
            .WithParameter(() => Resizable)
            .WithChangeHandler(OnSelectionOrBehaviorChangedAsync);
        _columnResizeModeState = registerScope.RegisterParameter<ResizeMode>(nameof(ColumnResizeMode))
            .WithParameter(() => ColumnResizeMode)
            .WithChangeHandler(OnSelectionOrBehaviorChangedAsync);
        _indentSizeState = registerScope.RegisterParameter<int>(nameof(IndentSize))
            .WithParameter(() => IndentSize)
            .WithChangeHandler(OnSelectionOrBehaviorChangedAsync);
        _loadChildrenAsyncState = registerScope.RegisterParameter<Func<ITreeItemData<T>, Task<IEnumerable<ITreeItemData<T>>>>?>(nameof(LoadChildrenAsync))
            .WithParameter(() => LoadChildrenAsync);

        _selection = new HashSet<T>(Comparer);
    }

    [CascadingParameter(Name = "RightToLeft")]
    public bool RightToLeft { get; set; }

    /// <summary>
    /// The hierarchical items displayed by this tree list.
    /// </summary>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Data)]
    public IReadOnlyCollection<ITreeItemData<T>>? Items { get; set; } = Array.Empty<ITreeItemData<T>>();

    /// <summary>
    /// Column definitions rendered inside this tree list.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Data)]
    public RenderFragment? Columns { get; set; }

    /// <summary>
    /// Alternate content slot for <see cref="VekTreeListColumn{T}"/> definitions when <see cref="Columns"/> is not specified.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Data)]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Content displayed when <see cref="Items"/> has no visible nodes.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Data)]
    public RenderFragment? EmptyContent { get; set; }

    /// <summary>
    /// Controls how many rows can be selected at one time.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="SelectionMode.SingleSelection"/>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Selecting)]
    public SelectionMode SelectionMode { get; set; } = SelectionMode.SingleSelection;

    /// <summary>
    /// Enables row selection when clicking or activating rows.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Selecting)]
    public bool Selectable { get; set; } = true;

    /// <summary>
    /// The currently selected value in <see cref="SelectionMode.SingleSelection"/> or <see cref="SelectionMode.ToggleSelection"/> mode.
    /// </summary>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Selecting)]
    public T? SelectedValue { get; set; }

    /// <summary>
    /// Occurs when <see cref="SelectedValue"/> changes.
    /// </summary>
    [Parameter]
    public EventCallback<T?> SelectedValueChanged { get; set; }

    /// <summary>
    /// The currently selected values in <see cref="SelectionMode.MultiSelection"/> mode.
    /// </summary>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Selecting)]
    public IReadOnlyCollection<T>? SelectedValues { get; set; }

    /// <summary>
    /// Occurs when <see cref="SelectedValues"/> changes.
    /// </summary>
    [Parameter]
    public EventCallback<IReadOnlyCollection<T>?> SelectedValuesChanged { get; set; }

    /// <summary>
    /// The currently selected node data item.
    /// </summary>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Selecting)]
    public ITreeItemData<T>? SelectedItem { get; set; }

    /// <summary>
    /// Occurs when <see cref="SelectedItem"/> changes.
    /// </summary>
    [Parameter]
    public EventCallback<ITreeItemData<T>?> SelectedItemChanged { get; set; }

    /// <summary>
    /// Uses checkboxes supporting an indeterminate state when descendants have mixed selection.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>. Applies when <see cref="SelectionMode"/> is <see cref="SelectionMode.MultiSelection"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Selecting)]
    public bool TriState { get; set; } = true;

    /// <summary>
    /// Automatically selects a parent node when all selectable descendants are checked.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>. Applies when <see cref="SelectionMode"/> is <see cref="SelectionMode.MultiSelection"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Selecting)]
    public bool AutoSelectParent { get; set; } = true;

    /// <summary>
    /// The comparer used to match selected values.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Selecting)]
    public IEqualityComparer<T?> Comparer { get; set; } = EqualityComparer<T?>.Default;

    /// <summary>
    /// Expands or collapses a node with children when clicking anywhere on its row.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.ClickAction)]
    public bool ExpandOnClick { get; set; }

    /// <summary>
    /// Expands or collapses a node with children when double-clicking its row.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>. Takes precedence over <see cref="ExpandOnClick"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.ClickAction)]
    public bool ExpandOnDoubleClick { get; set; }

    /// <summary>
    /// Horizontal indentation in pixels applied per nesting level in the primary tree column.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>20</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Appearance)]
    public int IndentSize { get; set; } = 20;

    /// <summary>
    /// Enables column header sorting across columns unless overridden on a specific <see cref="VekTreeListColumn{T}"/>.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Behavior)]
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// Controls single-column or multi-column sorting behavior.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="SortMode.Single"/>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Behavior)]
    public SortMode SortMode { get; set; } = SortMode.Single;

    /// <summary>
    /// Allows clicking a descending-sorted column header to clear sorting and return to original order.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Behavior)]
    public bool AllowUnsorted { get; set; } = true;

    /// <summary>
    /// Occurs when column sorting changes.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Behavior)]
    public EventCallback<TreeListSortEventArgs<T>> OnSortChanged { get; set; }

    /// <summary>
    /// Enables interactive column resizing across columns unless overridden on a specific <see cref="VekTreeListColumn{T}"/>.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Behavior)]
    public bool Resizable { get; set; } = true;

    /// <summary>
    /// Controls how resizing a column affects adjacent columns or the container width.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="ResizeMode.Container"/>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Behavior)]
    public ResizeMode ColumnResizeMode { get; set; } = ResizeMode.Container;

    /// <summary>
    /// Occurs when a column resize operation completes or resets.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Behavior)]
    public EventCallback<TreeListColumnResizeEventArgs<T>> OnColumnResized { get; set; }

    /// <summary>
    /// Occurs when a row is clicked.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.ClickAction)]
    public EventCallback<TreeListRowClickEventArgs<T>> OnRowClick { get; set; }

    /// <summary>
    /// An asynchronous delegate called to load child nodes on demand when a node is expanded.
    /// </summary>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Data)]
    public Func<ITreeItemData<T>, Task<IEnumerable<ITreeItemData<T>>>>? LoadChildrenAsync { get; set; }

    /// <summary>
    /// An optional predicate determining whether a node has children before they are loaded.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Behavior)]
    public Func<ITreeItemData<T>, bool>? HasChildren { get; set; }

    /// <summary>
    /// Shows a hover highlight on rows.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public bool Hover { get; set; } = true;

    /// <summary>
    /// Uses compact vertical padding for headers and rows.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public bool Dense { get; set; }

    /// <summary>
    /// Displays cell borders around columns and rows.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public bool Bordered { get; set; }

    /// <summary>
    /// Applies alternating row background striping.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public bool Striped { get; set; }

    /// <summary>
    /// Sticks the header row to the top when scrolling vertically.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>false</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public bool FixedHeader { get; set; }

    /// <summary>
    /// Sets a fixed height for the tree list container.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public string? Height { get; set; }

    /// <summary>
    /// Sets a maximum height for the tree list container.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public string? MaxHeight { get; set; }

    /// <summary>
    /// Sets a fixed width for the tree list root element.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public string? Width { get; set; }

    /// <summary>
    /// Disables user interaction across the tree list.
    /// </summary>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Behavior)]
    public bool Disabled { get; set; }

    /// <summary>
    /// Prevents selection changes while keeping expand/collapse and sorting active.
    /// </summary>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Selecting)]
    public bool ReadOnly { get; set; }

    /// <summary>
    /// The color used for selected rows.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="Color.Primary"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Selecting)]
    public Color Color { get; set; } = Color.Primary;

    /// <summary>
    /// The color used for multi-selection checkboxes.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Selecting)]
    public Color CheckBoxColor { get; set; }

    /// <summary>
    /// The icon displayed for expand/collapse chevrons.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Expanding)]
    public string ExpandButtonIcon { get; set; } = Icons.Material.Filled.ChevronRight;

    /// <summary>
    /// The icon displayed while a node loads child items asynchronously.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public string LoadingIcon { get; set; } = Icons.Material.Filled.Loop;

    /// <summary>
    /// The icon displayed inside sortable column headers.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public string SortIcon { get; set; } = Icons.Material.Filled.ArrowUpward;

    /// <summary>
    /// The icon displayed for checked rows in multi-selection mode.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Selecting)]
    public string CheckedIcon { get; set; } = Icons.Material.Filled.CheckBox;

    /// <summary>
    /// The icon displayed for unchecked rows in multi-selection mode.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Selecting)]
    public string UncheckedIcon { get; set; } = Icons.Material.Filled.CheckBoxOutlineBlank;

    /// <summary>
    /// The icon displayed for partially checked parent rows in multi-selection mode.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Selecting)]
    public string IndeterminateIcon { get; set; } = Icons.Material.Filled.IndeterminateCheckBox;

    /// <summary>
    /// The accessible label applied to the underlying <c>treegrid</c> element.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>"Tree list"</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public string? AriaLabel { get; set; }

    protected string Classname =>
        new CssBuilder("vek-treelist")
            .AddClass("vek-treelist-dense", Dense)
            .AddClass("vek-treelist-hover", !GetDisabled() && Hover)
            .AddClass("vek-treelist-bordered", Bordered)
            .AddClass("vek-treelist-striped", Striped)
            .AddClass("vek-treelist-fixed-header", FixedHeader)
            .AddClass("vek-treelist-disabled", GetDisabled())
            .AddClass("vek-treelist-resizing-active", _resizingColumn is not null)
            .AddClass($"vek-treelist-selected-{Color.ToStringFast(true)}")
            .AddClass(Class)
            .Build();

    protected string Stylename =>
        new StyleBuilder()
            .AddStyle("width", Width, !string.IsNullOrWhiteSpace(Width))
            .AddStyle(Style)
            .Build();

    private string ContainerClassname =>
        new CssBuilder("vek-treelist-container")
            .Build();

    private string ContainerStylename =>
        new StyleBuilder()
            .AddStyle("height", Height, !string.IsNullOrWhiteSpace(Height))
            .AddStyle("max-height", MaxHeight, !string.IsNullOrWhiteSpace(MaxHeight))
            .Build();

    private string TableClassname =>
        new CssBuilder("vek-treelist-table")
            .Build();

    private string TableStylename(IReadOnlyList<VekTreeListColumn<T>> visibleColumns)
    {
        var allHaveRuntimePx = visibleColumns.Count > 0 && visibleColumns.All(c => c.RuntimeWidthPx.HasValue);
        if (allHaveRuntimePx && _columnResizeModeState.Value == ResizeMode.Container)
        {
            var totalWidth = visibleColumns.Sum(c => c.RuntimeWidthPx!.Value);
            return new StyleBuilder()
                .AddStyle("width", $"{totalWidth.ToString("0.##", CultureInfo.InvariantCulture)}px")
                .AddStyle("min-width", "100%")
                .Build();
        }

        return new StyleBuilder()
            .AddStyle("width", "100%")
            .Build();
    }

    internal void RegisterColumn(VekTreeListColumn<T> column)
    {
        if (!_columns.Contains(column))
        {
            _columns.Add(column);
            if (column.GetSortDirection() != SortDirection.None)
            {
                SyncColumnSortDirection(column, column.GetSortDirection());
            }
            else if (!_isFirstRender)
            {
                StateHasChanged();
            }
        }
    }

    internal void UnregisterColumn(VekTreeListColumn<T> column)
    {
        _columns.Remove(column);
        if (_sortColumns.Remove(column))
        {
            ReindexSortColumns();
        }
        if (!_isFirstRender)
        {
            StateHasChanged();
        }
    }

    internal void NotifyColumnChanged()
    {
        if (!_isFirstRender)
        {
            StateHasChanged();
        }
    }

    internal void SyncColumnSortDirection(VekTreeListColumn<T> column, SortDirection direction)
    {
        if (direction == SortDirection.None)
        {
            _sortColumns.Remove(column);
            column.SortOrderIndex = -1;
        }
        else
        {
            if (_sortModeState.Value == SortMode.Single)
            {
                foreach (var other in _sortColumns.ToList())
                {
                    if (!ReferenceEquals(other, column))
                    {
                        other.SortOrderIndex = -1;
                    }
                }
                _sortColumns.Clear();
            }

            if (!_sortColumns.Contains(column))
            {
                _sortColumns.Add(column);
            }
        }

        ReindexSortColumns();
        if (!_isFirstRender)
        {
            StateHasChanged();
        }
    }

    /// <summary>
    /// Expands all nodes recursively across the tree list.
    /// </summary>
    public Task ExpandAllAsync()
    {
        SetExpandedRecursive(_itemsState.Value, true);
        StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Collapses all nodes recursively across the tree list.
    /// </summary>
    public Task CollapseAllAsync()
    {
        SetExpandedRecursive(_itemsState.Value, false);
        StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Expands the specified node, invoking <see cref="LoadChildrenAsync"/> if configured and not yet loaded.
    /// </summary>
    /// <param name="item">The item to expand.</param>
    public async Task ExpandNodeAsync(ITreeItemData<T> item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (!DetermineHasChildren(item))
        {
            return;
        }

        await EnsureChildrenLoadedAsync(item);
        item.Expanded = true;
        StateHasChanged();
    }

    /// <summary>
    /// Collapses the specified node.
    /// </summary>
    /// <param name="item">The item to collapse.</param>
    public Task CollapseNodeAsync(ITreeItemData<T> item)
    {
        ArgumentNullException.ThrowIfNull(item);
        item.Expanded = false;
        StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Toggles the expanded state of the specified node.
    /// </summary>
    /// <param name="item">The item to toggle.</param>
    public Task ToggleNodeAsync(ITreeItemData<T> item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.Expanded ? CollapseNodeAsync(item) : ExpandNodeAsync(item);
    }

    /// <summary>
    /// Applies sorting to the specified column in the requested direction.
    /// </summary>
    /// <param name="column">The column to sort.</param>
    /// <param name="direction">The sort direction to apply.</param>
    public async Task SortAsync(VekTreeListColumn<T> column, SortDirection direction)
    {
        ArgumentNullException.ThrowIfNull(column);
        if (!IsColumnSortable(column))
        {
            return;
        }

        await ApplySortAsync(column, direction, extendMultiSort: false);
    }

    /// <summary>
    /// Clears all active column sorting and restores original sibling order.
    /// </summary>
    public async Task ClearSortAsync()
    {
        var sorted = _sortColumns.ToList();
        _sortColumns.Clear();
        foreach (var col in sorted)
        {
            col.SortOrderIndex = -1;
            await col.SetSortDirectionAsync(SortDirection.None);
        }
        StateHasChanged();
    }

    /// <summary>
    /// Resets all interactive column widths back to their configured <see cref="VekTreeListColumn{T}.Width"/> values.
    /// </summary>
    public async Task ResetColumnWidthsAsync()
    {
        foreach (var column in _columns)
        {
            if (column.RuntimeWidthPx.HasValue)
            {
                var oldWidth = column.RuntimeWidthPx.Value;
                column.RuntimeWidthPx = null;
                await OnColumnResized.InvokeAsync(new TreeListColumnResizeEventArgs<T>
                {
                    Column = column,
                    OldWidth = oldWidth,
                    NewWidth = null
                });
            }
        }
        StateHasChanged();
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isFirstRender = false;
            if (_selectionModeState.Value == SelectionMode.MultiSelection && _selectedValuesState.Value is { } initialValues)
            {
                _selection = new HashSet<T>(initialValues, Comparer);
            }

            if (_columns.Count > 0)
            {
                StateHasChanged();
            }
        }

        if (_pendingFocusItem is not null && _rowElements.TryGetValue(_pendingFocusItem, out var targetRow))
        {
            _pendingFocusItem = null;
            try
            {
                await targetRow.FocusAsync();
            }
            catch
            {
                // Ignore focus failures when the element is no longer mounted or running in a headless test renderer.
            }
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private Task OnItemsChangedAsync(ParameterChangedEventArgs<IReadOnlyCollection<ITreeItemData<T>>?> args)
    {
        if (_isFirstRender)
        {
            return Task.CompletedTask;
        }
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task OnSelectionOrBehaviorChangedAsync()
    {
        if (_isFirstRender)
        {
            return Task.CompletedTask;
        }
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task OnSelectedValueChangedAsync(ParameterChangedEventArgs<T?> args)
    {
        if (_isFirstRender)
        {
            return Task.CompletedTask;
        }
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task OnSelectedValuesChangedAsync(ParameterChangedEventArgs<IReadOnlyCollection<T>?> args)
    {
        _selection = args.Value is not null
            ? new HashSet<T>(args.Value, Comparer)
            : new HashSet<T>(Comparer);
        if (!_isFirstRender)
        {
            StateHasChanged();
        }
        return Task.CompletedTask;
    }

    private async Task OnSortConfigurationChangedAsync()
    {
        if (_sortModeState.Value == SortMode.None || !_sortableState.Value)
        {
            await ClearSortAsync();
            return;
        }

        if (_sortModeState.Value == SortMode.Single && _sortColumns.Count > 1)
        {
            var primary = _sortColumns[0];
            foreach (var extra in _sortColumns.Skip(1).ToList())
            {
                extra.SortOrderIndex = -1;
                await extra.SetSortDirectionAsync(SortDirection.None);
            }
            _sortColumns.Clear();
            _sortColumns.Add(primary);
            ReindexSortColumns();
        }

        StateHasChanged();
    }

    private IReadOnlyList<VekTreeListColumn<T>> GetVisibleColumns()
    {
        return _columns.Where(c => c.GetVisible()).ToList();
    }

    private static VekTreeListColumn<T>? ResolvePrimaryColumn(IReadOnlyList<VekTreeListColumn<T>> visibleColumns)
    {
        if (visibleColumns.Count == 0)
        {
            return null;
        }

        return visibleColumns.FirstOrDefault(c => c.IsTreeColumn) ?? visibleColumns[0];
    }

    private List<TreeListRow<T>> BuildVisibleRows()
    {
        var rows = new List<TreeListRow<T>>();
        BuildVisibleRowsRecursive(_itemsState.Value, 0, null, rows);
        return rows;
    }

    private void BuildVisibleRowsRecursive(
        IReadOnlyCollection<ITreeItemData<T>>? items,
        int depth,
        ITreeItemData<T>? parent,
        List<TreeListRow<T>> rows)
    {
        if (items is null || items.Count == 0)
        {
            return;
        }

        var visibleSiblings = new List<ITreeItemData<T>>(items.Count);
        foreach (var item in items)
        {
            if (item.Visible)
            {
                visibleSiblings.Add(item);
            }
        }

        if (visibleSiblings.Count == 0)
        {
            return;
        }

        var ordered = _sortColumns.Count > 0 ? SortSiblings(visibleSiblings) : visibleSiblings;
        var setSize = ordered.Count;

        for (var i = 0; i < setSize; i++)
        {
            var item = ordered[i];
            rows.Add(new TreeListRow<T>(item, parent, depth, i + 1, setSize));

            if (item.Expanded && item.Children is { Count: > 0 })
            {
                BuildVisibleRowsRecursive(item.Children, depth + 1, item, rows);
            }
        }
    }

    private IReadOnlyList<ITreeItemData<T>> SortSiblings(List<ITreeItemData<T>> siblings)
    {
        if (siblings.Count <= 1 || _sortColumns.Count == 0)
        {
            return siblings;
        }

        var primaryColumn = ResolvePrimaryColumn(GetVisibleColumns());
        var sorted = new List<ITreeItemData<T>>(siblings);
        sorted.Sort((left, right) => CompareNodes(left, right, primaryColumn));
        return sorted;
    }

    private int CompareNodes(ITreeItemData<T> left, ITreeItemData<T> right, VekTreeListColumn<T>? primaryColumn)
    {
        foreach (var column in _sortColumns)
        {
            var direction = column.GetSortDirection();
            if (direction == SortDirection.None)
            {
                continue;
            }

            var leftKey = GetSortKey(column, left, ReferenceEquals(column, primaryColumn));
            var rightKey = GetSortKey(column, right, ReferenceEquals(column, primaryColumn));
            var comparison = CompareSortValues(leftKey, rightKey);

            if (comparison != 0)
            {
                return direction == SortDirection.Descending ? -comparison : comparison;
            }
        }

        return 0;
    }

    private static object? GetSortKey(VekTreeListColumn<T> column, ITreeItemData<T> item, bool isPrimary)
    {
        if (column.SortBy is not null)
        {
            return item.Value is not null ? column.SortBy(item.Value) : null;
        }

        if (column.Field is not null)
        {
            return item.Value is not null ? column.Field(item.Value) : null;
        }

        return isPrimary ? item.Text ?? item.Value?.ToString() : null;
    }

    private static int CompareSortValues(object? left, object? right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left is null)
        {
            return -1;
        }

        if (right is null)
        {
            return 1;
        }

        if (left is string leftStr && right is string rightStr)
        {
            return string.Compare(leftStr, rightStr, StringComparison.CurrentCultureIgnoreCase);
        }

        if (left.GetType() == right.GetType() && left is IComparable comparable)
        {
            return comparable.CompareTo(right);
        }

        return string.Compare(left.ToString(), right.ToString(), StringComparison.CurrentCultureIgnoreCase);
    }

    private static void SetExpandedRecursive(IReadOnlyCollection<ITreeItemData<T>>? items, bool expanded)
    {
        if (items is null)
        {
            return;
        }

        foreach (var item in items)
        {
            if (item.Expandable && (item.HasChildren || item.Children is { Count: > 0 }))
            {
                item.Expanded = expanded;
            }

            if (item.Children is { Count: > 0 })
            {
                SetExpandedRecursive(item.Children, expanded);
            }
        }
    }

    private bool DetermineHasChildren(ITreeItemData<T> item)
    {
        if (!item.Expandable)
        {
            return false;
        }

        if (HasChildren is not null)
        {
            return HasChildren(item);
        }

        if (item.Children is { Count: > 0 })
        {
            return true;
        }

        return item.HasChildren;
    }

    private bool IsNodeLoading(ITreeItemData<T> item) => _loadingNodes.Contains(item);

    private bool IsNodeLoaded(ITreeItemData<T> item) => _nodeLoadStates.TryGetValue(item, out var state) && state.IsLoaded;

    private async Task EnsureChildrenLoadedAsync(ITreeItemData<T> item)
    {
        if ((item.Children is { Count: > 0 }) || _loadChildrenAsyncState.Value is null || IsNodeLoaded(item) || IsNodeLoading(item))
        {
            return;
        }

        _loadingNodes.Add(item);
        StateHasChanged();

        try
        {
            var loaded = await _loadChildrenAsyncState.Value(item);
            if (loaded is not null)
            {
                item.Children = loaded as IReadOnlyCollection<ITreeItemData<T>> ?? loaded.ToList();
            }

            _nodeLoadStates.GetOrCreateValue(item).IsLoaded = true;
        }
        finally
        {
            _loadingNodes.Remove(item);
        }
    }

    private bool GetDisabled() => _disabledState.Value;

    private bool GetReadOnly() => _readOnlyState.Value;

    private bool IsColumnSortable(VekTreeListColumn<T> column)
    {
        if (_sortModeState.Value == SortMode.None)
        {
            return false;
        }

        return column.Sortable ?? _sortableState.Value;
    }

    private bool IsColumnResizable(VekTreeListColumn<T> column)
    {
        if (_columnResizeModeState.Value == ResizeMode.None && column.Resizable is null)
        {
            return false;
        }

        return column.Resizable ?? _resizableState.Value;
    }

    private async Task OnHeaderSortClickAsync(MouseEventArgs args, VekTreeListColumn<T> column)
    {
        if (GetDisabled() || !IsColumnSortable(column))
        {
            return;
        }

        var nextDirection = column.GetSortDirection() switch
        {
            SortDirection.None => SortDirection.Ascending,
            SortDirection.Ascending => SortDirection.Descending,
            SortDirection.Descending => AllowUnsorted ? SortDirection.None : SortDirection.Ascending,
            _ => SortDirection.Ascending
        };

        var extendMultiSort = _sortModeState.Value == SortMode.Multiple && (args.CtrlKey || args.MetaKey);
        await ApplySortAsync(column, nextDirection, extendMultiSort);
    }

    private async Task ApplySortAsync(VekTreeListColumn<T> column, SortDirection nextDirection, bool extendMultiSort)
    {
        if (!extendMultiSort || _sortModeState.Value == SortMode.Single)
        {
            foreach (var existing in _sortColumns.ToList())
            {
                if (!ReferenceEquals(existing, column))
                {
                    existing.SortOrderIndex = -1;
                    await existing.SetSortDirectionAsync(SortDirection.None);
                }
            }
            _sortColumns.Clear();
        }

        if (nextDirection == SortDirection.None)
        {
            _sortColumns.Remove(column);
            column.SortOrderIndex = -1;
        }
        else if (!_sortColumns.Contains(column))
        {
            _sortColumns.Add(column);
        }

        await column.SetSortDirectionAsync(nextDirection);
        ReindexSortColumns();

        var descriptors = _sortColumns
            .Select(c => new TreeListSortDescriptor<T>
            {
                Column = c,
                SortDirection = c.GetSortDirection()
            })
            .ToList();

        await OnSortChanged.InvokeAsync(new TreeListSortEventArgs<T>
        {
            Column = column,
            SortDirection = nextDirection,
            ActiveSorts = descriptors
        });

        StateHasChanged();
    }

    private void ReindexSortColumns()
    {
        for (var i = 0; i < _sortColumns.Count; i++)
        {
            _sortColumns[i].SortOrderIndex = i;
        }
    }

    private async Task OnResizerPointerDownAsync(PointerEventArgs args, VekTreeListColumn<T> column)
    {
        if (GetDisabled() || !IsColumnResizable(column))
        {
            return;
        }

        if (args.Detail > 1)
        {
            await ResetColumnWidthAsync(column);
            return;
        }

        var visibleColumns = GetVisibleColumns();
        if (_columnResizeModeState.Value == ResizeMode.Container)
        {
            await SnapshotContainerWidthsAsync(visibleColumns);
        }

        var startWidth = await MeasureColumnWidthAsync(column);
        VekTreeListColumn<T>? nextColumn = null;
        double nextStartWidth = 0;

        if (_columnResizeModeState.Value == ResizeMode.Column)
        {
            var index = -1;
            for (var i = 0; i < visibleColumns.Count; i++)
            {
                if (ReferenceEquals(visibleColumns[i], column))
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                for (var i = index + 1; i < visibleColumns.Count; i++)
                {
                    if (IsColumnResizable(visibleColumns[i]))
                    {
                        nextColumn = visibleColumns[i];
                        break;
                    }
                }
            }

            if (nextColumn is null)
            {
                return;
            }

            nextStartWidth = await MeasureColumnWidthAsync(nextColumn);
        }

        _resizingColumn = column;
        _resizeNextColumn = nextColumn;
        _resizeStartX = args.ClientX;
        _resizeStartWidth = startWidth;
        _resizeNextStartWidth = nextStartWidth;

        await JSRuntime.InvokeVoidAsyncIgnoreErrors("vekPointerCapture.capture", column.ResizerElement, args.PointerId);
        StateHasChanged();
    }

    private Task OnResizerPointerMoveAsync(PointerEventArgs args)
    {
        if (_resizingColumn is null)
        {
            return Task.CompletedTask;
        }

        ApplyResizeDelta(args.ClientX);
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task OnResizerPointerUpAsync(PointerEventArgs args)
    {
        if (_resizingColumn is null)
        {
            return;
        }

        var resizedColumn = _resizingColumn;
        var nextColumn = _resizeNextColumn;
        var oldWidth = _resizeStartWidth;
        var nextOldWidth = _resizeNextStartWidth;

        ApplyResizeDelta(args.ClientX);

        await JSRuntime.InvokeVoidAsyncIgnoreErrors("vekPointerCapture.release", resizedColumn.ResizerElement, args.PointerId);

        _resizingColumn = null;
        _resizeNextColumn = null;

        await OnColumnResized.InvokeAsync(new TreeListColumnResizeEventArgs<T>
        {
            Column = resizedColumn,
            OldWidth = oldWidth,
            NewWidth = resizedColumn.RuntimeWidthPx
        });

        if (nextColumn is not null)
        {
            await OnColumnResized.InvokeAsync(new TreeListColumnResizeEventArgs<T>
            {
                Column = nextColumn,
                OldWidth = nextOldWidth,
                NewWidth = nextColumn.RuntimeWidthPx
            });
        }

        StateHasChanged();
    }

    private Task OnResizerDoubleClickAsync(VekTreeListColumn<T> column)
    {
        return ResetColumnWidthAsync(column);
    }

    private async Task OnResizerKeyDownAsync(KeyboardEventArgs args, VekTreeListColumn<T> column)
    {
        if (GetDisabled() || !IsColumnResizable(column))
        {
            return;
        }

        if (args.Key == "Home")
        {
            await ResetColumnWidthAsync(column);
            return;
        }

        var step = args.Key switch
        {
            "ArrowRight" => RightToLeft ? -16d : 16d,
            "ArrowLeft" => RightToLeft ? 16d : -16d,
            _ => 0d
        };

        if (step == 0d)
        {
            return;
        }

        var visibleColumns = GetVisibleColumns();
        if (_columnResizeModeState.Value == ResizeMode.Container)
        {
            await SnapshotContainerWidthsAsync(visibleColumns);
        }

        var currentWidth = await MeasureColumnWidthAsync(column);
        var oldWidth = currentWidth;
        var targetWidth = ClampColumnWidth(column, currentWidth + step);

        if (_columnResizeModeState.Value == ResizeMode.Column)
        {
            var nextColumn = FindNextResizableColumn(visibleColumns, column);
            if (nextColumn is null)
            {
                return;
            }

            var nextCurrentWidth = await MeasureColumnWidthAsync(nextColumn);
            var delta = targetWidth - currentWidth;
            var nextTargetWidth = ClampColumnWidth(nextColumn, nextCurrentWidth - delta);
            var appliedDelta = nextCurrentWidth - nextTargetWidth;
            column.RuntimeWidthPx = ClampColumnWidth(column, currentWidth + appliedDelta);
            nextColumn.RuntimeWidthPx = nextTargetWidth;
        }
        else
        {
            column.RuntimeWidthPx = targetWidth;
        }

        await OnColumnResized.InvokeAsync(new TreeListColumnResizeEventArgs<T>
        {
            Column = column,
            OldWidth = oldWidth,
            NewWidth = column.RuntimeWidthPx
        });

        StateHasChanged();
    }

    private async Task ResetColumnWidthAsync(VekTreeListColumn<T> column)
    {
        if (!column.RuntimeWidthPx.HasValue)
        {
            return;
        }

        var oldWidth = column.RuntimeWidthPx.Value;
        column.RuntimeWidthPx = null;
        await OnColumnResized.InvokeAsync(new TreeListColumnResizeEventArgs<T>
        {
            Column = column,
            OldWidth = oldWidth,
            NewWidth = null
        });
        StateHasChanged();
    }

    private void ApplyResizeDelta(double clientX)
    {
        if (_resizingColumn is null)
        {
            return;
        }

        var deltaX = RightToLeft ? (_resizeStartX - clientX) : (clientX - _resizeStartX);
        var targetWidth = ClampColumnWidth(_resizingColumn, _resizeStartWidth + deltaX);

        if (_columnResizeModeState.Value == ResizeMode.Column && _resizeNextColumn is not null)
        {
            var requestedDelta = targetWidth - _resizeStartWidth;
            var nextTargetWidth = ClampColumnWidth(_resizeNextColumn, _resizeNextStartWidth - requestedDelta);
            var actualDelta = _resizeNextStartWidth - nextTargetWidth;
            _resizingColumn.RuntimeWidthPx = ClampColumnWidth(_resizingColumn, _resizeStartWidth + actualDelta);
            _resizeNextColumn.RuntimeWidthPx = nextTargetWidth;
        }
        else
        {
            _resizingColumn.RuntimeWidthPx = targetWidth;
        }
    }

    private async Task SnapshotContainerWidthsAsync(IReadOnlyList<VekTreeListColumn<T>> visibleColumns)
    {
        foreach (var col in visibleColumns)
        {
            if (!col.RuntimeWidthPx.HasValue)
            {
                col.RuntimeWidthPx = await MeasureColumnWidthAsync(col);
            }
        }
    }

    private async Task<double> MeasureColumnWidthAsync(VekTreeListColumn<T> column)
    {
        if (column.RuntimeWidthPx.HasValue)
        {
            return column.RuntimeWidthPx.Value;
        }

        var rect = await column.HeaderElement.VekGetBoundingClientRectAsync();
        if (rect?.Width > 0)
        {
            return rect.Width;
        }

        return ParsePixelWidth(column.GetWidth()) ?? Math.Max(column.MinWidth, 160d);
    }

    private static VekTreeListColumn<T>? FindNextResizableColumn(IReadOnlyList<VekTreeListColumn<T>> visibleColumns, VekTreeListColumn<T> current)
    {
        for (var i = 0; i < visibleColumns.Count; i++)
        {
            if (ReferenceEquals(visibleColumns[i], current))
            {
                for (var j = i + 1; j < visibleColumns.Count; j++)
                {
                    if (visibleColumns[j].Resizable ?? true)
                    {
                        return visibleColumns[j];
                    }
                }
                break;
            }
        }
        return null;
    }

    private static double ClampColumnWidth(VekTreeListColumn<T> column, double width)
    {
        var clamped = Math.Max(column.MinWidth, width);
        if (column.MaxWidth.HasValue)
        {
            clamped = Math.Min(column.MaxWidth.Value, clamped);
        }
        return clamped;
    }

    private static double? ParsePixelWidth(string? cssWidth)
    {
        if (string.IsNullOrWhiteSpace(cssWidth))
        {
            return null;
        }

        var trimmed = cssWidth.Trim();
        if (trimmed.EndsWith("px", StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[..^2].Trim();
        }

        return double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) && value > 0
            ? value
            : null;
    }

    private void EnsureFocusedRow(IReadOnlyList<TreeListRow<T>> visibleRows)
    {
        if (visibleRows.Count == 0)
        {
            _focusedItem = null;
            return;
        }

        if (_focusedItem is null || !visibleRows.Any(r => ReferenceEquals(r.Item, _focusedItem)))
        {
            _focusedItem = visibleRows[0].Item;
        }
    }

    private void OnRowFocus(ITreeItemData<T> item)
    {
        _focusedItem = item;
    }

    private async Task OnRowClickAsync(MouseEventArgs args, TreeListRow<T> row)
    {
        if (GetDisabled())
        {
            return;
        }

        _focusedItem = row.Item;

        if (ExpandOnClick && !ExpandOnDoubleClick && DetermineHasChildren(row.Item))
        {
            await ToggleNodeAsync(row.Item);
        }

        if (Selectable && !GetReadOnly())
        {
            await HandleRowSelectionAsync(row.Item);
        }

        await OnRowClick.InvokeAsync(new TreeListRowClickEventArgs<T>
        {
            Item = row.Item,
            Depth = row.Depth,
            MouseEventArgs = args
        });
    }

    private async Task OnRowDoubleClickAsync(MouseEventArgs args, TreeListRow<T> row)
    {
        if (GetDisabled())
        {
            return;
        }

        if (ExpandOnDoubleClick && DetermineHasChildren(row.Item))
        {
            await ToggleNodeAsync(row.Item);
        }
    }

    private async Task OnRowKeyDownAsync(KeyboardEventArgs args, TreeListRow<T> row, IReadOnlyList<TreeListRow<T>> visibleRows)
    {
        if (GetDisabled())
        {
            return;
        }

        var currentIndex = -1;
        for (var i = 0; i < visibleRows.Count; i++)
        {
            if (ReferenceEquals(visibleRows[i].Item, row.Item))
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex < 0)
        {
            return;
        }

        var expandKey = RightToLeft ? "ArrowLeft" : "ArrowRight";
        var collapseKey = RightToLeft ? "ArrowRight" : "ArrowLeft";

        if (args.Key == "ArrowDown")
        {
            if (currentIndex + 1 < visibleRows.Count)
            {
                FocusRow(visibleRows[currentIndex + 1].Item);
            }
        }
        else if (args.Key == "ArrowUp")
        {
            if (currentIndex - 1 >= 0)
            {
                FocusRow(visibleRows[currentIndex - 1].Item);
            }
        }
        else if (args.Key == expandKey)
        {
            if (DetermineHasChildren(row.Item))
            {
                if (!row.Item.Expanded)
                {
                    await ExpandNodeAsync(row.Item);
                }
                else if (currentIndex + 1 < visibleRows.Count && ReferenceEquals(visibleRows[currentIndex + 1].Parent, row.Item))
                {
                    FocusRow(visibleRows[currentIndex + 1].Item);
                }
            }
        }
        else if (args.Key == collapseKey)
        {
            if (row.Item.Expanded && DetermineHasChildren(row.Item))
            {
                await CollapseNodeAsync(row.Item);
            }
            else if (row.Parent is not null)
            {
                FocusRow(row.Parent);
            }
        }
        else if (args.Key == "Home")
        {
            if (visibleRows.Count > 0)
            {
                FocusRow(visibleRows[0].Item);
            }
        }
        else if (args.Key == "End")
        {
            if (visibleRows.Count > 0)
            {
                FocusRow(visibleRows[^1].Item);
            }
        }
        else if (args.Key is "Enter" or " ")
        {
            if (Selectable && !GetReadOnly())
            {
                await HandleRowSelectionAsync(row.Item);
            }
        }
    }

    private void FocusRow(ITreeItemData<T> item)
    {
        _focusedItem = item;
        _pendingFocusItem = item;
        StateHasChanged();
    }

    private async Task HandleRowSelectionAsync(ITreeItemData<T> item)
    {
        if (_selectionModeState.Value == SelectionMode.MultiSelection)
        {
            await OnMultiSelectToggleAsync(item);
            return;
        }

        var currentlySelected = IsItemSelected(item);
        if (_selectionModeState.Value == SelectionMode.ToggleSelection && currentlySelected)
        {
            await _selectedValueState.SetValueAsync(default);
            await _selectedItemState.SetValueAsync(null);
            return;
        }

        await _selectedValueState.SetValueAsync(item.Value);
        await _selectedItemState.SetValueAsync(item);
    }

    private async Task OnMultiSelectToggleAsync(ITreeItemData<T> item)
    {
        if (GetDisabled() || GetReadOnly() || !Selectable)
        {
            return;
        }

        var selectableNodes = CollectSubtreeValues(item);
        var allSelected = selectableNodes.Count > 0 && selectableNodes.All(v => _selection.Contains(v));

        foreach (var value in selectableNodes)
        {
            if (allSelected)
            {
                _selection.Remove(value);
            }
            else
            {
                _selection.Add(value);
            }
        }

        if (AutoSelectParent)
        {
            SynchronizeParentSelection(_itemsState.Value);
        }

        await _selectedValuesState.SetValueAsync(_selection.ToList());
        await _selectedItemState.SetValueAsync(item);
        StateHasChanged();
    }

    private List<T> CollectSubtreeValues(ITreeItemData<T> root)
    {
        var values = new List<T>();
        Traverse(root);
        return values;

        void Traverse(ITreeItemData<T> node)
        {
            if (node.Value is not null)
            {
                values.Add(node.Value);
            }

            if (node.Children is not null)
            {
                foreach (var child in node.Children)
                {
                    Traverse(child);
                }
            }
        }
    }

    private bool SynchronizeParentSelection(IReadOnlyCollection<ITreeItemData<T>>? items)
    {
        if (items is null || items.Count == 0)
        {
            return true;
        }

        var allSelectedInLevel = true;
        foreach (var item in items)
        {
            if (item.Children is { Count: > 0 })
            {
                var allDescendantsSelected = SynchronizeParentSelection(item.Children);
                if (item.Value is not null)
                {
                    if (allDescendantsSelected)
                    {
                        _selection.Add(item.Value);
                    }
                    else
                    {
                        _selection.Remove(item.Value);
                    }
                }
            }

            if (item.Value is not null && !_selection.Contains(item.Value))
            {
                allSelectedInLevel = false;
            }
        }

        return allSelectedInLevel;
    }

    private bool IsItemSelected(ITreeItemData<T> item)
    {
        if (!Selectable)
        {
            return false;
        }

        if (_selectionModeState.Value == SelectionMode.MultiSelection)
        {
            return item.Value is not null && _selection.Contains(item.Value);
        }

        if (_selectedItemState.Value is not null && ReferenceEquals(_selectedItemState.Value, item))
        {
            return true;
        }

        return item.Value is not null
            && _selectedValueState.Value is not null
            && Comparer.Equals(item.Value, _selectedValueState.Value);
    }

    private bool? GetTriStateChecked(ITreeItemData<T> item)
    {
        var hasSelected = false;
        var hasUnselected = false;

        Traverse(item);

        if (hasSelected && hasUnselected)
        {
            return TriState ? null : false;
        }

        return hasSelected;

        void Traverse(ITreeItemData<T> node)
        {
            if (node.Value is not null)
            {
                if (_selection.Contains(node.Value))
                {
                    hasSelected = true;
                }
                else
                {
                    hasUnselected = true;
                }
            }

            if (hasSelected && hasUnselected)
            {
                return;
            }

            if (node.Children is not null)
            {
                foreach (var child in node.Children)
                {
                    Traverse(child);
                    if (hasSelected && hasUnselected)
                    {
                        return;
                    }
                }
            }
        }
    }

    private string GetCheckBoxIcon(bool? state)
    {
        return state switch
        {
            true => CheckedIcon,
            false => UncheckedIcon,
            null => IndeterminateIcon
        };
    }

    private static string GetAriaChecked(bool? state)
    {
        return state switch
        {
            true => "true",
            false => "false",
            null => "mixed"
        };
    }

    private static string GetItemLabel(ITreeItemData<T> item)
    {
        return item.Text ?? item.Value?.ToString() ?? "Select row";
    }

    private string GetAriaLabel() => string.IsNullOrWhiteSpace(AriaLabel) ? "Tree list" : AriaLabel;

    private static string? GetColStyle(VekTreeListColumn<T> column)
    {
        if (column.RuntimeWidthPx.HasValue)
        {
            return $"width: {column.RuntimeWidthPx.Value.ToString("0.##", CultureInfo.InvariantCulture)}px;";
        }

        var width = column.GetWidth();
        return string.IsNullOrWhiteSpace(width) ? null : $"width: {width};";
    }

    private string GetHeaderCellClass(VekTreeListColumn<T> column)
    {
        var sortDirection = column.GetSortDirection();
        return new CssBuilder("vek-treelist-header-cell")
            .AddClass($"vek-treelist-align-{column.Alignment.ToStringFast(true)}")
            .AddClass("vek-treelist-sortable", IsColumnSortable(column))
            .AddClass("vek-treelist-sorted", sortDirection != SortDirection.None)
            .AddClass("vek-treelist-resizable", IsColumnResizable(column))
            .AddClass(column.HeaderClass)
            .Build();
    }

    private static string? GetHeaderCellStyle(VekTreeListColumn<T> column)
    {
        return new StyleBuilder()
            .AddStyle("min-width", $"{column.MinWidth}px")
            .AddStyle(column.HeaderStyle)
            .Build();
    }

    private string? GetAriaSort(VekTreeListColumn<T> column)
    {
        if (!IsColumnSortable(column))
        {
            return null;
        }

        return column.GetSortDirection() switch
        {
            SortDirection.Ascending => "ascending",
            SortDirection.Descending => "descending",
            _ => "none"
        };
    }

    private static string GetSortButtonAriaLabel(VekTreeListColumn<T> column)
    {
        var title = string.IsNullOrWhiteSpace(column.Title) ? "column" : column.Title;
        return column.GetSortDirection() switch
        {
            SortDirection.Ascending => $"Sort {title}, currently ascending",
            SortDirection.Descending => $"Sort {title}, currently descending",
            _ => $"Sort {title}"
        };
    }

    private static string GetSortIconClass(VekTreeListColumn<T> column)
    {
        return new CssBuilder("vek-treelist-sort-icon")
            .AddClass("vek-treelist-sort-asc", column.GetSortDirection() == SortDirection.Ascending)
            .AddClass("vek-treelist-sort-desc", column.GetSortDirection() == SortDirection.Descending)
            .Build();
    }

    private string GetResizerClass(VekTreeListColumn<T> column)
    {
        return new CssBuilder("vek-treelist-resizer")
            .AddClass("vek-treelist-resizing", ReferenceEquals(_resizingColumn, column))
            .Build();
    }

    private static int GetResizerAriaValueNow(VekTreeListColumn<T> column)
    {
        if (column.RuntimeWidthPx.HasValue)
        {
            return (int)Math.Round(column.RuntimeWidthPx.Value);
        }

        var parsed = ParsePixelWidth(column.GetWidth());
        return parsed.HasValue ? (int)Math.Round(parsed.Value) : column.MinWidth;
    }

    private string GetRowClass(ITreeItemData<T> item, bool isSelected, bool disabled)
    {
        return new CssBuilder("vek-treelist-row")
            .AddClass("vek-treelist-row-selected", isSelected && !disabled)
            .AddClass("vek-treelist-row-expanded", item.Expanded)
            .AddClass("vek-treelist-row-disabled", disabled)
            .AddClass("cursor-pointer", !disabled && (Selectable || ExpandOnClick))
            .Build();
    }

    private static string GetBodyCellClass(VekTreeListColumn<T> column, bool isPrimary)
    {
        return new CssBuilder("vek-treelist-cell")
            .AddClass("vek-treelist-cell-primary", isPrimary)
            .AddClass($"vek-treelist-align-{column.Alignment.ToStringFast(true)}")
            .AddClass(column.CellClass)
            .Build();
    }

    private static string? GetBodyCellStyle(VekTreeListColumn<T> column)
    {
        return string.IsNullOrWhiteSpace(column.CellStyle) ? null : column.CellStyle;
    }

    private string? GetTreeIndentStyle(int depth)
    {
        if (depth <= 0)
        {
            return null;
        }

        var indentPx = Math.Max(0, _indentSizeState.Value) * depth;
        return $"padding-inline-start: {indentPx}px;";
    }

    private static string GetToggleButtonClass(ITreeItemData<T> item, bool isLoading)
    {
        return new CssBuilder("vek-treelist-toggle-btn")
            .AddClass("vek-treelist-toggle-expanded", item.Expanded && !isLoading)
            .AddClass("vek-treelist-toggle-loading", isLoading)
            .Build();
    }

    private static string GetCheckBoxClass() => "vek-treelist-checkbox";

    private static string? FormatCellValue(VekTreeListColumn<T> column, ITreeItemData<T> item, bool isPrimary)
    {
        object? raw = null;
        if (column.Field is not null && item.Value is not null)
        {
            raw = column.Field(item.Value);
        }
        else if (isPrimary)
        {
            raw = item.Text ?? item.Value?.ToString();
        }

        if (raw is null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(column.Format) && raw is IFormattable formattable)
        {
            return formattable.ToString(column.Format, CultureInfo.CurrentCulture);
        }

        return raw.ToString();
    }

    private sealed class NodeLoadState
    {
        public bool IsLoaded { get; set; }
    }
}
