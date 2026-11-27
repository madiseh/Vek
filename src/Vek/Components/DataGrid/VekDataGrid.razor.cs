// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Vek.Components.DataGrid.Export;
using Vek.Components.Highlighter;
using Vek.State;
using Vek.Utilities;
using Vek.Utilities.Clone;
using Collections = System.Collections;

namespace Vek
{
    /// <summary>
    /// Displays rows of data in a sortable, filterable grid with multiselection, pagination, inline editing, grouping, and aggregation.
    /// </summary>
    /// <remarks>
    /// Supports server-side sorting, filtering, and paging of large datasets, including through an <see cref="IQueryable{T}"/> source.
    /// </remarks>
    /// <typeparam name="T">The type of data represented by each row in this grid.</typeparam>
    /// <seealso cref="Column{T}" />
    /// <seealso cref="VekDataGridPager{T}" />
    /// <seealso cref="VekTable{T}" />
    [CascadingTypeParameter(nameof(T))]
    public partial class VekDataGrid<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : VekComponentBase, IDisposable
    {
        [Inject]
        private IJSRuntime JSRuntime { get; set; } = null!;

        [Inject]
        private IScrollManager ScrollManager { get; set; } = null!;

        private VekForm? _editForm;
        internal int? _rowsPerPage;
        private int _currentPage = 0;
        internal bool _groupInitialExpanded = true;
        internal VekVirtualize<IndexBag<T>>? _vekVirtualize;
        private bool _isFirstRendered = false;
        private bool _filtersMenuVisible = false;
        private bool _columnsPanelVisible = false;
        internal HashSet<T> _openHierarchies;
        private HashSet<T> _initialExpansions;
        private Func<T, bool>? _initialExpandedFunc = null;
        private Func<T, bool>? _buttonDisabledFunc = null;
        private EventCallback<DataGridHierarchyVisibilityToggledEventArgs<T>> _hierarchyColumnVisibilityToggled;
        private string _columnsPanelSearch = string.Empty;
        private VekDropContainer<Column<T>>? _dropContainer;
        private VekDropContainer<Column<T>>? _columnsPanelDropContainer;
        private readonly PropertyInfo[] _properties = typeof(T).GetProperties();
        private CancellationTokenSource? _serverDataCancellationTokenSource;
        private IEnumerable<T>? _currentRenderFilteredItemsCache = null;
        internal GroupDefinition<T>? _groupDefinition;
        internal Dictionary<GroupKey, bool> _groupExpansionsDict = [];
        private GridData<T> _serverData = new() { TotalItems = 0, Items = Array.Empty<T>() };
        private Func<IFilterDefinition<T>> _defaultFilterDefinitionFactory = () => new FilterDefinition<T>();
        private (double Top, double Left) _filtersMenuPosition = (0, 0);
        private (double Top, double Left) _columnsPanelPosition = (0, 0);
        private Guid? _filterDefinitionIdToFocus;

        private readonly ParameterState<T?> _selectedItemState;
        private readonly ParameterState<HashSet<T>?> _selectedItemsState;
        private readonly ParameterState<bool> _expandSingleRowState;
        internal readonly ParameterState<bool> _showFilterRowState;
        internal readonly ParameterState<int> _filterRowInputDelayState;
        internal readonly ParameterState<bool> _showSearchBoxState;
        internal readonly ParameterState<string?> _searchTextState;
        internal readonly ParameterState<DataGridSearchTextParseMode> _searchTextParseModeState;
        internal readonly ParameterState<int> _searchBoxInputDelayState;
        internal readonly ParameterState<string?> _searchBoxPlaceholderState;
        internal readonly ParameterState<bool> _highlightSearchResultsState;
        private readonly ParameterState<bool> _focusedRowEnabledState;
        private readonly ParameterState<int> _focusedRowIndexState;
        private readonly ParameterState<T?> _focusedItemState;
        private readonly ParameterState<bool> _autoScrollToFocusedRowState;
        private readonly ParameterState<bool> _keyboardNavigationEnabledState;
        private readonly ParameterState<int> _focusedColumnIndexState;
        private readonly ParameterState<DataGridSelectionMode> _selectionModeState;
        private readonly ParameterState<DataGridCellRange?> _selectedCellRangeState;
        private readonly ParameterState<bool> _contextMenuEnabledState;
        private readonly ParameterState<bool> _allowRowReorderState;
        private readonly DataGridRowDragManager<T> _rowDragManager = new();
        private VekMenu? _contextMenu;
        private bool _contextMenuOpen;
        private DataGridContextMenuArgs<T>? _currentContextMenuArgs;
        private readonly DataGridCellSelectionModel _cellSelectionModel = new();
        private bool _isDraggingCellRange;
        private bool _cellRangePointerGesture;
        private readonly string _gridId = Identifier.Create("vek_datagrid_");

        /// <summary>
        /// Gets the manager responsible for focused row tracking and keyboard navigation.
        /// </summary>
        public DataGridFocusManager<T> FocusManager { get; }

        /// <summary>
        /// Gets the handler responsible for WAI-ARIA Grid keyboard navigation, roving tabindex, and keyboard edit/selection lifecycles.
        /// </summary>
        public DataGridAriaKeyboardHandler<T> KeyboardHandler { get; }

        internal IReadOnlyList<string> CurrentSearchTokens { get; private set; } = Array.Empty<string>();
        internal ParameterState<bool> HighlightSearchResultsState => _highlightSearchResultsState;
        internal bool HasFocusedRow => _focusedRowEnabledState.Value;
        internal bool IsKeyboardNavigationEnabled => _keyboardNavigationEnabledState.Value;
        internal bool IsAriaGrid => _keyboardNavigationEnabledState.Value || _selectionModeState.Value == DataGridSelectionMode.CellRange;
        internal int CurrentFocusedIndex => _focusedRowIndexState.Value;
        internal int CurrentFocusedColumnIndex => _focusedColumnIndexState.Value;
        internal T? CurrentFocusedItem => _focusedItemState.Value;
        internal bool HasActiveEdit => !EqualityComparer<T?>.Default.Equals(_editingSourceItem, default) || _isEditFormOpen;

        private const string PrePrefix = "__vek_dg_pre__:";
        private const string PostPrefix = "__vek_dg_post__:";

        /// <summary>
        /// Holds the editors of the row being edited, which are the only controls a commit validates.
        /// </summary>
        internal readonly DataGridInlineEditValidator _inlineEditValidator;

        /// <summary>
        /// Inline data attributes for positioning the menu at the cursor's location.
        /// </summary>
        internal Dictionary<string, object> FiltersPositionAttributes => new()
        {
            { "data-pc-x", _filtersMenuPosition.Left.ToString(CultureInfo.InvariantCulture) },
            { "data-pc-y", _filtersMenuPosition.Top.ToString(CultureInfo.InvariantCulture) }
        };

        internal Dictionary<string, object> ColumnsPanelPositionAttributes => new()
        {
            { "data-pc-x", _columnsPanelPosition.Left.ToString(CultureInfo.InvariantCulture) },
            { "data-pc-y", _columnsPanelPosition.Top.ToString(CultureInfo.InvariantCulture) }
        };

        public VekDataGrid()
        {
            _inlineEditValidator = new DataGridInlineEditValidator(() => Validator);
            Selection = new HashSet<T>(Comparer);
            SelectedItems = new HashSet<T>(Comparer);
            _openHierarchies = new HashSet<T>(Comparer);
            _initialExpansions = new HashSet<T>(Comparer);
            FocusManager = new DataGridFocusManager<T>(this);
            KeyboardHandler = new DataGridAriaKeyboardHandler<T>(this);
            using var registerScope = CreateRegisterScope();
            registerScope.RegisterParameter<IEnumerable<T>?>(nameof(Items))
                .WithParameter(() => Items)
                .WithChangeHandler(OnItemsChangedAsync);

            _selectedItemState = registerScope.RegisterParameter<T?>(nameof(SelectedItem))
                .WithParameter(() => SelectedItem)
                .WithEventCallback(() => SelectedItemChanged)
                .WithChangeHandler(OnSelectedItemChangedAsync);

            _selectedItemsState = registerScope.RegisterParameter<HashSet<T>?>(nameof(SelectedItems))
                .WithParameter(() => SelectedItems)
                .WithEventCallback(() => SelectedItemsChanged)
                .WithChangeHandler(OnSelectedItemsChanged);

            _expandSingleRowState = registerScope.RegisterParameter<bool>(nameof(ExpandSingleRow))
                .WithParameter(() => ExpandSingleRow)
                .WithChangeHandler(OnExpandSingleRowChangedAsync);

            registerScope.RegisterParameter<IEqualityComparer<T>?>(nameof(Comparer))
                .WithParameter(() => Comparer)
                .WithChangeHandler(OnComparerChanged);

            _showFilterRowState = registerScope.RegisterParameter<bool>(nameof(ShowFilterRow))
                .WithParameter(() => ShowFilterRow);

            _filterRowInputDelayState = registerScope.RegisterParameter<int>(nameof(FilterRowInputDelay))
                .WithParameter(() => FilterRowInputDelay);

            _showSearchBoxState = registerScope.RegisterParameter<bool>(nameof(ShowSearchBox))
                .WithParameter(() => ShowSearchBox);

            _searchTextState = registerScope.RegisterParameter<string?>(nameof(SearchText))
                .WithParameter(() => SearchText)
                .WithEventCallback(() => SearchTextChanged)
                .WithChangeHandler(OnSearchTextChangedAsync);

            _searchTextParseModeState = registerScope.RegisterParameter<DataGridSearchTextParseMode>(nameof(SearchTextParseMode))
                .WithParameter(() => SearchTextParseMode)
                .WithChangeHandler(OnSearchTextParseModeChangedAsync);

            _searchBoxInputDelayState = registerScope.RegisterParameter<int>(nameof(SearchBoxInputDelay))
                .WithParameter(() => SearchBoxInputDelay);

            _searchBoxPlaceholderState = registerScope.RegisterParameter<string?>(nameof(SearchBoxPlaceholder))
                .WithParameter(() => SearchBoxPlaceholder);

            _highlightSearchResultsState = registerScope.RegisterParameter<bool>(nameof(HighlightSearchResults))
                .WithParameter(() => HighlightSearchResults);

            _focusedRowEnabledState = registerScope.RegisterParameter<bool>(nameof(FocusedRowEnabled))
                .WithParameter(() => FocusedRowEnabled);

            _focusedRowIndexState = registerScope.RegisterParameter<int>(nameof(FocusedRowIndex))
                .WithParameter(() => FocusedRowIndex)
                .WithEventCallback(() => FocusedRowIndexChanged)
                .WithChangeHandler(OnFocusedRowIndexChangedAsync);

            _focusedItemState = registerScope.RegisterParameter<T?>(nameof(FocusedItem))
                .WithParameter(() => FocusedItem)
                .WithEventCallback(() => FocusedItemChanged)
                .WithChangeHandler(OnFocusedItemChangedAsync);

            _autoScrollToFocusedRowState = registerScope.RegisterParameter<bool>(nameof(AutoScrollToFocusedRow))
                .WithParameter(() => AutoScrollToFocusedRow);

            _keyboardNavigationEnabledState = registerScope.RegisterParameter<bool>(nameof(KeyboardNavigationEnabled))
                .WithParameter(() => KeyboardNavigationEnabled);

            _focusedColumnIndexState = registerScope.RegisterParameter<int>(nameof(FocusedColumnIndex))
                .WithParameter(() => FocusedColumnIndex)
                .WithEventCallback(() => FocusedColumnIndexChanged)
                .WithChangeHandler(OnFocusedColumnIndexChangedAsync);

            _selectionModeState = registerScope.RegisterParameter<DataGridSelectionMode>(nameof(SelectionMode))
                .WithParameter(() => SelectionMode);

            _selectedCellRangeState = registerScope.RegisterParameter<DataGridCellRange?>(nameof(SelectedCellRange))
                .WithParameter(() => SelectedCellRange)
                .WithEventCallback(() => SelectedCellRangeChanged)
                .WithChangeHandler(OnSelectedCellRangeChangedAsync);

            _contextMenuEnabledState = registerScope.RegisterParameter<bool>(nameof(ContextMenuEnabled))
                .WithParameter(() => ContextMenuEnabled);

            _allowRowReorderState = registerScope.RegisterParameter<bool>(nameof(AllowRowReorder))
                .WithParameter(() => AllowRowReorder);
        }

        protected string Classname =>
            new CssBuilder("vek-table")
                .AddClass("vek-data-grid")
                .AddClass("vek-xs-table", Breakpoint == Breakpoint.Xs)
                .AddClass("vek-sm-table", Breakpoint == Breakpoint.Sm)
                .AddClass("vek-md-table", Breakpoint == Breakpoint.Md)
                .AddClass("vek-lg-table", Breakpoint == Breakpoint.Lg || Breakpoint == Breakpoint.Always)
                .AddClass("vek-xl-table", Breakpoint == Breakpoint.Xl || Breakpoint == Breakpoint.Always)
                .AddClass("vek-table-dense", Dense)
                .AddClass("vek-table-hover", Hover)
                .AddClass("vek-table-bordered", Bordered)
                .AddClass("vek-table-striped", Striped)
                .AddClass("vek-table-outlined", Outlined)
                .AddClass("vek-table-square", Square)
                .AddClass("vek-table-sticky-header", FixedHeader)
                .AddClass("vek-table-sticky-footer", FixedFooter)
                .AddClass($"vek-elevation-{Elevation}", !Outlined)
                .AddClass(Class)
                .Build();

        protected string Stylename =>
            new StyleBuilder()
                .AddStyle("overflow-x", "auto", when: HorizontalScrollbar || ColumnResizeMode == ResizeMode.Container)
                .AddStyle("position", "relative", when: HasStickyColumns)
                .AddStyle(Style)
                .Build();

        protected string TableStyle =>
            new StyleBuilder()
                .AddStyle("height", Height, !string.IsNullOrWhiteSpace(Height))
                .AddStyle("display", "block", when: HorizontalScrollbar)
                .Build();

        protected string TableRootStyle =>
            new StyleBuilder()
                .AddStyle("width", "max-content", when: HorizontalScrollbar || ColumnResizeMode == ResizeMode.Container)
                .AddStyle("min-width", "100%", when: HorizontalScrollbar || ColumnResizeMode == ResizeMode.Container)
                .Build();

        protected string TableClass =>
            new CssBuilder("vek-table-container")
                .AddClass("cursor-col-resize", when: IsResizing)
                .AddClass("overflow-visible-table", when: ColumnReorderMode == DataGridDragAndDropColumnReorderMode.Insert)
                .Build();

        protected string HeadClassname =>
            new CssBuilder("vek-table-head")
                .AddClass(HeaderClass)
                .AddClass("vek-table-dense", Dense)
                .Build();

        protected string FootClassname =>
            new CssBuilder("vek-table-foot")
                .AddClass(FooterClass).Build();

        protected string BodyClassname =>
            new CssBuilder("vek-table-body")
                .AddClass("vek-table-body-virtualized", Virtualize)
                .Build();

        protected string HeaderFooterStyle =>
            new StyleBuilder()
                .AddStyle("position", "sticky", when: HasStickyColumns)
                .AddStyle("left", "0px", when: HasStickyColumns)
                .Build();

        protected string ColumnsPanelClassname =>
            new CssBuilder()
                .AddClass("vek-data-grid-columns-panel")
                .AddClass("px-2")
                .AddClass("vek-popover-position-override")
                .AddClass("vek-data-grid-columns-panel-dense", when: Dense)
                .Build();

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            UpdateStickyColumnOffsets();
            if (Items != null)
            {
                if (ServerData != null)
                {
                    throw new InvalidOperationException(
                        $"{GetType()} can only accept one item source from its parameters. " +
                        $"Do not supply both '{nameof(Items)}' and '{nameof(ServerData)}'.");
                }
                if (VirtualizeServerData != null)
                {
                    throw new InvalidOperationException(
                        $"{GetType()} can only accept one item source from its parameters. " +
                        $"Do not supply both '{nameof(Items)}' and '{nameof(VirtualizeServerData)}'.");
                }
                return;
            }

            if (VirtualizeServerData != null)
            {
                if (ServerData != null)
                {
                    throw new InvalidOperationException(
                        $"{GetType()} can only accept one item source from its parameters. " +
                        $"Do not supply both '{nameof(VirtualizeServerData)}' and '{nameof(ServerData)}'.");
                }
                if (QuickFilter != null)
                {
                    throw new InvalidOperationException(
                        $"Do not supply both '{nameof(VirtualizeServerData)}' and '{nameof(QuickFilter)}'.");
                }
                return;
            }

            if (ServerData != null && QuickFilter != null)
            {
                throw new InvalidOperationException(
                    $"Do not supply both '{nameof(ServerData)}' and '{nameof(QuickFilter)}'.");
            }
        }

        internal SortDirection GetColumnSortDirection(string? columnName)
        {
            if (columnName == null)
            {
                return SortDirection.None;
            }

            if (SortDefinitions.TryGetValue(columnName, out var sortDefinition))
            {
                return sortDefinition.Descending ? SortDirection.Descending : SortDirection.Ascending;
            }

            return SortDirection.None;
        }

        protected int numPages
        {
            get
            {
                if (HasServerData)
                    return (int)Math.Ceiling(_serverData.TotalItems / (double)RowsPerPage);

                return (int)Math.Ceiling(FilteredItems.Count() / (double)RowsPerPage);
            }
        }

        internal static bool RenderedColumnsItemsSelector(Column<T> item, string dropZone) => item?.PropertyName == dropZone;

        private static void Move<TItem>(List<TItem> list, int fromIndex, int toIndex, int positionModifier)
        {
            if (fromIndex == toIndex)
            {
                return;
            }

            var item = list[fromIndex];
            list.RemoveAt(fromIndex);

            if (fromIndex < toIndex)
            {
                toIndex--;
            }

            list.Insert(toIndex + positionModifier, item);
        }

        private static void Swap<TItem>(List<TItem> list, int indexA, int indexB)
        {
            (list[indexB], list[indexA]) = (list[indexA], list[indexB]);
        }

        private Task ItemUpdatedAsync(VekItemDropInfo<Column<T>> dropItem)
        {
            Debug.Assert(dropItem.Item is not null);
            dropItem.Item.Identifier = dropItem.DropzoneIdentifier;

            var dragAndDropSource = RenderedColumns.SingleOrDefault(rc => rc.PropertyName == dropItem.Item?.PropertyName);
            var destinationPropertyName = dropItem.DropzoneIdentifier;
            if (ColumnReorderMode == DataGridDragAndDropColumnReorderMode.Insert)
            {
                if (dropItem.DropzoneIdentifier.StartsWith(PrePrefix, StringComparison.Ordinal))
                {
                    destinationPropertyName = dropItem.DropzoneIdentifier[PrePrefix.Length..];
                }
                else if (dropItem.DropzoneIdentifier.StartsWith(PostPrefix, StringComparison.Ordinal))
                {
                    destinationPropertyName = dropItem.DropzoneIdentifier[PostPrefix.Length..];
                }
            }

            var dragAndDropDestination = RenderedColumns.SingleOrDefault(rc => rc.PropertyName == destinationPropertyName);
            if (dragAndDropSource != null && dragAndDropDestination != null)
            {
                var dragAndDropSourceIndex = RenderedColumns.IndexOf(dragAndDropSource);
                var dragAndDropDestinationIndex = RenderedColumns.IndexOf(dragAndDropDestination);

                if (ColumnReorderMode == DataGridDragAndDropColumnReorderMode.Insert)
                {
                    var position = dropItem.DropzoneIdentifier.StartsWith(PostPrefix, StringComparison.Ordinal) ? 1 : 0;
                    Move(RenderedColumns, dragAndDropSourceIndex, dragAndDropDestinationIndex, position);
                }
                else
                {
                    Swap(RenderedColumns, dragAndDropSourceIndex, dragAndDropDestinationIndex);
                }

                Debug.Assert(dragAndDropSource.HeaderCell is not null);
                Debug.Assert(dragAndDropDestination.HeaderCell is not null);

                if (DragPinningEnabled)
                {
                    UpdateColumnPinningAfterDrop(dragAndDropSource);
                }

                UpdateStickyColumnOffsets();
                StateHasChanged();
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// The columns currently being displayed.
        /// </summary>
        public readonly List<Column<T>> RenderedColumns = new List<Column<T>>();

        internal T? _editingItem;

        internal T? _editingSourceItem;

        private T? _newRowItem;
        private bool _isCreatingNewRow;

        internal bool HasNewRow => _isCreatingNewRow;
        internal T? NewRowItem => _newRowItem;
        internal bool IsNewRowAtTop => NewRowPosition == global::Vek.NewRowPosition.Top;
        internal bool IsNewRowAtBottom => NewRowPosition == global::Vek.NewRowPosition.Bottom;

        internal T? _previousEditingItem;
        internal bool _isEditFormOpen;

        // converters
        private readonly IConverter<bool, bool?> _oppositeBoolConverter = Conversions.From(value => !value, (bool? value) => !value ?? true);

        #region Notify Children Delegates

        internal Action<Dictionary<string, SortDefinition<T>>, HashSet<string>?>? SortChangedEvent { get; set; }
        internal Action? StartedEditingItemEvent { get; set; }
        internal Action? EditingCanceledEvent { get; set; }

        /// <summary>
        /// Occurs when the pager state has changed.
        /// </summary>
        public Action? PagerStateHasChangedEvent { get; set; }

        #endregion

        #region EventCallbacks

        /// <summary>
        /// Occurs when the <see cref="SortDefinitions"/> have changed.
        /// </summary>
        /// <remarks>
        /// This callback is raised when the grid's sort state is updated.
        /// When <see cref="ServerData"/> or <see cref="VirtualizeServerData"/> is used,
        /// it is invoked before the data reload triggered by the new sort completes.
        /// </remarks>        
        [Parameter]
        public EventCallback<Dictionary<string, SortDefinition<T>>> SortChanged { get; set; }

        /// <summary>
        /// Occurs when the <see cref="SelectedItem"/> has changed.
        /// </summary>
        /// <remarks>
        /// This typically occurs when a row has been clicked.
        /// </remarks>
        [Parameter]
        public EventCallback<T?> SelectedItemChanged { get; set; }

        /// <summary>
        /// Occurs when the <see cref="SelectedItems"/> have changed.
        /// </summary>
        /// <remarks>
        /// This typically occurs when one or more rows have been clicked when <see cref="MultiSelection"/> is <c>true</c>.
        /// </remarks>
        [Parameter]
        public EventCallback<HashSet<T>?> SelectedItemsChanged { get; set; }

        /// <summary>
        /// Occurs when a row has been clicked.
        /// </summary>
        [Parameter]
        public EventCallback<DataGridRowClickEventArgs<T>> RowClick { get; set; }

        /// <summary>
        /// Occurs when a row has been right-clicked.
        /// </summary>
        [Parameter]
        public EventCallback<DataGridRowClickEventArgs<T>> RowContextMenuClick { get; set; }

        /// <summary>
        /// Occurs when a cell has been clicked.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Selecting)]
        public EventCallback<DataGridCellClickEventArgs<T>> CellClick { get; set; }

        /// <summary>
        /// Occurs when a cell has been right-clicked.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Selecting)]
        public EventCallback<DataGridCellClickEventArgs<T>> CellContextMenuClick { get; set; }

        /// <summary>
        /// Occurs when a context menu is requested via right-click on a header, row, cell, or footer.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public EventCallback<DataGridContextMenuArgs<T>> OnContextMenu { get; set; }

        /// <summary>
        /// Occurs when edit mode begins for an item.
        /// </summary>
        /// <remarks>
        /// Use this event to react when editing starts. For form field changes, use <see cref="FormFieldChanged"/>.
        /// For save and cancel notifications, use <see cref="CommittedItemChanges"/>, <see cref="CommittedItemChanged"/>,
        /// or <see cref="CanceledEditingItem"/>.
        /// </remarks>
        [Parameter]
        public EventCallback<T> StartedEditingItem { get; set; }

        /// <summary>
        /// Occurs when editing of an item has been canceled.
        /// </summary>
        /// <remarks>
        /// This event is raised instead of <see cref="CommittedItemChanges"/> and <see cref="CommittedItemChanged"/>
        /// when the edit operation is canceled.
        /// </remarks>
        [Parameter]
        public EventCallback<T> CanceledEditingItem { get; set; }

        /// <summary>
        /// Occurs when a new row is started and its initial item is ready for editing.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public EventCallback<T> OnRowCreating { get; set; }

        /// <summary>
        /// Occurs after a new row has passed validation and is ready to be added to the data source.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public EventCallback<T> OnRowCreated { get; set; }

        /// <summary>
        /// Invoked when the user saves changes to an item, allowing validation, persistence, or other pre-commit work.
        /// </summary>
        /// <remarks>
        /// In <see cref="DataGridEditMode.Form"/>, this callback receives the edited copy before values are applied to
        /// the source item. Return <see cref="DataGridEditFormAction.Close"/> to continue or
        /// <see cref="DataGridEditFormAction.KeepOpen"/> to leave the form open for corrections.
        /// Use <see cref="CommittedItemChanged"/> to react after the source item has been updated.
        /// </remarks>
        [Parameter]
        public Func<T, Task<DataGridEditFormAction>>? CommittedItemChanges { get; set; }

        /// <summary>
        /// Occurs after committed changes have been applied to the underlying item.
        /// </summary>
        /// <remarks>
        /// Use this callback for post-commit logic. In <see cref="DataGridEditMode.Form"/>, it runs after
        /// <see cref="CommittedItemChanges"/> completes and after values are copied to the source item.
        /// In <see cref="DataGridEditMode.Cell"/>, it runs after <see cref="CommittedItemChanges"/> completes,
        /// with the already-updated item.
        /// </remarks>
        [Parameter]
        public EventCallback<T> CommittedItemChanged { get; set; }

        /// <summary>
        /// Occurs when a field changes in the edit dialog.
        /// </summary>
        /// <remarks>
        /// This event only occurs in <see cref="DataGridEditMode.Form"/> and fires for individual field changes before save.
        /// </remarks>
        [Parameter]
        public EventCallback<FormFieldChangedEventArgs> FormFieldChanged { get; set; }

        /// <summary>
        /// Occurs when hierarchy visibility toggled.
        /// </summary>
        [Parameter]
        public EventCallback<DataGridHierarchyVisibilityToggledEventArgs<T>> HierarchyVisibilityToggled { get; set; }

        /// <summary>
        /// Occurs when the active filters have changed.
        /// </summary>
        /// <remarks>
        /// This callback receives the current active filters after the change is applied.
        /// </remarks>
        [Parameter]
        public EventCallback<IReadOnlyCollection<IFilterDefinition<T>>> FilterChanged { get; set; }

        #endregion

        #region Parameters
        /// <summary>
        /// Shows the search box in the data grid toolbar.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public bool ShowSearchBox { get; set; }

        /// <summary>
        /// The current search text used to filter rows.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public string? SearchText { get; set; }

        /// <summary>
        /// Occurs when the <see cref="SearchText"/> has changed.
        /// </summary>
        [Parameter]
        public EventCallback<string?> SearchTextChanged { get; set; }

        /// <summary>
        /// The parse mode used to split and evaluate the search text.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="DataGridSearchTextParseMode.GroupWordsByAnd"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public DataGridSearchTextParseMode SearchTextParseMode { get; set; } = DataGridSearchTextParseMode.GroupWordsByAnd;

        /// <summary>
        /// The debounce interval in milliseconds for user input in the search box.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>300</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public int SearchBoxInputDelay { get; set; } = 300;

        /// <summary>
        /// The placeholder text for the search box.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public string? SearchBoxPlaceholder { get; set; }

        /// <summary>
        /// Custom template for rendering the search box in the toolbar.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public RenderFragment<DataGridSearchBoxContext<T>>? SearchBoxTemplate { get; set; }

        /// <summary>
        /// Gets or sets the custom template used to render the column chooser dialog.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>. When set, this overrides the default <see cref="DataGridColumnChooser{T}"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public RenderFragment<DataGridColumnChooserContext<T>>? ColumnChooserTemplate { get; set; }

        /// <summary>
        /// Gets or sets the custom template used to render the visual filter builder dialog.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>. When set, this overrides the default <see cref="VekFilterBuilder{T}"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Filtering)]
        public RenderFragment<VekDataGrid<T>>? FilterBuilderTemplate { get; set; }

        /// <summary>
        /// Enables contextual right-click menus on headers, data rows, cells, and footers with standard data operations.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. When set to <c>true</c> without custom templates, built-in standard data actions are displayed.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public bool ContextMenuEnabled { get; set; }

        /// <summary>
        /// Custom template used to render the context menu for column headers.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>. When set, this overrides the default context menu when a header cell is right-clicked.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public RenderFragment<DataGridContextMenuArgs<T>>? HeaderContextMenuTemplate { get; set; }

        /// <summary>
        /// Custom template used to render the context menu for data rows.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>. When set, this overrides the default context menu when a data row is right-clicked.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public RenderFragment<DataGridContextMenuArgs<T>>? RowContextMenuTemplate { get; set; }

        /// <summary>
        /// Custom template used to render the context menu for data cells.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>. When set, this overrides the default context menu when a cell is right-clicked.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public RenderFragment<DataGridContextMenuArgs<T>>? CellContextMenuTemplate { get; set; }

        /// <summary>
        /// Custom template used to render the context menu for footer cells.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>. When set, this overrides the default context menu when a footer cell is right-clicked.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public RenderFragment<DataGridContextMenuArgs<T>>? FooterContextMenuTemplate { get; set; }

        /// <summary>
        /// Default custom template used when a more specific template is not set.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public RenderFragment<DataGridContextMenuArgs<T>>? ContextMenuTemplate { get; set; }

        /// <summary>
        /// Whether matching search terms are highlighted in searchable cells.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public bool HighlightSearchResults { get; set; } = true;

        /// <summary>
        /// Attributes for the HTML table element, such as <c>aria-label</c> and <c>aria-labelledby</c>.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public Dictionary<string, object?> TableAttributes { get; set; } = [];

        /// <summary>
        /// Allows columns to be reordered via the columns panel.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        public bool ColumnsPanelReordering { get; set; } = false;

        [CascadingParameter(Name = "RightToLeft")]
        private bool RightToLeft { get; set; }

        /// <summary>
        /// Allows columns to be reordered via drag-and-drop.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  Can be overridden for individual columns via <see cref="Column{T}.DragAndDropEnabled"/>.
        /// </remarks>
        [Parameter]
        public bool DragDropColumnReordering { get; set; } = false;

        /// <summary>
        /// Allows rows to be reordered with a <see cref="DataGridDragHandleColumn{T}"/>.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public bool AllowRowReorder { get; set; }

        /// <summary>
        /// Occurs when a row is dropped at a new insertion position.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public EventCallback<DataGridRowReorderedEventArgs<T>> OnRowReordered { get; set; }

        /// <summary>
        /// The icon displayed when hovering over a draggable column.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.DragIndicator"/>.  Use the <see cref="DragIndicatorSize"/> property to control this icon's size.
        /// </remarks>
        [Parameter]
        public string DragIndicatorIcon { get; set; } = Icons.Material.Filled.DragIndicator;

        /// <summary>
        /// The size of the icon displayed when hovering over a draggable column.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Size.Small"/>.  Use the <see cref="DragIndicatorIcon"/> property to control which icon is displayed.
        /// </remarks>
        [Parameter]
        public Size DragIndicatorSize { get; set; } = Size.Small;

        /// <summary>
        /// The CSS class applied to columns where a dragged column can be dropped.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>drop-allowed</c>.
        /// </remarks>
        [Parameter]
        public string DropAllowedClass { get; set; } = "drop-allowed";

        /// <summary>
        /// The CSS class applied to columns where a dragged column cannot be dropped.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>drop-not-allowed</c>.
        /// </remarks>
        [Parameter]
        public string DropNotAllowedClass { get; set; } = "drop-not-allowed";

        /// <summary>
        /// Shows drop locations for columns even when not currently dragging a column.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        public bool ApplyDropClassesOnDragStarted { get; set; } = false;

        /// <summary>
        /// Sorts data in the grid.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="SortMode.Multiple"/>.  Can be overridden for individual columns via <see cref="Column{T}.Sortable"/>.
        /// </remarks>
        [Parameter]
        public SortMode SortMode { get; set; } = SortMode.Multiple;

        /// <summary>
        /// The icon shown when a column is sortable.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.ArrowUpward"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public string SortIcon { get; set; } = Icons.Material.Filled.ArrowUpward;

        /// <summary>
        /// Allows filtering of data in this grid.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  Can be overridden for individual columns via <see cref="Column{T}.Filterable"/>.
        /// </remarks>
        [Parameter]
        public bool Filterable { get; set; } = false;

        /// <summary>
        /// Displays a dedicated filter row beneath the column headers.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Filtering)]
        public bool ShowFilterRow { get; set; }

        /// <summary>
        /// The debounce delay in milliseconds for filter row input changes before applying the filter.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>300</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Filtering)]
        public int FilterRowInputDelay { get; set; } = 300;

        /// <summary>
        /// Allows columns to be hidden.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  Can be overridden for individual columns via <see cref="Column{T}.Hideable"/>.
        /// </remarks>
        [Parameter]
        public bool Hideable { get; set; } = false;

        /// <summary>
        /// Shows options for columns.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.  Can be overridden for individual columns via <see cref="Column{T}.ShowColumnOptions"/>.
        /// </remarks>
        [Parameter]
        public bool ShowColumnOptions { get; set; } = true;

        /// <summary>
        /// Allows columns to be pinned or fixed to the left or right edges of the grid viewport.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. Can be overridden for individual columns via <see cref="Column{T}.AllowPinning"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public bool AllowColumnPinning { get; set; } = false;

        /// <summary>
        /// Enables dynamic pinning and unpinning of columns during drag-and-drop column reordering.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>. When <c>true</c>, dragging a column into a pinned region pins it, and dragging out unpins it.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public bool DragPinningEnabled { get; set; } = true;

        /// <summary>
        /// Occurs when a column's fixed pinning state is changed via user interaction or programmatic pinning.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public EventCallback<DataGridColumnPinChangedEventArgs<T>> ColumnPinChanged { get; set; }

        /// <summary>
        /// The breakpoint at which the grid switches to mobile layout.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Breakpoint.Xs"/>.  Supported values are <c>None</c>, <c>Xs</c>, <c>Sm</c>, <c>Md</c>, <c>Lg</c> and <c>Xl</c>.
        /// </remarks>
        [Parameter]
        public Breakpoint Breakpoint { get; set; } = Breakpoint.Xs;

        /// <summary>
        /// The size of the drop shadow.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>.  A higher number creates a heavier drop shadow.  Use a value of <c>0</c> for no shadow.
        /// </remarks>
        [Parameter]
        public int Elevation { set; get; } = 1;

        /// <summary>
        /// Disables rounded corners.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        public bool Square { get; set; }

        /// <summary>
        /// Shows an outline around this grid.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        public bool Outlined { get; set; }

        /// <summary>
        /// Shows left and right borders for each column.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        public bool Bordered { get; set; }

        /// <summary>
        /// The content for any column groupings.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This property specifies a groupedColumns of one or more columns in a table for formatting.  For example:
        /// </para>
        /// <para>
        /// table
        ///     colgroup
        ///        col span="2" style="background-color:red"
        ///        col style="background-color:yellow"
        ///      colgroup
        ///      header
        ///      body
        /// table
        /// </para>
        /// </remarks>
        [Parameter]
        public RenderFragment? ColGroup { get; set; }

        /// <summary>
        /// Uses compact padding.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        public bool Dense { get; set; }

        /// <summary>
        /// Highlights rows when hovering over them.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        public bool Hover { get; set; }

        /// <summary>
        /// Shows alternating row styles.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        public bool Striped { get; set; }

        /// <summary>
        /// Fixes the header in place even as the grid is scrolled.
        /// </summary>
        /// <remarks>
        /// Set the <see cref="Height"/> property to make this grid scrollable.
        /// </remarks>
        [Parameter]
        public bool FixedHeader { get; set; }

        /// <summary>
        /// Fixes the footer in place even as the grid is scrolled.
        /// </summary>
        /// <remarks>
        /// Set the <see cref="Height"/> property to make this grid scrollable.
        /// </remarks>
        [Parameter]
        public bool FixedFooter { get; set; }

        /// <summary>
        /// Shows icons for each column filter.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.  Can be overridden for individual columns via <see cref="Column{T}.ShowFilterIcon"/>.
        /// </remarks>
        [Parameter]
        public bool ShowFilterIcons { get; set; } = true;

        /// <summary>
        /// The empty filter icon shown on a column when <see cref="Filterable"/> is <c>true</c> and no filters are applied to this column.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public string FilterIconEmpty { get; set; } = Icons.Material.Outlined.FilterAlt;

        /// <summary>
        /// The filled filter icon shown on a column when <see cref="Filterable"/> is <c>true</c> and filters are applied to this column.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public string FilterIconFilled { get; set; } = Icons.Material.Filled.FilterAlt;

        /// <summary>
        /// The clear filter icon shown on a column when <see cref="Filterable"/> is <c>true</c> to remove filters applied to this column.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public string FilterIconClear { get; set; } = Icons.Material.Filled.FilterAltOff;

        /// <summary>
        /// The icon used for column options menus in header cells.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Icons.Material.Filled.MoreVert"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public string ColumnOptionsIcon { get; set; } = Icons.Material.Filled.MoreVert;

        /// <summary>
        /// The way that this grid filters data.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="DataGridFilterMode.Simple"/>.
        /// </remarks>
        [Parameter]
        public DataGridFilterMode FilterMode { get; set; }

        /// <summary>
        /// The case sensitivity setting for columns with <c>string</c> values.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="DataGridFilterCaseSensitivity.Default"/>.
        /// </remarks>
        [Parameter]
        public DataGridFilterCaseSensitivity FilterCaseSensitivity { get; set; }

        /// <summary>
        /// The template used to display each filter.
        /// </summary>
        [Parameter]
        public RenderFragment<VekDataGrid<T>>? FilterTemplate { get; set; }

        /// <summary>
        /// The filter definitions for all columns.
        /// </summary>
        /// <remarks>
        /// When using a <see cref="FilterMode"/> of <see cref="DataGridFilterMode.Simple"/>, this property is managed automatically.
        /// </remarks>
        [Parameter]
        public List<IFilterDefinition<T>> FilterDefinitions { get; set; } = new List<IFilterDefinition<T>>();

        /// <summary>
        /// The sort definitions for all columns.
        /// </summary>
        /// <remarks>
        /// When using a <see cref="FilterMode"/> of <see cref="DataGridFilterMode.Simple"/>, this property is managed automatically.
        /// </remarks>
        [Parameter]
        public Dictionary<string, SortDefinition<T>> SortDefinitions { get; set; } = new Dictionary<string, SortDefinition<T>>();

        /// <summary>
        /// Renders only visible items instead of all items.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  Only works when <see cref="Height"/> is set.  This feature can improve performance for large data sets.
        /// </remarks>
        [Parameter]
        public bool Virtualize { get; set; }

        /// <summary>
        /// A RenderFragment that will be used as a placeholder when the Virtualize component is asynchronously loading data.
        /// This placeholder is displayed for each item in the data source that is yet to be loaded. Useful for presenting a loading indicator
        /// in a data grid row while the actual data is being fetched from the server.
        /// </summary>
        [Parameter]
        public RenderFragment? RowLoadingContent { get; set; }

        /// <summary>
        /// The number of additional items rendered outside the visible region when <see cref="Virtualize"/> is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>3</c>.  This value can reduce the amount of rendering during scrolling, but higher values can affect performance.
        /// </remarks>
        [Parameter]
        public int OverscanCount { get; set; } = 3;

        /// <summary>
        /// The height of each row, in pixels, when <see cref="Virtualize"/> is <c>true</c>.
        /// </summary>
        [Parameter]
        public float ItemSize { get; set; } = 50f;

        /// <summary>
        /// The CSS class applied to each row.
        /// </summary>
        /// <remarks>
        /// Multiple classes must be separated by spaces.  Note that some CSS settings are overridden by other styles, such as those from <see cref="VekTd"/>.
        /// </remarks>
        [Parameter]
        public string? RowClass { get; set; }

        /// <summary>
        /// The CSS styles applied to each row.
        /// </summary>
        /// <remarks>
        /// Some CSS settings are overridden by other styles, such as those from <see cref="VekTd"/>.
        /// </remarks>
        [Parameter]
        public string? RowStyle { get; set; }

        /// <summary>
        /// The function which calculates CSS classes for each row.
        /// </summary>
        /// <remarks>
        /// The function passes the current item and row index as parameters.
        /// </remarks>
        [Parameter]
        public Func<T, int, string>? RowClassFunc { get; set; }

        /// <summary>
        /// The function which calculates CSS styles for each row.
        /// </summary>
        /// <remarks>
        /// The function passes the current item and row index as parameters.
        /// </remarks>
        [Parameter] public Func<T, int, string>? RowStyleFunc { get; set; }

        /// <summary>
        /// Allows selection of more than one row.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        public bool MultiSelection { get; set; }

        /// <summary>
        /// Selects or deselects a row when it is clicked.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.  When <c>false</c>, rows are only selected through a <see cref="SelectColumn{T}"/> checkbox or in code.
        /// <see cref="RowClick"/> is raised either way, so a click can still be handled without changing <see cref="SelectedItem"/> or <see cref="SelectedItems"/>.
        /// </remarks>
        [Parameter]
        public bool SelectOnRowClick { get; set; } = true;

        /// <summary>
        /// Allows the selection to be changed by the user.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.  When <c>false</c>, <see cref="SelectedItem"/> and <see cref="SelectedItems"/> can still be set in code.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Selecting)]
        public bool SelectionChangeable { get; set; } = true;

        /// <summary>
        /// Controls how cell values are edited.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="DataGridEditMode.Cell"/>.  Only works when <see cref="ReadOnly"/> is <c>false</c>.
        /// </remarks>
        [Parameter]
        public DataGridEditMode? EditMode { get; set; }

        /// <summary>
        /// Positions the active new row before or after the data rows.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="NewRowPosition.Top"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public NewRowPosition NewRowPosition { get; set; }

        /// <summary>
        /// Creates the item used when <see cref="StartNewRowAsync"/> begins a new row.
        /// </summary>
        /// <remarks>This factory is required.</remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public Func<T>? NewItemFactory { get; set; }

        /// <summary>
        /// The behavior which begins editing a cell when <see cref="EditMode"/> is <see cref="DataGridEditMode.Form"/>.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="DataGridEditTrigger.Manual"/>.
        /// </remarks>
        [Parameter]
        public DataGridEditTrigger? EditTrigger { get; set; } = DataGridEditTrigger.Manual;

        /// <summary>
        /// Any options applied to the edit dialog when <see cref="EditMode"/> is <see cref="DataGridEditMode.Form"/>.
        /// </summary>
        [Parameter]
        public DialogOptions? EditDialogOptions { get; set; }

        /// <summary>
        /// The technique used to copy items for editing.
        /// </summary>
        /// <remarks>
        /// During edit mode, a copy of the item is edited, in order to allow an edit to be canceled.  This property controls how that copy is made.
        /// </remarks>
        [Parameter]
        public ICloneStrategy<T> CloneStrategy { get; set; } = SystemTextJsonDeepCloneStrategy<T>.Instance;

        /// <summary>
        /// The data for this grid when <see cref="ServerData"/> is not set.
        /// </summary>
        /// <remarks>
        /// One row will be displayed per item.  Use the <see cref="ServerData"/> function instead of this property to get data on demand.
        /// </remarks>
        [Parameter, ParameterState(ParameterUsage = ParameterUsageOptions.None)]
        public IEnumerable<T>? Items { get; set; }

        private void OnPagerStateChanged()
        {
            if (PagerStateHasChangedEvent is not null)
                InvokeAsync(PagerStateHasChangedEvent);
        }

        private void SetupGrouping()
        {
            if (Groupable)
                GroupItems();
        }

        private void SetupCollectionChangeTracking()
        {
            if (Items is INotifyCollectionChanged changed)
            {
                changed.CollectionChanged += (s, e) =>
                {
                    InvokeAsync(async () =>
                    {
                        _currentRenderFilteredItemsCache = null;

                        if (Groupable)
                            GroupItems();

                        ApplyInitialExpansionForNewItems(e);
                        await CleanupRemovedItemsAsync(e);
                        StateHasChanged();
                    });
                };
            }
        }

        private async Task CleanupRemovedItemsAsync(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // For reset, check all items in selection against the current items
                await CleanupStaleSelectionsAsync();
                CleanupStaleHierarchyExpansions();
                return;
            }

            if (e.OldItems is null)
                return;

            // Build a set of removed items for efficient lookup
            var removedItems = new HashSet<T>(e.OldItems.Cast<T>(), Comparer);

            // Directly check removed items against selection (more efficient for small removals)
            var (selectionChanged, selectedItemChanged) = PruneSelectionFromRemovedItems(removedItems);

            // Handle SelectedItem reset if needed
            if (selectedItemChanged)
            {
                await _selectedItemState.SetValueAsync(default);
            }

            // Clean up hierarchy expansions for removed items
            PruneHierarchyExpansions(removedItems);

            if (selectionChanged)
            {
                await FireSelectionChangedEventsAsync();
            }
        }

        private async Task CleanupStaleSelectionsAsync()
        {
            if (Selection.Count == 0 && _selectedItemState.Value is null)
                return;

            var currentItems = BuildCurrentItemsSet();
            var (selectionChanged, selectedItemChanged) = PruneSelectionAndSelectedItem(currentItems);

            if (!selectionChanged && !selectedItemChanged)
                return;

            if (selectedItemChanged)
            {
                await _selectedItemState.SetValueAsync(default);
            }

            if (selectionChanged)
            {
                await FireSelectionChangedEventsAsync();
            }
        }

        private void CleanupStaleHierarchyExpansions()
        {
            if (_openHierarchies.Count == 0 && _initialExpansions.Count == 0)
                return;

            var currentItems = BuildCurrentItemsSet();
            PruneHierarchyExpansionsFromCurrentItems(currentItems);
        }

        /// <summary>
        /// Builds a HashSet of current items from the data source.
        /// </summary>
        private HashSet<T> BuildCurrentItemsSet()
        {
            return Items is not null ? new HashSet<T>(Items, Comparer) : new HashSet<T>(Comparer);
        }

        /// <summary>
        /// Removes items from Selection that are not present in currentItems.
        /// Returns a tuple indicating whether Selection changed and whether SelectedItem needs to be reset.
        /// </summary>
        private (bool SelectionChanged, bool SelectedItemChanged) PruneSelectionAndSelectedItem(HashSet<T> currentItems)
        {
            var selectionChanged = Selection.RemoveWhere(s => !currentItems.Contains(s)) > 0;

            // Check if SelectedItem needs to be reset
            var selectedItemChanged = _selectedItemState.Value is not null && !currentItems.Contains(_selectedItemState.Value);

            return (selectionChanged, selectedItemChanged);
        }

        /// <summary>
        /// Removes items from Selection that are present in removedItems.
        /// More efficient when a small number of items are removed from a large collection.
        /// </summary>
        private (bool SelectionChanged, bool SelectedItemChanged) PruneSelectionFromRemovedItems(HashSet<T> removedItems)
        {
            var selectionChanged = false;
            foreach (var item in removedItems)
            {
                if (Selection.Remove(item))
                {
                    selectionChanged = true;
                }
            }

            // Check if SelectedItem needs to be reset
            var selectedItemChanged = _selectedItemState.Value is not null && removedItems.Contains(_selectedItemState.Value);

            return (selectionChanged, selectedItemChanged);
        }

        /// <summary>
        /// Removes items from _openHierarchies and _initialExpansions that are present in the removedItems set.
        /// Used when specific items are removed from the collection.
        /// </summary>
        private void PruneHierarchyExpansions(HashSet<T> removedItems)
        {
            _openHierarchies.ExceptWith(removedItems);
            _initialExpansions.ExceptWith(removedItems);
        }

        /// <summary>
        /// Removes items from _openHierarchies and _initialExpansions that are not present in currentItems.
        /// Used when checking against the full current items set.
        /// </summary>
        private void PruneHierarchyExpansionsFromCurrentItems(HashSet<T> currentItems)
        {
            _openHierarchies.RemoveWhere(h => !currentItems.Contains(h));
            _initialExpansions.RemoveWhere(e => !currentItems.Contains(e));
        }

        /// <summary>
        /// Publishes the current selection so bound parameters and their change callbacks see it.
        /// </summary>
        private async Task FireSelectionChangedEventsAsync()
        {
            // Create new HashSet instance to ensure ParameterState's comparer detects changes
            await _selectedItemsState.SetValueAsync(new HashSet<T>(Selection, Comparer));
        }

        private void ApplyInitialExpansionForNewItems(NotifyCollectionChangedEventArgs e)
        {
            if (_initialExpandedFunc is null || e.NewItems is null)
                return;

            foreach (T item in e.NewItems)
            {
                if (_initialExpandedFunc.Invoke(item) && _initialExpansions.Add(item))
                {
                    _openHierarchies.Add(item);
                }
            }
        }

        private void ApplyInitialExpansionForItems(IEnumerable<T>? items)
        {
            if (_initialExpandedFunc is null || items is null)
                return;

            foreach (var item in items)
            {
                if (_initialExpandedFunc.Invoke(item) && _initialExpansions.Add(item))
                {
                    _openHierarchies.Add(item);
                }
            }
        }

        /// <summary>
        /// Shows a loading animation while querying data.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  This property is <c>true</c> while the <see cref="ServerData"/> function is executing.
        /// </remarks>
        [Parameter]
        public bool Loading { get; set; }

        /// <summary>
        /// Shows a cancel button during inline editing when <see cref="EditMode"/> is <see cref="DataGridEditMode.Cell"/>.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        public bool CanCancelEdit { get; set; } = true;

        /// <summary>
        /// The color of the loading progress indicator while <see cref="Loading" /> is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="Color.Info"/>.  Theme colors are supported.
        /// </remarks>
        [Parameter]
        public Color LoadingProgressColor { get; set; } = Color.Info;

        /// <summary>
        /// Any custom content to show in this grid's toolbar.
        /// </summary>
        [Parameter]
        public RenderFragment? ToolBarContent { get; set; }

        /// <summary>
        /// Allows the toolbar's content to wrap onto multiple lines.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. See <see cref="VekToolBar.WrapContent"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public bool ToolBarWrapContent { get; set; }

        /// <summary>
        /// Shows a horizontal scrollbar.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        public bool HorizontalScrollbar { get; set; }

        /// <summary>
        /// The column resizing behavior for this grid.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="ResizeMode.None"/>.  Other values include <see cref="ResizeMode.Column"/> and <see cref="ResizeMode.Container"/>.
        /// </remarks>
        [Parameter]
        public ResizeMode ColumnResizeMode { get; set; }

        /// <summary>
        /// The CSS classes applied to the grid header.
        /// </summary>
        /// <remarks>
        /// These classes are applied to the <c>thead</c> tag of the grid.  Multiple classes must be separated by spaces.
        /// </remarks>
        [Parameter]
        public string? HeaderClass { get; set; }

        /// <summary>
        /// The height of this grid.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  Values such as <c>30%</c> and <c>500px</c> are allowed.  When <c>null</c>, the grid will try to grow in height.  Must be set when <see cref="Virtualize"/> is <c>true</c>.
        /// </remarks>
        [Parameter]
        public string? Height { get; set; }

        /// <summary>
        /// The CSS classes applied to the grid footer.
        /// </summary>
        /// <remarks>
        /// These classes are applied to the <c>tfoot</c> tag of the grid.  Multiple classes must be separated by spaces.
        /// </remarks>
        [Parameter]
        public string? FooterClass { get; set; }

        /// <summary>
        /// The function which determines visibility of each item in this grid.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  This function is typically used to implement a custom search.
        /// </remarks>
        [Parameter]
        public Func<T, bool>? QuickFilter { get; set; } = null;

        /// <summary>
        /// Any custom content for this grid's header.
        /// </summary>
        [Parameter]
        public RenderFragment? Header { get; set; }

        /// <summary>
        /// Any custom content for this grid's columns.
        /// </summary>
        [Parameter]
        public RenderFragment? Columns { get; set; }

        /// <summary>
        /// The culture used to format numeric and date values.  Can be overridden by <see cref="Column{T}.Culture"/>.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  When no culture is set, cells use the current culture via .NET's default formatting behavior.
        /// Set this to <see cref="CultureInfo.InvariantCulture"/> when invariant numeric and date formatting is required.
        /// </remarks>
        [Parameter]
        public CultureInfo? Culture { get; set; }

        /// <summary>
        /// The content shown for each cell.
        /// </summary>
        [Parameter]
        public RenderFragment<CellContext<T>>? ChildRowContent { get; set; }

        /// <summary>
        /// The custom renderer function for child row content.
        /// </summary>
        /// <remarks>
        /// This provides an alternative to <see cref="ChildRowContent"/> with more control over the rendering.
        /// </remarks>
        [Parameter]
        public RenderFragment<CellContext<T>>? ChildRowRenderer { get; set; }

        /// <summary>
        /// The content shown when there are no rows to display.
        /// </summary>
        [Parameter]
        public RenderFragment? NoRecordsContent { get; set; }

        /// <summary>
        /// The content shown while <see cref="Loading"/> is <c>true</c>.
        /// </summary>
        [Parameter]
        public RenderFragment? LoadingContent { get; set; }

        /// <summary>
        /// The content shown for pagination.
        /// </summary>
        /// <remarks>
        /// A <see cref="VekTablePager"/> is typically added here to break up rows into multiple pages.
        /// </remarks>
        [Parameter]
        public RenderFragment? PagerContent { get; set; }

        /// <summary>
        /// The function which gets data for this grid.
        /// </summary>
        /// <remarks>
        /// The function accepts a <see cref="GridState{T}"/> with current sorting, filtering, and pagination parameters.  Then, return a <see cref="GridData{T}"/> with a page of values, and the total (unpaginated) items set in <see cref="GridData{T}.TotalItems"/>.  When set, the <see cref="Items"/> property cannot be set.
        /// </remarks>
        [Parameter]
        public Func<GridState<T>, CancellationToken, Task<GridData<T>>>? ServerData { get; set; }

        /// <summary>
        /// The function which gets data for this grid.
        /// </summary>
        /// <remarks>
        /// The function accepts a <see cref="GridStateVirtualize{T}"/> with current sorting, filtering, and pagination parameters.
        /// Then, return a <see cref="GridData{T}"/> with a list of values, and the total (unpaginated) items count in <see cref="GridData{T}.TotalItems"/>.
        /// This property is used when you need to display a list without a paginator,
        /// but with loading data from the server as the scroll position changes.
        /// </remarks>
        [Parameter]
        public Func<GridStateVirtualize<T>, CancellationToken, Task<GridData<T>>>? VirtualizeServerData { get; set; }

        /// <summary>
        /// The number of rows displayed for each page.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>10</c>.  Applies when the <see cref="PagerContent"/> section contains a <see cref="VekTablePager"/>.  When this property changes, the <see cref="RowsPerPageChanged"/> event occurs.
        /// </remarks>
        [Parameter]
        public int RowsPerPage
        {
            get => _rowsPerPage ?? 10;
            set
            {
                if (_rowsPerPage == null)
                    InvokeAsync(() => SetRowsPerPageAsync(value));
            }
        }

        /// <summary>
        /// Occurs when the <see cref="RowsPerPage"/> has changed.
        /// </summary>
        [Parameter]
        public EventCallback<int> RowsPerPageChanged { get; set; }

        /// <summary>
        /// The current page being displayed.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>0</c>.  Applies when the <see cref="PagerContent"/> section contains a <see cref="VekTablePager"/>.
        /// </remarks>
        [Parameter]
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage == value)
                    return;
                _currentPage = value;
                InvokeAsync(StateHasChanged);
                CurrentPageChanged.InvokeAsync(_currentPage);

                if (_isFirstRendered)
                    InvokeAsync(InvokeServerLoadFunc);
            }
        }

        /// <summary>
        /// Occurs when <see cref="CurrentPage"/> has changed.
        /// </summary>
        [Parameter]
        public EventCallback<int> CurrentPageChanged { get; set; }

        /// <summary>
        /// Prevents values from being edited.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.  When <c>false</c>, the edit behavior is controlled via <see cref="EditMode"/>.
        /// </remarks>
        [Parameter]
        public bool ReadOnly { get; set; } = true;

        /// <summary>
        /// The currently selected rows when <see cref="MultiSelection"/> is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// This property can be bound (<c>@bind-SelectedItems</c>) to initially select rows.  Use <see cref="SelectedItem"/> when <see cref="MultiSelection"/> is <c>false</c>.
        /// To change the selection from code, assign a new set.  A change is detected by reference, so editing the bound set in place, such as calling <c>Clear()</c>, does not update the grid.
        /// </remarks>
        [Parameter, ParameterState]
        public HashSet<T>? SelectedItems { get; set; }

        /// <summary>
        /// The currently selected row when <see cref="MultiSelection"/> is <c>false</c>.
        /// </summary>
        /// <remarks>
        /// This property can be bound (<c>@bind-SelectedItem</c>) to initially select a row.  Use <see cref="SelectedItems"/> when <see cref="MultiSelection"/> is <c>true</c>.
        /// </remarks>
        [Parameter, ParameterState]
        public T? SelectedItem { get; set; }

        /// <summary>
        /// Determines whether the grid selects rows or rectangular cell ranges.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="DataGridSelectionMode.Row"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Selecting)]
        public DataGridSelectionMode SelectionMode { get; set; }

        /// <summary>
        /// The rectangular cell range currently selected by the user.
        /// </summary>
        /// <remarks>
        /// Coordinates are zero-based within the visible data rows and rendered columns. This parameter is used when <see cref="SelectionMode"/> is <see cref="DataGridSelectionMode.CellRange"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Selecting)]
        public DataGridCellRange? SelectedCellRange { get; set; }

        /// <summary>
        /// Occurs when <see cref="SelectedCellRange"/> changes.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Selecting)]
        public EventCallback<DataGridCellRange?> SelectedCellRangeChanged { get; set; }

        /// <summary>
        /// Enables dedicated row focus tracking and keyboard navigation.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. When enabled, a single row can have active focus independent of selection,
        /// navigable via keyboard arrow keys, Page Up/Down, Home, and End.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public bool FocusedRowEnabled { get; set; }

        /// <summary>
        /// The zero-based index of the currently focused row within the visible page or dataset.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>-1</c> (no row focused). Supports two-way binding (<c>@bind-FocusedRowIndex</c>).
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public int FocusedRowIndex { get; set; } = -1;

        /// <summary>
        /// Occurs when the <see cref="FocusedRowIndex"/> has changed.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public EventCallback<int> FocusedRowIndexChanged { get; set; }

        /// <summary>
        /// The data item corresponding to the currently focused row.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>default</c>. Supports two-way binding (<c>@bind-FocusedItem</c>).
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public T? FocusedItem { get; set; }

        /// <summary>
        /// Occurs when the <see cref="FocusedItem"/> has changed.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public EventCallback<T?> FocusedItemChanged { get; set; }

        /// <summary>
        /// Occurs when the focused row changes due to user interaction or programmatic update.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public EventCallback<DataGridFocusedRowChangedEventArgs<T>> FocusedRowChanged { get; set; }

        /// <summary>
        /// Automatically scrolls the focused row into view when focus changes.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>. Requires <see cref="FocusedRowEnabled"/> to be <c>true</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public bool AutoScrollToFocusedRow { get; set; } = true;

        /// <summary>
        /// Enables full WAI-ARIA Grid keyboard navigation with roving tabindex across cells.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. When enabled, arrow keys, Home/End, PageUp/PageDown navigate cells,
        /// Enter and F2 activate editing, Escape cancels active edits, Space toggles row selection,
        /// and Tab exits the grid to adjacent interactive elements.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public bool KeyboardNavigationEnabled { get; set; }

        /// <summary>
        /// The zero-based visible column index of the currently focused cell.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>-1</c>. Supports two-way binding (<c>@bind-FocusedColumnIndex</c>) when <see cref="KeyboardNavigationEnabled"/> is <c>true</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public int FocusedColumnIndex { get; set; } = -1;

        /// <summary>
        /// Occurs when <see cref="FocusedColumnIndex"/> has changed.
        /// </summary>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public EventCallback<int> FocusedColumnIndexChanged { get; set; }

        /// <summary>
        /// Allows grouping of columns in this grid.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  When <c>true</c>, columns can be used to groupedColumns sets of items.  Can be overridden for individual columns via <see cref="Column{T}.Groupable"/>.
        /// </remarks>
        [Parameter]
        public bool Groupable
        {
            get { return _groupable; }
            set
            {
                if (_groupable != value)
                {
                    _groupable = value;

                    if (!_groupable)
                    {
                        _groupDefinition = null;
                        // do not need to RemoveGrouping here, if Groupable is set to false they won't show
                    }
                }
            }
        }

        private bool _groupable = false;

        /// <summary>
        /// Expands grouped columns by default. Overrides <see cref="Column{T}.GroupExpanded" />
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. Applies when <see cref="Groupable"/> is <c>true</c>.
        /// </remarks>
        [Parameter]
        public bool GroupExpanded { get; set; }

        /// <summary>
        /// The CSS classes applied to column groups.
        /// </summary>
        /// <remarks>
        /// Applies when <see cref="Groupable"/> is <c>true</c>.  Multiple classes must be separated by spaces.
        /// </remarks>
        [Parameter]
        public string? GroupClass { get; set; }

        /// <summary>
        /// The CSS styles applied to column groups.
        /// </summary>
        /// <remarks>
        /// Applies when <see cref="Groupable"/> is <c>true</c>.
        /// </remarks>
        [Parameter]
        public string? GroupStyle { get; set; }

        /// <summary>
        /// The function which determines CSS classes for column groups.
        /// </summary>
        /// <remarks>
        /// Applies when <see cref="Groupable"/> is <c>true</c>.
        /// </remarks>
        [Parameter]
        public Func<GroupDefinition<T>, string>? GroupClassFunc { get; set; }

        /// <summary>
        /// The function which determines CSS styles for column groups.
        /// </summary>
        /// <remarks>
        /// Applies when <see cref="Groupable"/> is <c>true</c>.
        /// </remarks>
        [Parameter]
        public Func<GroupDefinition<T>, string>? GroupStyleFunc { get; set; }

        /// <summary>
        /// Shows the settings icon in the grid header.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.  When <c>true</c>, an icon will be displayed to control column visibility, collapse all columns, or expand all columns.
        /// </remarks>
        [Parameter]
        public bool ShowMenuIcon { get; set; } = false;

        /// <summary>
        /// Ensures the user can only expand one Hierarchy row at a time. This only has an effect if you are using a Hierarchy column.
        /// </summary>
        /// <remarks>Defaults to <c>false</c>.</remarks>
        [Parameter, ParameterState]
        public bool ExpandSingleRow { get; set; }

        /// <summary>
        /// The comparer used to determine row selection.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="EqualityComparer{T}.Default" />.
        /// This comparer decides whether a row is selected and whether an expanded hierarchy row is still the same row after the data is refreshed.
        /// Pass a stable instance: a change is detected by reference, so an inline <c>Comparer="@(new MyComparer())"</c> creates a fresh instance on every render and rebuilds the selection set every time.
        /// </remarks>
        [Parameter, ParameterState(ParameterUsage = ParameterUsageOptions.None)]
        public IEqualityComparer<T>? Comparer { get; set; } = EqualityComparer<T>.Default;

        /// <summary>
        /// The default template used to display column grouping for any column that is grouped.
        /// </summary>
        /// <remarks>Can be overridden by using the column level GroupTemplate, defaults to <c>null</c>.</remarks>
        [Parameter]
        public RenderFragment<GroupDefinition<T>>? GroupTemplate { get; set; }

        /// <summary>
        /// The behavior of the grid when the user reorders column headers via drag and drop.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>DataGridDragAndDropColumnReorderMode.Swap</c>.
        /// This mode applies to header drag-and-drop reordering. Reordering from the columns panel uses insert-by-index behavior.
        /// </remarks>
        [Parameter]
        public DataGridDragAndDropColumnReorderMode ColumnReorderMode { get; set; } = DataGridDragAndDropColumnReorderMode.Swap;

        #endregion

        /// <summary>
        /// Determines whether an unsorted state (<see cref="SortDirection.None"/>) is allowed when toggling sort directions.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>. When <c>false</c>, the sort direction toggles only between
        /// <see cref="SortDirection.Ascending"/> and <see cref="SortDirection.Descending"/>.
        /// When <c>true</c>, a third toggle state, <see cref="SortDirection.None"/>, is included.
        /// </remarks>
        [Parameter]
        public bool AllowUnsorted { get; set; } = false;

        #region Properties

        internal IEnumerable<T> CurrentPageItems
        {
            get
            {
                if (PagerContent == null)
                {
                    return FilteredItems; // we have no pagination
                }

                if (!HasServerData)
                {
                    var filteredItemCount = GetFilteredItemsCount();
                    int lastPageNo;
                    if (filteredItemCount == 0)
                        lastPageNo = 0;
                    else
                        lastPageNo = (filteredItemCount / RowsPerPage) - (filteredItemCount % RowsPerPage == 0 ? 1 : 0);
                    CurrentPage = lastPageNo < CurrentPage ? lastPageNo : CurrentPage;
                }

                return GetItemsOfPage(CurrentPage, RowsPerPage);
            }
        }

        /// <summary>
        /// The currently selected items.
        /// </summary>
        public HashSet<T> Selection { get; set; }

        /// <summary>
        /// Indicates if a <see cref="VekDataGridPager{T}"/> is present.
        /// </summary>
        public bool HasPager { get; set; }

        /// <summary>
        /// The items returned by the <see cref="ServerData"/> function.
        /// </summary>
        public IEnumerable<T> ServerItems => _serverData.Items;

        /// <summary>
        /// Defines the ItemsProviderDelegate property, which is necessary for implementing the ServerData methodology with Virtualization.
        /// This property is used to populate items virtually from the server.
        /// </summary>
        internal ItemsProviderDelegate<IndexBag<T>>? VirtualItemsProvider { get; set; }

        /// <summary>
        /// For unit testing the filtering cache mechanism.
        /// </summary>
        internal uint FilteringRunCount { get; private set; }

        /// <summary>
        /// The items which remain after applying filters.
        /// </summary>
        public IEnumerable<T> FilteredItems
        {
            // TODO: When adding one FilterDefinition, this is called once for each RenderedColumn...
            get
            {
                if (_currentRenderFilteredItemsCache != null) return _currentRenderFilteredItemsCache;
                var items = HasServerData
                    ? _serverData.Items
                    : Items;

                items ??= Enumerable.Empty<T>();

                // Quick filtering
                if (QuickFilter != null)
                {
                    items = items.Where(QuickFilter);
                }

                var searchTokens = CurrentSearchTokens;
                if (searchTokens.Count == 0 && !string.IsNullOrWhiteSpace(_searchTextState.Value))
                {
                    UpdateSearchTokens();
                    searchTokens = CurrentSearchTokens;
                }

                if (searchTokens.Count > 0)
                {
                    items = items.Where(item => DataGridSearchEngine.MatchesRow(item, RenderedColumns, searchTokens, _searchTextParseModeState.Value, Culture));
                }

                if (!HasServerData)
                {
                    foreach (var filterDefinition in FilterDefinitions)
                    {
                        var filterFunc = filterDefinition.GenerateFilterFunction(new FilterOptions
                        {
                            FilterCaseSensitivity = FilterCaseSensitivity
                        });
                        items = items.Where(filterFunc);
                    }
                }

                _currentRenderFilteredItemsCache = Sort(items).ToList(); // To list to ensure evaluation only once per render
                unchecked { FilteringRunCount++; }
                GroupItems(noStateChange: true);
                return _currentRenderFilteredItemsCache;
            }
        }

        /// <summary>
        /// Writes the current grid view as comma-separated values to a stream.
        /// </summary>
        /// <param name="stream">The writable destination stream.</param>
        /// <param name="options">Options that control the exported items, columns, and values.</param>
        public Task ExportToCsvAsync(Stream stream, DataGridExportOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!stream.CanWrite)
            {
                throw new ArgumentException("The stream must be writable.", nameof(stream));
            }

            options ??= new DataGridExportOptions();
            return DataGridCsvExporter.WriteAsync(stream, GetExportColumns(options), GetExportItems(options), options);
        }

        /// <summary>
        /// Writes the current grid view as an XLSX workbook to a stream.
        /// </summary>
        /// <param name="stream">The writable destination stream.</param>
        /// <param name="options">Options that control the exported items, columns, and values.</param>
        public Task ExportToXlsxAsync(Stream stream, DataGridExportOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (!stream.CanWrite)
            {
                throw new ArgumentException("The stream must be writable.", nameof(stream));
            }

            options ??= new DataGridExportOptions();
            return DataGridXlsxExporter.WriteAsync(stream, GetExportColumns(options), GetExportItems(options), options);
        }

        /// <summary>
        /// Downloads the current grid view as a comma-separated values file.
        /// </summary>
        /// <param name="fileName">The name used for the downloaded file.</param>
        /// <param name="options">Options that control the exported items, columns, and values.</param>
        public Task ExportToCsvAsync(string fileName, DataGridExportOptions? options = null)
        {
            return DownloadExportAsync(fileName, "text/csv;charset=utf-8", ExportToCsvAsync, options);
        }

        /// <summary>
        /// Downloads the current grid view as an XLSX workbook.
        /// </summary>
        /// <param name="fileName">The name used for the downloaded file.</param>
        /// <param name="options">Options that control the exported items, columns, and values.</param>
        public Task ExportToXlsxAsync(string fileName, DataGridExportOptions? options = null)
        {
            return DownloadExportAsync(fileName, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ExportToXlsxAsync, options);
        }

        private IReadOnlyList<Column<T>> GetExportColumns(DataGridExportOptions options)
        {
            return RenderedColumns
                .Where(column => options.ExportHiddenColumns || !column.HiddenState.Value)
                .ToArray();
        }

        private IEnumerable<T> GetExportItems(DataGridExportOptions options)
        {
            if (options.OnlyFilteredItems)
            {
                return FilteredItems;
            }

            return HasServerData ? _serverData.Items : Items ?? Enumerable.Empty<T>();
        }

        private async Task DownloadExportAsync(
            string fileName,
            string contentType,
            Func<Stream, DataGridExportOptions?, Task> export,
            DataGridExportOptions? options)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

            await using var stream = new MemoryStream();
            await export(stream, options);
            stream.Position = 0;

            using var streamReference = new DotNetStreamReference(stream);
            await JSRuntime.InvokeVoidAsync("vekWindow.downloadFileFromStream", fileName, contentType, streamReference);
        }

        /// <summary>
        /// The validator which validates values in each row.
        /// </summary>
        /// <remarks>
        /// Use to bind validation and touched handling to outer form.
        /// </remarks>
        [Parameter]
        public Interfaces.IForm Validator { get; set; } = new DataGridRowValidator();

        /// <summary>
        /// Returns true if the grid successfully grouped any column
        /// </summary>
        public bool IsGrouped
        {
            get
            {
                return _groupDefinition != null;
            }
        }

        #endregion

        #region Computed Properties

        internal string GetGroupIcon(bool isExpanded, bool? rtl = null)
        {
            if (rtl == null)
                rtl = RightToLeft;
            if (isExpanded)
            {
                return Icons.Material.Filled.ExpandMore;
            }
            return rtl.Value ? Icons.Material.Filled.ChevronLeft : Icons.Material.Filled.ChevronRight;
        }

        internal bool HasFooter
        {
            get
            {
                return RenderedColumns.Any(IsFooterCellDisplayable);
            }
        }

        private bool HasStickyColumns
        {
            get
            {
                return RenderedColumns.Any(x => x.IsPinnedLeft || x.IsPinnedRight);
            }
        }

        internal bool HasHierarchyColumn
        {
            get
            {
                return RenderedColumns.Any(x => x.Tag?.ToString() == "hierarchy-column");
            }
        }

        /// <summary>
        /// This property is determined by checking if the <see cref="ServerData"/> or <see cref="VirtualizeServerData"/> property is not null.
        /// </summary>
        internal bool HasServerData => ServerData != null || VirtualizeServerData != null;

        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await InvokeServerLoadFunc();
                if (HasServerData)
                    StateHasChanged();
                _isFirstRendered = true;
            }
            else
            {
                PagerStateHasChangedEvent?.Invoke();
            }

            await base.OnAfterRenderAsync(firstRender);
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            var sortModeBefore = SortMode;
            var hasSortModeParameter = parameters.TryGetValue(nameof(SortMode), out SortMode sortMode);
            await base.SetParametersAsync(parameters);

            VirtualItemsProviderInitialize();
            if (_isFirstRendered && hasSortModeParameter && sortMode != sortModeBefore)
                await ClearCurrentSortings();
        }

        private async Task OnItemsChangedAsync(ParameterChangedEventArgs<IEnumerable<T>?> args)
        {
            // Always clean up stale selections when Items is reassigned.
            // For INotifyCollectionChanged (e.g., ObservableCollection), the event handler
            // additionally handles incremental changes (add/remove without reassigning).
            await CleanupStaleSelectionsAsync();
            CleanupStaleHierarchyExpansions();

            OnPagerStateChanged();
            SetupGrouping();
            ApplyInitialExpansionForItems(args.Value);
            SetupCollectionChangeTracking();
        }

        private async Task OnSelectedItemChangedAsync(ParameterChangedEventArgs<T?> args)
        {
            if (args.Value is null)
            {
                return;
            }

            if (!MultiSelection)
            {
                Selection.Clear();
            }

            // add new item to Selection
            if (!Selection.Remove(args.Value))
            {
                Selection.Add(args.Value);
            }

            // Create new HashSet instance to ensure ParameterState's comparer detects changes
            await _selectedItemsState.SetValueAsync(new HashSet<T>(Selection, Comparer));
        }

        private void OnSelectedItemsChanged(ParameterChangedEventArgs<HashSet<T>?> args)
        {
            // Make defensive copy to avoid shared references between parent and child
            if (args.Value == null)
            {
                Selection.Clear();
            }
            else
            {
                Selection.Clear();
                Selection.UnionWith(args.Value);
            }
        }

        private async Task OnComparerChanged(ParameterChangedEventArgs<IEqualityComparer<T>?> args)
        {
            // A HashSet keeps the comparer it was constructed with, so every item-keyed set has to be rebuilt to adopt the new one.
            // These are safe to reassign because CellContext captures them by reference but is constructed fresh inside the render fragment on every render.
            var previousSelectionCount = Selection.Count;
            Selection = new HashSet<T>(Selection, args.Value);
            _openHierarchies = new HashSet<T>(_openHierarchies, args.Value);
            _initialExpansions = new HashSet<T>(_initialExpansions, args.Value);

            // A coarser comparer collapses entries that were previously distinct, which silently drops rows from the selection.
            // Publish that, otherwise the bound SelectedItems keeps contents the grid can no longer hold.
            if (Selection.Count != previousSelectionCount)
            {
                await FireSelectionChangedEventsAsync();
            }
        }

        private async Task OnExpandSingleRowChangedAsync(ParameterChangedEventArgs<bool> args)
        {
            // If user changes the ExpandSingleRow parameter, clear all open hierarchies except the first
            if (_openHierarchies.Count > 0)
            {
                var first = _openHierarchies.First();
                foreach (var item in _openHierarchies.Skip(1))
                {
                    await InvokeHierarchyVisibilityToggledAsync(new(item, false));
                }
                _openHierarchies.Clear();
                _openHierarchies.Add(first);
                await InvokeAsync(StateHasChanged);
            }
        }

        private async Task OnFocusedRowIndexChangedAsync(ParameterChangedEventArgs<int> args)
        {
            if (!_focusedRowEnabledState.Value && !_keyboardNavigationEnabledState.Value)
            {
                return;
            }

            var newIndex = args.Value;
            var visibleItems = GetVisibleRowItems();
            if (newIndex >= 0 && newIndex < visibleItems.Count)
            {
                var targetItem = visibleItems[newIndex];
                var comparer = Comparer ?? EqualityComparer<T>.Default;
                if (!comparer.Equals(_focusedItemState.Value, targetItem))
                {
                    await _focusedItemState.SetValueAsync(targetItem);
                }

                if (_autoScrollToFocusedRowState.Value)
                {
                    await ScrollToFocusedRowAsync(newIndex);
                }
            }
            else if (newIndex < 0 && _focusedItemState.Value is not null)
            {
                await _focusedItemState.SetValueAsync(default);
            }
        }

        private Task OnFocusedColumnIndexChangedAsync(ParameterChangedEventArgs<int> args)
        {
            return Task.CompletedTask;
        }

        private async Task OnFocusedItemChangedAsync(ParameterChangedEventArgs<T?> args)
        {
            if (!_focusedRowEnabledState.Value && !_keyboardNavigationEnabledState.Value)
            {
                return;
            }

            var newItem = args.Value;
            if (newItem is null)
            {
                if (_focusedRowIndexState.Value != -1)
                {
                    await _focusedRowIndexState.SetValueAsync(-1);
                }
                return;
            }

            var visibleItems = GetVisibleRowItems();
            var comparer = Comparer ?? EqualityComparer<T>.Default;
            var newIndex = -1;
            for (var i = 0; i < visibleItems.Count; i++)
            {
                if (comparer.Equals(visibleItems[i], newItem))
                {
                    newIndex = i;
                    break;
                }
            }

            if (_focusedRowIndexState.Value != newIndex)
            {
                await _focusedRowIndexState.SetValueAsync(newIndex);
            }

            if (newIndex >= 0 && _autoScrollToFocusedRowState.Value)
            {
                await ScrollToFocusedRowAsync(newIndex);
            }
        }

        /// <summary>
        /// Sets the focused row by index and optionally scrolls it into view.
        /// </summary>
        /// <param name="index">The zero-based row index to focus, or <c>-1</c> to clear focus.</param>
        /// <param name="autoScroll">Whether to scroll the newly focused row into view.</param>
        public async Task SetFocusedRowIndexAsync(int index, bool autoScroll = true)
        {
            if (!_focusedRowEnabledState.Value && !_keyboardNavigationEnabledState.Value)
            {
                return;
            }

            var visibleItems = GetVisibleRowItems();
            var count = visibleItems.Count;
            var previousIndex = _focusedRowIndexState.Value;
            var previousItem = _focusedItemState.Value;

            if (index < 0 || index >= count)
            {
                await _focusedRowIndexState.SetValueAsync(-1);
                await _focusedItemState.SetValueAsync(default);

                if (previousIndex != -1 || previousItem is not null)
                {
                    await FocusedRowChanged.InvokeAsync(new DataGridFocusedRowChangedEventArgs<T>(default, -1, previousItem, previousIndex));
                }

                await InvokeAsync(StateHasChanged);
                return;
            }

            var targetItem = visibleItems[index];
            var comparer = Comparer ?? EqualityComparer<T>.Default;

            await _focusedRowIndexState.SetValueAsync(index);
            await _focusedItemState.SetValueAsync(targetItem);

            if (previousIndex != index || !comparer.Equals(previousItem, targetItem))
            {
                await FocusedRowChanged.InvokeAsync(new DataGridFocusedRowChangedEventArgs<T>(targetItem, index, previousItem, previousIndex));
            }

            if (autoScroll && _autoScrollToFocusedRowState.Value)
            {
                await ScrollToFocusedRowAsync(index);
            }

            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Sets the focused row by item reference and optionally scrolls it into view.
        /// </summary>
        /// <param name="item">The item to focus, or <c>null</c> to clear focus.</param>
        /// <param name="autoScroll">Whether to scroll the newly focused row into view.</param>
        public async Task SetFocusedItemAsync(T? item, bool autoScroll = true)
        {
            if (!_focusedRowEnabledState.Value && !_keyboardNavigationEnabledState.Value)
            {
                return;
            }

            if (item is null)
            {
                await SetFocusedRowIndexAsync(-1, false);
                return;
            }

            var visibleItems = GetVisibleRowItems();
            var comparer = Comparer ?? EqualityComparer<T>.Default;
            var index = -1;
            for (var i = 0; i < visibleItems.Count; i++)
            {
                if (comparer.Equals(visibleItems[i], item))
                {
                    index = i;
                    break;
                }
            }

            await SetFocusedRowAsync(index, item, autoScroll);
        }

        /// <summary>
        /// Sets the focused row by both index and item reference.
        /// </summary>
        /// <param name="index">The zero-based row index.</param>
        /// <param name="item">The data item.</param>
        /// <param name="autoScroll">Whether to scroll the newly focused row into view.</param>
        public async Task SetFocusedRowAsync(int index, T? item, bool autoScroll = true)
        {
            if (!_focusedRowEnabledState.Value && !_keyboardNavigationEnabledState.Value)
            {
                return;
            }

            var previousIndex = _focusedRowIndexState.Value;
            var previousItem = _focusedItemState.Value;
            var comparer = Comparer ?? EqualityComparer<T>.Default;

            await _focusedRowIndexState.SetValueAsync(index);
            await _focusedItemState.SetValueAsync(item);

            if (previousIndex != index || !comparer.Equals(previousItem, item))
            {
                await FocusedRowChanged.InvokeAsync(new DataGridFocusedRowChangedEventArgs<T>(item, index, previousItem, previousIndex));
            }

            if (autoScroll && index >= 0 && _autoScrollToFocusedRowState.Value)
            {
                await ScrollToFocusedRowAsync(index);
            }

            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Sets the active focused cell by row and visible column indices.
        /// </summary>
        /// <param name="rowIndex">The zero-based visible row index.</param>
        /// <param name="columnIndex">The zero-based visible column index.</param>
        /// <param name="autoScroll">Whether to scroll the focused row into view.</param>
        public async Task SetFocusedCellAsync(int rowIndex, int columnIndex, bool autoScroll = true)
        {
            if (!_keyboardNavigationEnabledState.Value && !_focusedRowEnabledState.Value)
            {
                return;
            }

            var visibleItems = GetVisibleRowItems();
            var visibleColumnCount = GetVisibleColumnCount();
            if (visibleItems.Count == 0 || visibleColumnCount == 0)
            {
                return;
            }

            var targetRow = Math.Clamp(rowIndex, 0, visibleItems.Count - 1);
            var targetColumn = Math.Clamp(columnIndex, 0, visibleColumnCount - 1);
            var previousRowIndex = _focusedRowIndexState.Value;
            var previousItem = _focusedItemState.Value;
            var targetItem = visibleItems[targetRow];
            var comparer = Comparer ?? EqualityComparer<T>.Default;

            await _focusedRowIndexState.SetValueAsync(targetRow);
            await _focusedColumnIndexState.SetValueAsync(targetColumn);
            await _focusedItemState.SetValueAsync(targetItem);

            if (previousRowIndex != targetRow || !comparer.Equals(previousItem, targetItem))
            {
                await FocusedRowChanged.InvokeAsync(new DataGridFocusedRowChangedEventArgs<T>(targetItem, targetRow, previousItem, previousRowIndex));
            }

            if (autoScroll && _autoScrollToFocusedRowState.Value)
            {
                await ScrollToFocusedRowAsync(targetRow);
            }

            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Checks whether the cell at the specified visible row and column coordinates currently holds keyboard focus.
        /// </summary>
        /// <param name="rowIndex">The zero-based visible row index.</param>
        /// <param name="columnIndex">The zero-based visible column index.</param>
        /// <returns><c>true</c> if the cell is focused; otherwise, <c>false</c>.</returns>
        public bool IsCellFocused(int rowIndex, int columnIndex)
        {
            if (!_keyboardNavigationEnabledState.Value)
            {
                return false;
            }

            var activeRow = GetEffectiveFocusedRowIndex();
            var activeColumn = GetEffectiveFocusedColumnIndex();
            return activeRow >= 0 && activeColumn >= 0 && rowIndex == activeRow && columnIndex == activeColumn;
        }

        internal int GetVisibleColumnCount()
        {
            var count = 0;
            foreach (var column in RenderedColumns)
            {
                if (!column.HiddenState.Value)
                {
                    count++;
                }
            }

            return count;
        }

        internal int GetEffectiveFocusedRowIndex()
        {
            var count = GetVisibleRowItems().Count;
            if (count == 0)
            {
                return -1;
            }

            var index = _focusedRowIndexState.Value;
            return index < 0 ? 0 : Math.Clamp(index, 0, count - 1);
        }

        internal int GetEffectiveFocusedColumnIndex()
        {
            var count = GetVisibleColumnCount();
            if (count == 0)
            {
                return -1;
            }

            var index = _focusedColumnIndexState.Value;
            return index < 0 ? 0 : Math.Clamp(index, 0, count - 1);
        }

        internal string GetCellId(int rowIndex, int columnIndex) => $"{_gridId}_cell_{rowIndex}_{columnIndex}";

        internal string? GetCellAriaSelected(T item, bool cellSelected)
        {
            if (_selectionModeState.Value == DataGridSelectionMode.CellRange)
            {
                return cellSelected ? "true" : "false";
            }

            if (_keyboardNavigationEnabledState.Value &&
                GetSelectColumn() is null &&
                !IsRowSelectionDisabled(item) &&
                (Selection.Contains(item) || (SelectionChangeable && (SelectOnRowClick || MultiSelection))))
            {
                return Selection.Contains(item) ? "true" : "false";
            }

            return null;
        }

        internal Task NotifyStateChangedAsync() => InvokeAsync(StateHasChanged);

        /// <summary>
        /// Checks whether the row representing the given item and index is currently focused.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <param name="index">The row index to test.</param>
        /// <returns><c>true</c> if the row is focused; otherwise, <c>false</c>.</returns>
        public bool IsRowFocused(T? item, int index)
        {
            if (!_focusedRowEnabledState.Value)
            {
                return false;
            }

            if (_focusedRowIndexState.Value >= 0 && _focusedRowIndexState.Value == index && !IsGrouped)
            {
                return true;
            }

            if (_focusedItemState.Value is not null && item is not null)
            {
                var comparer = Comparer ?? EqualityComparer<T>.Default;
                return comparer.Equals(_focusedItemState.Value, item);
            }

            return false;
        }

        /// <summary>
        /// Checks whether the specified item is currently focused.
        /// </summary>
        /// <param name="item">The item to test.</param>
        /// <returns><c>true</c> if the item is focused; otherwise, <c>false</c>.</returns>
        public bool IsRowFocused(T? item)
        {
            if (!_focusedRowEnabledState.Value || item is null || _focusedItemState.Value is null)
            {
                return false;
            }

            var comparer = Comparer ?? EqualityComparer<T>.Default;
            return comparer.Equals(_focusedItemState.Value, item);
        }

        /// <summary>
        /// Checks whether the specified visible row index is currently focused.
        /// </summary>
        /// <param name="index">The row index to test.</param>
        /// <returns><c>true</c> if the index is focused; otherwise, <c>false</c>.</returns>
        public bool IsIndexFocused(int index)
        {
            return _focusedRowEnabledState.Value && index >= 0 && _focusedRowIndexState.Value == index;
        }

        internal string GetRowId(int index) => $"{_gridId}_row_{index}";

        internal IReadOnlyList<T> GetVisibleRowItems()
        {
            if (CurrentPageItems is IReadOnlyList<T> list)
            {
                return list;
            }

            return CurrentPageItems.ToList();
        }

        internal bool IsRowReorderingEnabled
            => _allowRowReorderState.Value && OnRowReordered.HasDelegate && !IsGrouped && HasRowReorderColumn;

        private bool HasRowReorderColumn
            => RenderedColumns.Any(column => column.Tag?.ToString() == "row-reorder-column");

        internal bool IsRowReorderSource(T item)
            => _rowDragManager.IsSource(item, GetRowComparer());

        internal bool IsRowReorderTarget(T item)
            => _rowDragManager.IsTarget(item, GetRowComparer());

        internal bool IsRowReorderAfterTarget => _rowDragManager.InsertAfterTarget;

        internal void BeginRowReorder(T item)
        {
            if (!IsRowReorderingEnabled)
            {
                return;
            }

            _rowDragManager.Start(item, GetVisibleRowItems(), GetRowComparer(), keyboard: false);
            StateHasChanged();
        }

        internal void CancelRowReorder()
        {
            if (!_rowDragManager.IsDragging)
            {
                return;
            }

            _rowDragManager.Cancel();
            StateHasChanged();
        }

        internal void UpdateRowReorderTarget(T item, double offsetY)
        {
            if (!IsRowReorderingEnabled)
            {
                return;
            }

            var items = GetVisibleRowItems();
            if (_rowDragManager.MoveTo(item, offsetY >= 24, items, GetRowComparer()))
            {
                StateHasChanged();
            }
        }

        internal async Task CompleteRowReorderAsync(T item, DragEventArgs args)
        {
            if (!IsRowReorderingEnabled)
            {
                CancelRowReorder();
                return;
            }

            var items = GetVisibleRowItems();
            _rowDragManager.MoveTo(item, args.OffsetY >= 24, items, GetRowComparer());
            var reorderArgs = _rowDragManager.Complete(items, GetRowComparer());
            StateHasChanged();
            if (reorderArgs is not null)
            {
                await OnRowReordered.InvokeAsync(reorderArgs);
            }
        }

        internal async Task OnRowReorderHandleKeyDownAsync(KeyboardEventArgs args, T item)
        {
            if (!IsRowReorderingEnabled)
            {
                return;
            }

            var items = GetVisibleRowItems();
            var comparer = GetRowComparer();
            if ((args.Key is " " or "Space") && !_rowDragManager.IsDragging)
            {
                _rowDragManager.Start(item, items, comparer, keyboard: true);
                StateHasChanged();
                return;
            }

            if (!_rowDragManager.IsKeyboardDrag)
            {
                return;
            }

            if (args.Key is "ArrowUp" or "ArrowDown")
            {
                if (_rowDragManager.MoveKeyboard(args.Key == "ArrowUp" ? -1 : 1, items))
                {
                    StateHasChanged();
                }

                return;
            }

            if (args.Key == "Escape")
            {
                CancelRowReorder();
                return;
            }

            if (args.Key == "Enter")
            {
                var reorderArgs = _rowDragManager.Complete(items, comparer);
                StateHasChanged();
                if (reorderArgs is not null)
                {
                    await OnRowReordered.InvokeAsync(reorderArgs);
                }
            }
        }

        private IEqualityComparer<T> GetRowComparer()
            => Comparer ?? EqualityComparer<T>.Default;

        internal async Task ScrollToFocusedRowAsync(int index)
        {
            if (index < 0 || !_autoScrollToFocusedRowState.Value || (!_focusedRowEnabledState.Value && !_keyboardNavigationEnabledState.Value))
            {
                return;
            }

            try
            {
                var rowId = GetRowId(index);
                if (Virtualize)
                {
                    await ScrollManager.ScrollToVirtualizedItemAsync(_gridId, index, ItemSize, rowId, ScrollBehavior.Auto);
                }
                else
                {
                    await ScrollManager.ScrollIntoViewAsync($"#{rowId}", ScrollBehavior.Auto);
                }
            }
            catch
            {
                // ScrollManager operations may not be supported during prerendering or headless testing.
            }
        }

        internal async Task OnKeyDownAsync(KeyboardEventArgs args)
        {
            if (_selectionModeState.Value == DataGridSelectionMode.CellRange && args.ShiftKey)
            {
                var rowDelta = args.Key switch
                {
                    "ArrowDown" => 1,
                    "ArrowUp" => -1,
                    _ => 0
                };
                var columnDelta = args.Key switch
                {
                    "ArrowRight" => 1,
                    "ArrowLeft" => -1,
                    _ => 0
                };

                if (rowDelta != 0 || columnDelta != 0)
                {
                    var visibleRows = GetVisibleRowItems();
                    var visibleColumns = RenderedColumns.Count;
                    if (_cellSelectionModel.Range is null && visibleRows.Count > 0 && visibleColumns > 0)
                    {
                        _cellSelectionModel.BeginAt(Math.Clamp(_focusedRowIndexState.Value, 0, visibleRows.Count - 1), 0);
                    }

                    if (_cellSelectionModel.MoveActiveCell(rowDelta, columnDelta, visibleRows.Count, visibleColumns) is { } range)
                    {
                        await _selectedCellRangeState.SetValueAsync(range);
                        await InvokeAsync(StateHasChanged);
                    }

                    return;
                }
            }

            if (_keyboardNavigationEnabledState.Value && await KeyboardHandler.HandleKeyDownAsync(args))
            {
                return;
            }

            if (_focusedRowEnabledState.Value)
            {
                await FocusManager.HandleKeyDownAsync(args);
            }
        }

        internal async Task OnCellMouseDownAsync(MouseEventArgs args, int rowIndex, int columnIndex)
        {
            if (_selectionModeState.Value != DataGridSelectionMode.CellRange)
            {
                return;
            }

            _isDraggingCellRange = true;
            _cellRangePointerGesture = true;
            var range = _cellSelectionModel.SetCell(rowIndex, columnIndex, args.ShiftKey);
            await _selectedCellRangeState.SetValueAsync(range);
        }

        internal async Task OnCellMouseEnterAsync(int rowIndex, int columnIndex)
        {
            if (!_isDraggingCellRange || _selectionModeState.Value != DataGridSelectionMode.CellRange)
            {
                return;
            }

            var range = _cellSelectionModel.SetCell(rowIndex, columnIndex, extend: true);
            await _selectedCellRangeState.SetValueAsync(range);
        }

        internal void OnCellMouseUp()
        {
            _isDraggingCellRange = false;
        }

        private EventCallback<MouseEventArgs> GetCellMouseDownCallback(int rowIndex, int columnIndex)
            => _selectionModeState.Value == DataGridSelectionMode.CellRange
                ? EventCallback.Factory.Create<MouseEventArgs>(this, args => OnCellMouseDownAsync(args, rowIndex, columnIndex))
                : default;

        private EventCallback<MouseEventArgs> GetCellMouseEnterCallback(int rowIndex, int columnIndex)
            => _selectionModeState.Value == DataGridSelectionMode.CellRange
                ? EventCallback.Factory.Create<MouseEventArgs>(this, () => OnCellMouseEnterAsync(rowIndex, columnIndex))
                : default;

        private EventCallback<MouseEventArgs> GetCellMouseUpCallback()
            => _selectionModeState.Value == DataGridSelectionMode.CellRange
                ? EventCallback.Factory.Create<MouseEventArgs>(this, OnCellMouseUp)
                : default;

        private Task OnSelectedCellRangeChangedAsync(ParameterChangedEventArgs<DataGridCellRange?> args)
        {
            _cellSelectionModel.SetRange(args.Value);
            _isDraggingCellRange = false;
            return Task.CompletedTask;
        }

        internal bool IsCellSelected(int rowIndex, int columnIndex)
        {
            return _selectionModeState.Value == DataGridSelectionMode.CellRange &&
                   _selectedCellRangeState.Value?.Contains(rowIndex, columnIndex) == true;
        }

        internal bool IsActiveCell(int rowIndex, int columnIndex)
        {
            return _selectionModeState.Value == DataGridSelectionMode.CellRange && _cellSelectionModel.IsActive(rowIndex, columnIndex);
        }

        internal string? ActiveCellId
        {
            get
            {
                if (_selectionModeState.Value == DataGridSelectionMode.CellRange && _cellSelectionModel.Range is { } range)
                {
                    return GetCellId(range.EndRow, range.EndColumn);
                }

                if (_keyboardNavigationEnabledState.Value)
                {
                    var activeRow = GetEffectiveFocusedRowIndex();
                    var activeColumn = GetEffectiveFocusedColumnIndex();
                    if (activeRow >= 0 && activeColumn >= 0)
                    {
                        return GetCellId(activeRow, activeColumn);
                    }
                }

                return null;
            }
        }

        internal (double Sum, double Average, int NumericCount, int CellCount) GetCellRangeSummary()
        {
            if (_selectionModeState.Value != DataGridSelectionMode.CellRange || _selectedCellRangeState.Value is not { } range)
            {
                return (0, 0, 0, 0);
            }

            var rows = GetVisibleRowItems();
            var columns = RenderedColumns;
            var sum = 0d;
            var numericCount = 0;
            for (var rowIndex = range.MinRow; rowIndex <= range.MaxRow && rowIndex < rows.Count; rowIndex++)
            {
                if (rowIndex < 0)
                {
                    continue;
                }

                for (var columnIndex = range.MinColumn; columnIndex <= range.MaxColumn && columnIndex < columns.Count; columnIndex++)
                {
                    if (columnIndex < 0 || !TypeIdentifier.IsNumber(columns[columnIndex].PropertyType))
                    {
                        continue;
                    }

                    var value = columns[columnIndex].PropertyFunc(rows[rowIndex]);
                    if (value is null)
                    {
                        continue;
                    }

                    try
                    {
                        sum += Convert.ToDouble(value, CultureInfo.InvariantCulture);
                        numericCount++;
                    }
                    catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException)
                    {
                        // Non-convertible numeric values do not contribute to the aggregate.
                    }
                }
            }

            return (sum, numericCount == 0 ? 0 : sum / numericCount, numericCount, range.CellCount);
        }

        /// <summary>
        /// Check if a specific Footer cell is displayable
        /// </summary>
        /// <param name="column"></param>
        /// <returns><see langword="true"/> when the cell can be displayed</returns>
        private static bool IsFooterCellDisplayable(Column<T> column)
        {
            return !column.HiddenState.Value && (column.GetFooterTemplate() != null || column.AggregateDefinition != null);
        }

        protected IEnumerable<T> GetItemsOfPage(int page, int pageSize)
        {
            if (page < 0 || pageSize <= 0)
                return Array.Empty<T>();

            if (HasServerData)
            {
                return _serverData.Items;
            }

            return FilteredItems.Skip(page * pageSize).Take(pageSize);
        }

        internal async Task InvokeServerLoadFunc()
        {
            if (!HasServerData)
                return;

            if (VirtualizeServerData != null)
            {
                if (_vekVirtualize != null)
                {
                    // Cancel any prior request
                    CancelServerDataToken();
                    await _vekVirtualize.RefreshDataAsync();
                }
                else
                {
                    Loading = true;
                    StateHasChanged();

                    var state = new GridStateVirtualize<T>
                    {
                        StartIndex = 0,
                        Count = 1,
                        SortDefinitions = SortDefinitions.Values.OrderBy(sd => sd.Index).ToList(),
                        // Additional ToList() here to decouple clients from internal list avoiding runtime issues
                        FilterDefinitions = FilterDefinitions.ToList()
                    };

                    // Cancel any prior request
                    var token = CancelServerDataToken();

                    GridData<T> serverData;
                    try
                    {
                        serverData = await VirtualizeServerData(state, token);
                    }
                    catch (Exception) when (token.IsCancellationRequested)
                    {
                        // A newer load cancelled this one, and it owns the data and the loading state.
                        return;
                    }

                    // Loads can finish out of order, so data from a load that a newer one replaced is stale (#13943).
                    if (token.IsCancellationRequested)
                        return;

                    _serverData = serverData;
                    _currentRenderFilteredItemsCache = null;

                    Loading = false;
                }
            }
            else
            {
                Loading = true;
                StateHasChanged();

                var state = new GridState<T>
                {
                    Page = CurrentPage,
                    PageSize = RowsPerPage,
                    SortDefinitions = SortDefinitions.Values.OrderBy(sd => sd.Index).ToList(),
                    // Additional ToList() here to decouple clients from internal list avoiding runtime issues
                    FilterDefinitions = FilterDefinitions.ToList()
                };

                // Cancel any prior request
                var token = CancelServerDataToken();

                Debug.Assert(ServerData is not null);
                GridData<T> serverData;
                try
                {
                    serverData = await ServerData(state, token);
                }
                catch (Exception) when (token.IsCancellationRequested)
                {
                    // A newer load cancelled this one, and it owns the data and the loading state.
                    // Data sources often report the cancellation as their own exception type, so the type does not matter.
                    return;
                }

                // Loads can finish out of order, so data from a load that a newer one replaced is stale (#13943).
                if (token.IsCancellationRequested)
                    return;

                _serverData = serverData;
                _currentRenderFilteredItemsCache = null;

                if (CurrentPage * RowsPerPage >= _serverData.TotalItems)
                {
                    CurrentPage = 0;

                    // The reset started a load for the first page, so that load ends the loading state and updates the pager.
                    if (token.IsCancellationRequested)
                        return;
                }

                Loading = false;
                PagerStateHasChangedEvent?.Invoke();
            }
            // handle initial hierarchy expansion
            if (_initialExpandedFunc is not null)
            {
                foreach (var data in _serverData.Items)
                {
                    // ensure we only add it once if they were expanded initially
                    if (_initialExpandedFunc(data) && _initialExpansions.Add(data))
                    {
                        _openHierarchies.Add(data);
                    }
                }
            }

            // Group before rendering: with a pager, CurrentPageItems takes the server items directly instead of FilteredItems, which would group them.
            // Rendering first showed the rows ungrouped, and the next render rebuilt every row under its group.
            GroupItems();
            StateHasChanged();
        }

        internal void AddColumn(Column<T> column)
        {
            if (column.Tag?.ToString() == "hierarchy-column")
            {
                if (column is TemplateColumn<T> templateColumn)
                {
                    _initialExpandedFunc = templateColumn.InitiallyExpandedFunc;
                    _buttonDisabledFunc = templateColumn.ButtonDisabledFunc;
                    _hierarchyColumnVisibilityToggled = templateColumn.HierarchyVisibilityToggled;
                    // Apply expansion now if items or _serverData.Items is already set
                    if (Items is not null)
                    {
                        ApplyInitialExpansionForItems(Items);
                    }
                    else if (_serverData?.Items?.Any() == true)
                    {
                        ApplyInitialExpansionForItems(_serverData.Items);
                    }
                }
                RenderedColumns.Insert(0, column);
            }
            else if (column.Tag?.ToString() == "select-column")
            {
                // Position SelectColumn after HierarchyColumn if present
                if (RenderedColumns.Select(x => x.Tag).Contains("hierarchy-column"))
                {
                    RenderedColumns.Insert(1, column);
                }
                else
                {
                    RenderedColumns.Insert(0, column);
                }
            }
            else if (column.Tag?.ToString() == "row-reorder-column")
            {
                var index = RenderedColumns.Any(x => x.Tag?.ToString() == "hierarchy-column") ? 1 : 0;
                if (index < RenderedColumns.Count && RenderedColumns[index].Tag?.ToString() == "select-column")
                {
                    index++;
                }

                RenderedColumns.Insert(index, column);
            }
            else
            {
                RenderedColumns.Add(column);
            }

            UpdateStickyColumnOffsets();
        }

        internal CancellationToken CancelServerDataToken()
        {
            // Swap in a fresh source before cancelling so the next load never observes the cancelled one.
            // Cancelling can resume a superseded load whose caller starts another load and replaces this source, so its token is read first.
            var next = new CancellationTokenSource();
            var token = next.Token;
            var previous = _serverDataCancellationTokenSource;
            _serverDataCancellationTokenSource = next;
            try
            {
                previous?.Cancel();
            }
            catch
            {
                /*ignored*/
            }
            finally
            {
                previous?.Dispose();
            }

            return token;
        }

        internal void RemoveColumn(Column<T> column)
        {
            RenderedColumns.Remove(column);
            UpdateStickyColumnOffsets();

            if (column.Tag?.ToString() == "hierarchy-column")
            {
                _hierarchyColumnVisibilityToggled = default;
                _initialExpandedFunc = null;
                _buttonDisabledFunc = null;
            }
            else if (column.Tag?.ToString() == "row-reorder-column")
            {
                _rowDragManager.Cancel();
            }
        }

        /// <summary>
        /// Adjusts each visible column to fit its rendered header and cell content.
        /// </summary>
        /// <remarks>
        /// Only rows currently rendered in the grid are measured. With virtualization enabled, this includes the rendered viewport.
        /// </remarks>
        public async Task AutoFitColumnWidths()
        {
            var columns = RenderedColumns.Where(column => !column.HiddenState.Value).ToArray();
            if (columns.Length == 0)
            {
                return;
            }

            var (success, widths) = await JSRuntime.InvokeAsyncWithErrorHandling(Array.Empty<double>(), "vekDataGrid.getColumnContentWidths", _gridElement);
            if (!success || widths.Length == 0)
            {
                return;
            }

            var visibleColumnIndex = 0;
            foreach (var column in columns)
            {
                if (visibleColumnIndex < widths.Length && widths[visibleColumnIndex] > 0 && column.HeaderCell is not null)
                {
                    await column.HeaderCell.SetAutoFitWidthAsync(widths[visibleColumnIndex]);
                }

                visibleColumnIndex++;
            }
        }

        /// <summary>
        /// Adjusts a visible column to fit its rendered header and cell content.
        /// </summary>
        /// <param name="column">The column to measure.</param>
        /// <remarks>
        /// Only rows currently rendered in the grid are measured. With virtualization enabled, this includes the rendered viewport.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="column"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException"><paramref name="column"/> does not belong to this grid.</exception>
        public async Task AutoFitColumnWidths(Column<T> column)
        {
            ArgumentNullException.ThrowIfNull(column);
            if (!RenderedColumns.Contains(column))
            {
                throw new ArgumentException("The column does not belong to this data grid.", nameof(column));
            }

            if (column.HiddenState.Value)
            {
                return;
            }

            var visibleColumnIndex = 0;
            foreach (var renderedColumn in RenderedColumns)
            {
                if (renderedColumn.HiddenState.Value)
                {
                    continue;
                }

                if (ReferenceEquals(renderedColumn, column))
                {
                    break;
                }

                visibleColumnIndex++;
            }

            var (success, widths) = await JSRuntime.InvokeAsyncWithErrorHandling(Array.Empty<double>(), "vekDataGrid.getColumnContentWidths", _gridElement);
            if (!success || visibleColumnIndex >= widths.Length || widths[visibleColumnIndex] <= 0 || column.HeaderCell is null)
            {
                return;
            }

            await column.HeaderCell.SetAutoFitWidthAsync(widths[visibleColumnIndex]);
        }

        internal IFilterDefinition<T> CreateFilterDefinitionInstance()
        {
            return _defaultFilterDefinitionFactory();
        }

        /// <summary>
        /// Creates a <see cref="FilterContext{T}"/> for a specific <see cref="IFilterDefinition{T}"/>,
        /// allowing each filter row to have its own independent context instance.
        /// </summary>
        /// <param name="column">The column associated with the filter.</param>
        /// <param name="filterDefinition">The filter definition for this context.</param>
        /// <returns>A new <see cref="FilterContext{T}"/> bound to the specified filter definition.</returns>
        internal FilterContext<T> CreateFilterContext(Column<T> column, IFilterDefinition<T> filterDefinition)
        {
            var context = new FilterContext<T>(this)
            {
                FilterDefinition = filterDefinition,
                HeaderCell = column.FilterContext.HeaderCell
            };
            context.SetActions(new FilterContext<T>.FilterActions
            {
                ApplyFilterAsync = ApplyFilterFromSimpleModeAsync,
                ApplyFiltersAsync = ApplyFiltersFromSimpleModeAsync,
                ClearFilterAsync = ClearFilterFromSimpleModeAsync,
                ClearFiltersAsync = ClearFiltersFromSimpleModeAsync,
                CloseFilterAsync = CloseFilterAsync
            });

            return context;
        }

        internal bool HasFilter(Column<T>? column)
        {
            if (column is null)
            {
                return false;
            }

            return FilterDefinitions.Any(x =>
                (ReferenceEquals(x.Column, column) || (column.PropertyName is not null && x.Column?.PropertyName == column.PropertyName)) &&
                IsFilterApplied(x));
        }

        private static bool IsFilterApplied(IFilterDefinition<T> filterDefinition)
        {
            return (filterDefinition is FilterDefinition<T> dataGridFilterDefinition && dataGridFilterDefinition.FilterFunction is not null) ||
                   filterDefinition.Value is not null ||
                   filterDefinition.Operator is FilterOperator.String.Empty or FilterOperator.String.NotEmpty or
                       FilterOperator.Number.Empty or FilterOperator.Number.NotEmpty or
                       FilterOperator.DateTime.Empty or FilterOperator.DateTime.NotEmpty;
        }

        private async Task ApplyFilterFromSimpleModeAsync(IFilterDefinition<T> filterDefinition)
        {
            if (FilterDefinitions.All(x => x.Id != filterDefinition.Id))
            {
                FilterDefinitions.Add(filterDefinition);
            }

            await InvokeServerLoadFunc();
            GroupItems();
            await NotifyFilterChangedAsync();

            if (!HasServerData)
            {
                StateHasChanged();
            }
        }

        private async Task ApplyFiltersFromSimpleModeAsync(IEnumerable<IFilterDefinition<T>> filterDefinitions)
        {
            var filterDefinitionsToApply = filterDefinitions.Where(x => FilterDefinitions.All(y => y.Id != x.Id)).ToArray();
            FilterDefinitions.AddRange(filterDefinitionsToApply);

            await InvokeServerLoadFunc();
            GroupItems();
            await NotifyFilterChangedAsync();

            if (!HasServerData)
            {
                StateHasChanged();
            }
        }

        private async Task ClearFilterFromSimpleModeAsync(IFilterDefinition<T> filterDefinition)
        {
            await RemoveFilterAsync(filterDefinition.Id);

            if (!HasServerData)
            {
                StateHasChanged();
            }
        }

        private async Task ClearFiltersFromSimpleModeAsync(IEnumerable<IFilterDefinition<T>> filterDefinitions)
        {
            FilterDefinitions.RemoveAll(x => filterDefinitions.Any(y => y.Id == x.Id));

            await InvokeServerLoadFunc();
            GroupItems();
            await NotifyFilterChangedAsync();

            if (!HasServerData)
            {
                StateHasChanged();
            }
        }

        /// <summary>
        /// Specifies the default <see cref="IFilterDefinition{T}"/> to be used by <see cref="AddFilter"/> and <see cref="Column{T}.FilterContext"/>.
        /// </summary>
        public void SetDefaultFilterDefinition<TFilterDefinition>() where TFilterDefinition : IFilterDefinition<T>, new()
        {
            SetDefaultFilterDefinition(() => new TFilterDefinition());
        }

        /// <summary>
        /// Specifies the default <see cref="IFilterDefinition{T}"/> to be used by <see cref="AddFilter"/> and <see cref="Column{T}.FilterContext"/>.
        /// </summary>
        /// <param name="factory">The factory function to create the default filter definition.</param>
        public void SetDefaultFilterDefinition(Func<IFilterDefinition<T>> factory)
        {
            _defaultFilterDefinitionFactory = factory;
        }

        /// <summary>
        /// Occurs when the "Add Filter" button is pressed.
        /// </summary>
        public void AddFilter()
        {
            var column = RenderedColumns.FirstOrDefault(x => x.filterable);
            var filterDefinition = CreateFilterDefinitionInstance();
            filterDefinition.Id = Guid.NewGuid();
            filterDefinition.Title = column?.Title;
            filterDefinition.Column = column;
            FilterDefinitions.Add(filterDefinition);
            _filterDefinitionIdToFocus = filterDefinition.Id;
            _filtersMenuVisible = true;
            StateHasChanged();
        }

        internal Task ApplyFiltersAsync()
        {
            _filtersMenuVisible = false;
            return InvokeServerLoadFunc();
        }

        /// <summary>
        /// Closes the filter UI owned by this data grid.
        /// </summary>
        /// <remarks>
        /// This method closes the filter panel shown by <see cref="VekDataGrid{T}"/>, such as the panel used by <see cref="DataGridFilterMode.Simple"/>.
        /// Incomplete filters which still require a value are removed as part of closing the panel.
        /// </remarks>
        public Task CloseFilterAsync()
        {
            _filtersMenuVisible = false;
            CleanupIncompleteFilters();
            StateHasChanged();
            return Task.CompletedTask;
        }

        private Task OnFiltersPanelKeyDownAsync(KeyboardEventArgs args)
        {
            return args.Key == "Escape" ? CloseFilterAsync() : Task.CompletedTask;
        }

        /// <summary>
        /// Removes all filters from all columns.
        /// </summary>
        public async Task ClearFiltersAsync()
        {
            FilterDefinitions.ForEach(x => x.Value = null);
            FilterDefinitions.Clear();
            await InvokeServerLoadFunc();
            await NotifyFilterChangedAsync();
        }

        /// <summary>
        /// Adds the specified filter to the list of filters.
        /// </summary>
        /// <param name="definition">The filter to add.</param>
        public async Task AddFilterAsync(IFilterDefinition<T> definition)
        {
            if (FilterDefinitions.All(x => x.Id != definition.Id))
            {
                FilterDefinitions.Add(definition);
            }
            _filtersMenuVisible = true;
            await InvokeServerLoadFunc();
            await NotifyFilterChangedAsync();
            if (!HasServerData) StateHasChanged();
        }

        internal async Task RemoveFilterAsync(Guid? id)
        {
            var index = FilterDefinitions.FindIndex(x => x.Id == id);
            if (index == -1)
            {
                return;
            }

            FilterDefinitions[index].Value = null;
            FilterDefinitions.RemoveAt(index);
            await InvokeServerLoadFunc();
            GroupItems();
            await NotifyFilterChangedAsync();
        }

        internal Task NotifyFilterChangedAsync() => FilterChanged.InvokeAsync(FilterDefinitions.AsReadOnly());

        private (string Pattern, Regex Regex)? _cachedHighlightRegex;

        private void UpdateSearchTokens()
        {
            CurrentSearchTokens = DataGridSearchEngine.ParseTokens(_searchTextState.Value, _searchTextParseModeState.Value);
            _cachedHighlightRegex = null;
        }

        private async Task OnSearchTextChangedAsync()
        {
            UpdateSearchTokens();
            _currentRenderFilteredItemsCache = null;
            GroupItems();
            await InvokeServerLoadFunc();
            if (!HasServerData)
            {
                StateHasChanged();
            }
        }

        private async Task OnSearchTextParseModeChangedAsync()
        {
            UpdateSearchTokens();
            _currentRenderFilteredItemsCache = null;
            GroupItems();
            await InvokeServerLoadFunc();
            if (!HasServerData)
            {
                StateHasChanged();
            }
        }

        internal Task OnSearchInputChangedAsync(string? text)
        {
            return SetSearchTextAsync(text);
        }

        /// <summary>
        /// Sets the search text used to filter rows in the data grid.
        /// </summary>
        /// <param name="searchText">The search text to apply, or <c>null</c> to clear the search.</param>
        public async Task SetSearchTextAsync(string? searchText)
        {
            var changed = !string.Equals(_searchTextState.Value, searchText, StringComparison.Ordinal);
            await _searchTextState.SetValueAsync(searchText);
            if (!changed && SearchTextChanged.HasDelegate)
            {
                await SearchTextChanged.InvokeAsync(searchText);
            }

            UpdateSearchTokens();
            _currentRenderFilteredItemsCache = null;
            GroupItems();
            await InvokeServerLoadFunc();
            if (!HasServerData)
            {
                StateHasChanged();
            }
        }

        private Regex GetOrCreateHighlightRegex(string pattern)
        {
            var cached = _cachedHighlightRegex;
            if (cached.HasValue && string.Equals(cached.Value.Pattern, pattern, StringComparison.Ordinal))
            {
                return cached.Value.Regex;
            }

            var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.NonBacktracking, RegexDefaults.MatchTimeout);
            _cachedHighlightRegex = (pattern, regex);
            return regex;
        }

        internal RenderFragment RenderCellHighlightedText(string? text, Column<T> column)
        {
            if (string.IsNullOrEmpty(text) || !column.AllowSearchState.Value || !_highlightSearchResultsState.Value || CurrentSearchTokens.Count == 0)
            {
                return builder => builder.AddContent(0, text);
            }

            var fragments = Splitter.GetFragments(text, null, CurrentSearchTokens, out var regex, caseSensitive: false);
            if (fragments.Length == 0 || string.IsNullOrEmpty(regex))
            {
                return builder => builder.AddContent(0, text);
            }

            var compiledRegex = GetOrCreateHighlightRegex(regex);

            return builder =>
            {
                foreach (var fragment in fragments.Span)
                {
                    if (compiledRegex.IsMatch(fragment))
                    {
                        builder.OpenElement(0, "mark");
                        builder.AddAttribute(1, "class", "vek-table-cell-highlight");
                        builder.AddContent(2, fragment);
                        builder.CloseElement();
                    }
                    else
                    {
                        builder.AddContent(3, fragment);
                    }
                }
            };
        }

        internal async Task SetSelectedItemAsync(bool value, T item)
        {
            Debug.Assert(item is not null);

            if (!SelectionChangeable)
                return;

            if (IsRowSelectionDisabled(item))
                return; // Do not change selection if the item is disabled

            if (value)
            {
                if (!MultiSelection)
                {
                    Selection.Clear();
                }

                Selection.Add(item);
                await _selectedItemState.SetValueAsync(item);
            }
            else
            {
                Selection.Remove(item);
                if (Comparer != null)
                {
                    if (Comparer.Equals(item, _selectedItemState.Value))
                    {
                        await _selectedItemState.SetValueAsync(default);
                    }
                }
                else
                {
                    if (item.Equals(_selectedItemState.Value))
                    {
                        await _selectedItemState.SetValueAsync(default);
                    }
                }
            }

            // Create new HashSet instance to ensure ParameterState's comparer detects changes
            await _selectedItemsState.SetValueAsync(new HashSet<T>(Selection, Comparer));

            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Toggles the selection of an item the same way a row click does.
        /// </summary>
        /// <param name="item">The item to select or deselect.</param>
        /// <remarks>
        /// Updates <see cref="SelectedItem"/> and <see cref="SelectedItems"/>.  Does nothing when <see cref="SelectOnRowClick"/> is <c>false</c>.
        /// </remarks>
        public Task SetSelectedItemAsync(T item)
        {
            if (!SelectOnRowClick)
            {
                return Task.CompletedTask;
            }

            var isSelected = Selection.Contains(item);
            return SetSelectedItemAsync(!isSelected, item);
        }

        internal async Task SetSelectAllAsync(bool value)
        {
            // nothing should happen if multiselection is false
            if (!MultiSelection)
                return;

            if (!SelectionChangeable)
                return;

            var selectableItems = GetSelectableItems();

            if (value)
            {
                Selection.UnionWith(selectableItems);
            }
            else
            {
                // Only remove the rows the checkbox controls so selections it cannot restore, such as disabled rows, survive.
                Selection.ExceptWith(selectableItems);
            }

            // Create new HashSet instance to ensure ParameterState's comparer detects changes
            await InvokeAsync(() => _selectedItemsState.SetValueAsync(new HashSet<T>(Selection, Comparer)));

            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Selects or clears every selectable row of a single group, leaving rows outside of the group untouched.
        /// </summary>
        /// <param name="value">When <c>true</c>, the group's rows are added to the selection; otherwise they are removed from it.</param>
        /// <param name="groupItems">The rows belonging to the group.</param>
        internal async Task SetGroupSelectAllAsync(bool value, IEnumerable<T> groupItems)
        {
            // nothing should happen if multiselection is false
            if (!MultiSelection)
                return;

            if (!SelectionChangeable)
                return;

            var selectableItems = GetSelectableItems(groupItems);

            if (value)
            {
                Selection.UnionWith(selectableItems);
            }
            else
            {
                Selection.ExceptWith(selectableItems);
            }

            // Create new HashSet instance to ensure ParameterState's comparer detects changes
            await InvokeAsync(() => _selectedItemsState.SetValueAsync(new HashSet<T>(Selection, Comparer)));

            await InvokeAsync(StateHasChanged);
        }

        private SelectColumn<T>? GetSelectColumn()
        {
            // Called once per rendered row, so avoid the LINQ iterator; RenderedColumns is a List and foreach uses its struct enumerator.
            foreach (var column in RenderedColumns)
            {
                if (column is SelectColumn<T> selectColumn)
                {
                    return selectColumn;
                }
            }

            return null;
        }

        /// <summary>
        /// The items which select-all would select, i.e. every displayed row whose selection is not disabled.
        /// </summary>
        internal IEnumerable<T> GetSelectableItems() => GetSelectableItems(HasServerData ? ServerItems : FilteredItems);

        /// <summary>
        /// The items of <paramref name="items"/> whose selection is not disabled.
        /// </summary>
        internal IEnumerable<T> GetSelectableItems(IEnumerable<T> items)
        {
            var selectColumn = GetSelectColumn();

            return items.Where(item => !IsRowSelectionDisabled(item, selectColumn));
        }

        /// <summary>
        /// The state of a select-all checkbox covering <paramref name="items"/>: <c>true</c> when every selectable row is selected, <c>false</c> when none of them is, otherwise <c>null</c>.
        /// </summary>
        /// <param name="items">The rows the checkbox covers, e.g. all displayed rows for the header checkbox or a group's rows for a group footer checkbox.</param>
        internal bool? GetSelectionState(IEnumerable<T> items)
        {
            // The checkbox covers every filtered row, not just the visible page, so walk the rows once and stop as soon as the answer cannot change.
            if (Selection.Count == 0)
            {
                return false;
            }

            var selectColumn = GetSelectColumn();
            var anySelected = false;
            var anyUnselected = false;

            foreach (var item in items)
            {
                if (IsRowSelectionDisabled(item, selectColumn))
                {
                    continue;
                }

                if (Selection.Contains(item))
                {
                    anySelected = true;
                }
                else
                {
                    anyUnselected = true;
                }

                if (anySelected && anyUnselected)
                {
                    return null;
                }
            }

            // No selectable row is selected, or every one of them is.
            return anySelected;
        }

        internal bool? GetRowSelectionState(T item)
        {
            var selectColumn = GetSelectColumn();
            if (selectColumn is null || IsRowSelectionDisabled(item, selectColumn))
            {
                return null;
            }

            return Selection.Contains(item);
        }

        private static bool IsRowSelectionDisabled(T item, SelectColumn<T>? selectColumn)
        {
            return selectColumn?.DisabledFunc?.Invoke(item) is true;
        }

        internal bool IsRowSelectionDisabled(T item)
        {
            return IsRowSelectionDisabled(item, GetSelectColumn());
        }

        internal IEnumerable<T> Sort(IEnumerable<T> items)
        {
            if (!items.Any())
                return items;

            if (null == SortDefinitions || 0 == SortDefinitions.Count)
                return items;

            IOrderedEnumerable<T>? orderedEnumerable = null;

            foreach (var sortDefinition in SortDefinitions.Values.Where(sd => sd.SortFunc != null).OrderBy(sd => sd.Index))
            {
                Debug.Assert(sortDefinition.SortFunc is not null);
                if (null == orderedEnumerable)
                    orderedEnumerable = sortDefinition.Descending ? items.OrderByDescending(item => sortDefinition.SortFunc(item), sortDefinition.Comparer)
                        : items.OrderBy(item => sortDefinition.SortFunc(item), sortDefinition.Comparer);
                else
                    orderedEnumerable = sortDefinition.Descending ? orderedEnumerable.ThenByDescending(item => sortDefinition.SortFunc(item), sortDefinition.Comparer)
                        : orderedEnumerable.ThenBy(item => sortDefinition.SortFunc(item), sortDefinition.Comparer);
            }

            return orderedEnumerable ?? items;
        }

        internal void ClearEditingItem()
        {
            _editingItem = default;
            _editingSourceItem = default;
        }

        /// <summary>
        /// This method notifies the consumer that changes to the data have been committed
        /// and what those changes are. This variation of the method is only used by the Cell
        /// when the EditMode is set to cell.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal async Task CommitItemChangesAsync(T item)
        {
            // Here, we need to validate at the cellular level...
            if (CommittedItemChanges != null)
                _ = await CommittedItemChanges(item); // ignore return value in cell edit mode

            await CommittedItemChanged.InvokeAsync(item);
        }

        /// <summary>
        /// This method notifies the consumer that changes to the data have been committed
        /// and what those changes are. This variation of the method is used when the EditMode
        /// is anything but Cell since the _editingItem is used.
        /// </summary>
        /// <returns></returns>
        internal async Task CommitItemChangesAsync()
        {
            Debug.Assert(_editingItem is not null);
            Debug.Assert(_editForm is not null);
            await _editForm.ValidateAsync();
            if (!_editForm.IsValid)
            {
                return;
            }

            if (_editingSourceItem != null)
            {
                if (CommittedItemChanges != null)
                {
                    var closeBehavior = await CommittedItemChanges(_editingItem);

                    if (closeBehavior == DataGridEditFormAction.KeepOpen)
                        return;
                }

                foreach (var property in _properties.Where(p => p.CanWrite))
                    property.SetValue(_editingSourceItem, property.GetValue(_editingItem));

                await CommittedItemChanged.InvokeAsync(_editingSourceItem);

                ClearEditingItem();
                _isEditFormOpen = false;
            }
        }

        /// <summary>
        /// Checks if the specified item is currently being edited in inline mode.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns><c>true</c> if the item is being edited; otherwise, <c>false</c>.</returns>
        public bool IsEditingItem(T item)
        {
            if (EditMode != DataGridEditMode.Inline || EqualityComparer<T?>.Default.Equals(_editingSourceItem, default))
                return false;

            var comparer = Comparer ?? EqualityComparer<T>.Default;

            // Check both the source item and the editing copy since the context may have either
            return comparer.Equals(_editingSourceItem, item) || (!EqualityComparer<T?>.Default.Equals(_editingItem, default) && comparer.Equals(_editingItem, item));
        }

        /// <summary>
        /// Gets the editing copy of the item if it's currently being edited in inline mode;
        /// otherwise, returns the original item.
        /// </summary>
        /// <param name="item">The source item to check.</param>
        /// <returns>The editing copy if the item is being edited; otherwise, the original item.</returns>
        internal T GetEditingItemOrSource(T item)
        {
            if (EditMode != DataGridEditMode.Inline || EqualityComparer<T?>.Default.Equals(_editingSourceItem, default))
                return item;

            var comparer = Comparer ?? EqualityComparer<T>.Default;

            // Check both the source item and the editing copy since the context may have either
            if (comparer.Equals(_editingSourceItem, item) || (!EqualityComparer<T?>.Default.Equals(_editingItem, default) && comparer.Equals(_editingItem, item)))
            {
                var editingItem = _editingItem;
                if (editingItem is not null)
                    return editingItem;
            }

            return item;
        }

        /// <summary>
        /// Starts a new row using <see cref="NewItemFactory"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="OnRowCreating"/> callback runs after the new item is created and before the row is rendered.
        /// </remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task StartNewRowAsync()
        {
            if (ReadOnly || _isCreatingNewRow || HasActiveEdit)
                return;

            if (NewItemFactory is null)
                throw new InvalidOperationException($"{nameof(NewItemFactory)} must be set before starting a new row.");

            var item = NewItemFactory();

            if (item is null)
                throw new InvalidOperationException($"{nameof(NewItemFactory)} must return a non-null item.");

            await OnRowCreating.InvokeAsync(item);
            _newRowItem = item;
            _isCreatingNewRow = true;
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Validates and completes the active new row.
        /// </summary>
        /// <remarks>
        /// When validation succeeds, <see cref="OnRowCreated"/> receives the new item so the caller can add it to the data source.
        /// The row remains open when validation fails or the callback throws.
        /// </remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task CommitNewRowAsync()
        {
            if (!_isCreatingNewRow)
                return;

            await _inlineEditValidator.ValidateAsync();
            if (_inlineEditValidator.Errors.Length > 0)
                return;

            var item = _newRowItem!;
            await OnRowCreated.InvokeAsync(item);
            _newRowItem = default;
            _isCreatingNewRow = false;
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Discards the active new row without invoking <see cref="OnRowCreated"/>.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task CancelNewRowAsync()
        {
            if (!_isCreatingNewRow)
                return;

            _newRowItem = default;
            _isCreatingNewRow = false;
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Commits inline edits, persists changes to the source item, and exits edit mode.
        /// </summary>
        /// <remarks>
        /// The row's editors are validated first, and the commit is abandoned when any of them reports an error.
        /// Use the <see cref="CommittedItemChanges"/> callback for validation the editors cannot express.
        /// Return <see cref="DataGridEditFormAction.KeepOpen"/> to prevent the commit and keep the row in edit mode.
        /// </remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task CommitInlineEditAsync()
        {
            if (EditMode != DataGridEditMode.Inline || _editingItem is not { } editingItem || _editingSourceItem is not { } editingSourceItem)
                return;

            // Mirror Form mode, which validates before copying anything back to the source item.
            // Errors is read instead of IsValid because that getter starts a second, unawaited validation pass which would clear the errors this one just collected.
            await _inlineEditValidator.ValidateAsync();
            if (_inlineEditValidator.Errors.Length > 0)
                return;

            // Allow consumer to validate/persist
            if (CommittedItemChanges != null)
            {
                var closeBehavior = await CommittedItemChanges(editingItem);

                if (closeBehavior == DataGridEditFormAction.KeepOpen)
                    return;
            }

            // Copy values from editing copy back to source
            foreach (var property in _properties.Where(p => p.CanWrite))
                property.SetValue(editingSourceItem, property.GetValue(editingItem));

            await CommittedItemChanged.InvokeAsync(editingSourceItem);

            ClearEditingItem();
        }

        internal async Task OnRowClickedAsync(MouseEventArgs args, T item, int rowIndex)
        {
            await RowClick.InvokeAsync(new DataGridRowClickEventArgs<T>(args, item, rowIndex));
            if (_focusedRowEnabledState.Value || _keyboardNavigationEnabledState.Value)
            {
                await SetFocusedRowAsync(rowIndex, item);
            }
            await ActivateRowBehaviorsAsync(item, selectRow: _selectionModeState.Value != DataGridSelectionMode.CellRange);
        }

        internal async Task OnContextMenuClickedAsync(MouseEventArgs args, T item, int rowIndex)
        {
            if (RowContextMenuClick.HasDelegate)
            {
                await RowContextMenuClick.InvokeAsync(new DataGridRowClickEventArgs<T>(args, item, rowIndex));
            }

            if (HasRowContextMenu)
            {
                var contextArgs = new DataGridContextMenuArgs<T>(
                    args,
                    DataGridContextMenuTarget.Row,
                    item,
                    rowIndex,
                    null,
                    null,
                    this,
                    null,
                    null);

                await OpenContextMenuAsync(contextArgs);
            }
        }

        internal async Task OnCellClickedAsync(MouseEventArgs args, T item, int rowIndex, int columnIndex, Column<T> column)
        {
            // stopPropagation on the <td> prevents this click from bubbling to the <tr> handler
            // (OnRowClickedAsync), so we share the same row behaviors via ActivateRowBehaviorsAsync.
            // RowClick is intentionally not raised here; whether it should fire on a cell click
            // is an explicit API decision.
            if (_selectionModeState.Value == DataGridSelectionMode.CellRange)
            {
                if (!_cellRangePointerGesture)
                {
                    var range = _cellSelectionModel.SetCell(rowIndex, columnIndex, args.ShiftKey);
                    await _selectedCellRangeState.SetValueAsync(range);
                }

                _cellRangePointerGesture = false;
            }

            if (_keyboardNavigationEnabledState.Value)
            {
                await SetFocusedCellAsync(rowIndex, columnIndex);
            }

            await CellClick.InvokeAsync(new DataGridCellClickEventArgs<T>(args, item, rowIndex, columnIndex, column));
            await ActivateRowBehaviorsAsync(item, selectRow: _selectionModeState.Value != DataGridSelectionMode.CellRange);
        }

        /// <summary>
        /// Applies the row-level side effects that should occur on any click interaction with a row
        /// or one of its cells: edit activation (when configured) and selection update.
        /// Both <see cref="OnRowClickedAsync"/> and <see cref="OnCellClickedAsync"/> delegate here
        /// so that new row-click behaviors only need to be added in one place.
        /// </summary>
        private async Task ActivateRowBehaviorsAsync(T item, bool selectRow = true)
        {
            if ((_focusedRowEnabledState.Value || _keyboardNavigationEnabledState.Value) &&
                (_focusedItemState.Value is null || !EqualityComparer<T?>.Default.Equals(_focusedItemState.Value, item)))
            {
                await SetFocusedItemAsync(item);
            }

            if (EditTrigger == DataGridEditTrigger.OnRowClick)
            {
                if (EditMode == DataGridEditMode.Cell)
                    await BeginCellEditAsync(item);
                else
                    await SetEditingItemAsync(item);
            }

            if (selectRow)
            {
                await SetSelectedItemAsync(item);
            }
        }

        internal async Task OnCellContextMenuClickedAsync(MouseEventArgs args, T item, int rowIndex, int columnIndex, Column<T> column)
        {
            if (CellContextMenuClick.HasDelegate)
            {
                await CellContextMenuClick.InvokeAsync(new DataGridCellClickEventArgs<T>(args, item, rowIndex, columnIndex, column));
            }

            if (HasCellContextMenu)
            {
                var cell = new Cell<T>(this, column, GetEditingItemOrSource(item), item);
                var value = cell.ComputedValue;
                string? formattedText;
                if (column.Culture is not null && column.isNumber)
                {
                    formattedText = column.ContentFormat is not null
                        ? ((IFormattable?)cell._valueNumber)?.ToString(column.ContentFormat, column.Culture)
                        : cell._valueNumber?.ToString(column.Culture);
                }
                else
                {
                    formattedText = column.ContentFormat is not null
                        ? ((IFormattable?)cell.ComputedValue)?.ToString(column.ContentFormat, column.Culture)
                        : cell.ComputedValue?.ToString();
                }

                var contextArgs = new DataGridContextMenuArgs<T>(
                    args,
                    DataGridContextMenuTarget.Cell,
                    item,
                    rowIndex,
                    columnIndex,
                    column,
                    this,
                    value,
                    formattedText);

                await OpenContextMenuAsync(contextArgs);
            }
        }

        // The lambdas live in their own methods because the compiler allocates a closure on entry to whichever method
        // declares one, even when the branch that needs it is never taken.
        private EventCallback<MouseEventArgs> GetCellClickCallback(T item, int rowIndex, int colIndex, Column<T> column)
            => CellClick.HasDelegate || _selectionModeState.Value == DataGridSelectionMode.CellRange || _keyboardNavigationEnabledState.Value
                ? CreateCellClickCallback(item, rowIndex, colIndex, column)
                : default;

        private EventCallback<MouseEventArgs> CreateCellClickCallback(T item, int rowIndex, int colIndex, Column<T> column)
            => EventCallback.Factory.Create<MouseEventArgs>(this, args => OnCellClickedAsync(args, item, rowIndex, colIndex, column));

        private EventCallback<MouseEventArgs> GetCellContextMenuClickCallback(T item, int rowIndex, int colIndex, Column<T> column)
            => CellContextMenuClick.HasDelegate || HasCellContextMenu
                ? CreateCellContextMenuClickCallback(item, rowIndex, colIndex, column)
                : default;

        private EventCallback<MouseEventArgs> CreateCellContextMenuClickCallback(T item, int rowIndex, int colIndex, Column<T> column)
            => EventCallback.Factory.Create<MouseEventArgs>(this, args => OnCellContextMenuClickedAsync(args, item, rowIndex, colIndex, column));

        internal bool HasContextMenu =>
            _contextMenuEnabledState.Value ||
            HeaderContextMenuTemplate is not null ||
            RowContextMenuTemplate is not null ||
            CellContextMenuTemplate is not null ||
            FooterContextMenuTemplate is not null ||
            ContextMenuTemplate is not null ||
            OnContextMenu.HasDelegate ||
            RowContextMenuClick.HasDelegate ||
            CellContextMenuClick.HasDelegate;

        internal bool HasHeaderContextMenu =>
            _contextMenuEnabledState.Value ||
            HeaderContextMenuTemplate is not null ||
            ContextMenuTemplate is not null ||
            OnContextMenu.HasDelegate;

        internal bool HasCellContextMenu =>
            _contextMenuEnabledState.Value ||
            CellContextMenuTemplate is not null ||
            RowContextMenuTemplate is not null ||
            ContextMenuTemplate is not null ||
            CellContextMenuClick.HasDelegate ||
            OnContextMenu.HasDelegate;

        internal bool HasRowContextMenu =>
            _contextMenuEnabledState.Value ||
            RowContextMenuTemplate is not null ||
            ContextMenuTemplate is not null ||
            RowContextMenuClick.HasDelegate ||
            OnContextMenu.HasDelegate;

        internal bool HasFooterContextMenu =>
            _contextMenuEnabledState.Value ||
            FooterContextMenuTemplate is not null ||
            ContextMenuTemplate is not null ||
            OnContextMenu.HasDelegate;

        internal RenderFragment<DataGridContextMenuArgs<T>>? GetContextMenuTemplate(DataGridContextMenuTarget target)
        {
            return target switch
            {
                DataGridContextMenuTarget.Header => HeaderContextMenuTemplate ?? ContextMenuTemplate,
                DataGridContextMenuTarget.Cell => CellContextMenuTemplate ?? RowContextMenuTemplate ?? ContextMenuTemplate,
                DataGridContextMenuTarget.Row => RowContextMenuTemplate ?? ContextMenuTemplate,
                DataGridContextMenuTarget.Footer => FooterContextMenuTemplate ?? ContextMenuTemplate,
                _ => ContextMenuTemplate
            };
        }

        internal async Task OpenContextMenuAsync(DataGridContextMenuArgs<T> args)
        {
            if (OnContextMenu.HasDelegate)
            {
                await OnContextMenu.InvokeAsync(args);
            }

            if (args.Handled)
            {
                return;
            }

            var template = GetContextMenuTemplate(args.Target);
            if (!_contextMenuEnabledState.Value && template is null)
            {
                return;
            }

            _currentContextMenuArgs = args;
            _contextMenuOpen = true;
            StateHasChanged();
            await Task.Yield();

            if (_contextMenu is not null)
            {
                await _contextMenu.OpenMenuAsync(args.MouseEventArgs);
            }
        }

        /// <summary>
        /// Closes the currently active context menu.
        /// </summary>
        public Task CloseContextMenuAsync()
        {
            _contextMenuOpen = false;
            _currentContextMenuArgs = null;
            StateHasChanged();
            return Task.CompletedTask;
        }

        private Task OnContextMenuOpenChangedAsync(bool open)
        {
            _contextMenuOpen = open;
            if (!open)
            {
                _currentContextMenuArgs = null;
            }
            return Task.CompletedTask;
        }

        internal async Task CopyTextToClipboardAsync(string text)
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
            }
            catch
            {
                try
                {
                    await JSRuntime.InvokeVoidAsync("vekWindow.copyToClipboard", text);
                }
                catch
                {
                    // Graceful fallback when clipboard permissions or runtime methods are unavailable
                }
            }
        }

        internal async Task OnHeaderContextMenuClickedAsync(MouseEventArgs args, Column<T> column)
        {
            if (HasHeaderContextMenu)
            {
                var colIndex = RenderedColumns.IndexOf(column);
                var contextArgs = new DataGridContextMenuArgs<T>(
                    args,
                    DataGridContextMenuTarget.Header,
                    default,
                    null,
                    colIndex >= 0 ? colIndex : null,
                    column,
                    this,
                    null,
                    column.Title);

                await OpenContextMenuAsync(contextArgs);
            }
        }

        internal async Task OnFooterContextMenuClickedAsync(MouseEventArgs args, Column<T> column)
        {
            if (HasFooterContextMenu)
            {
                var colIndex = RenderedColumns.IndexOf(column);
                var contextArgs = new DataGridContextMenuArgs<T>(
                    args,
                    DataGridContextMenuTarget.Footer,
                    default,
                    null,
                    colIndex >= 0 ? colIndex : null,
                    column,
                    this,
                    null,
                    null);

                await OpenContextMenuAsync(contextArgs);
            }
        }

        /// <summary>
        /// Gets the total count of filtered items in the data grid.
        /// </summary>
        /// <returns>
        /// The number of items remaining after applying filters.  When <see cref="ServerData"/> is in use, the <see cref="GridData{T}.TotalItems"/> value is returned.
        /// </returns>
        public int GetFilteredItemsCount()
        {
            if (HasServerData)
                return _serverData.TotalItems;
            return FilteredItems.Count();
        }

        /// <summary>
        /// Navigates to a page when this grid has a <see cref="VekDataGridPager{T}"/>.
        /// </summary>
        /// <param name="page">The page to navigate to.</param>
        public void NavigateTo(Page page)
        {
            CurrentPage = page switch
            {
                Page.First => 0,
                Page.Last => Math.Max(0, numPages - 1),
                Page.Next => Math.Min(numPages - 1, CurrentPage + 1),
                Page.Previous => Math.Max(0, CurrentPage - 1),
                _ => CurrentPage
            };

            GroupItems();
        }

        /// <summary>
        /// Sets the <see cref="RowsPerPage"/> when this grid contains a <see cref="VekDataGridPager{T}"/>.
        /// </summary>
        /// <param name="size">The new page size.</param>
        public Task SetRowsPerPageAsync(int size) => SetRowsPerPageAsync(size, true);

        /// <summary>
        /// Sets the <see cref="RowsPerPage"/> when this grid contains a <see cref="VekDataGridPager{T}"/>.
        /// </summary>
        /// <param name="size">The new page size.</param>
        /// <param name="resetPage">When <c>true</c>, resets <see cref="CurrentPage"/> to 0.</param>
        public async Task SetRowsPerPageAsync(int size, bool resetPage)
        {
            if (_rowsPerPage == size)
                return;

            _rowsPerPage = size;

            if (resetPage)
            {
                var currentPageHasChanged = _currentPage != 0;
                _currentPage = 0;
                if (currentPageHasChanged)
                    await CurrentPageChanged.InvokeAsync(_currentPage);
            }

            await RowsPerPageChanged.InvokeAsync(_rowsPerPage.Value);

            StateHasChanged();

            if (_isFirstRendered)
                await InvokeAsync(InvokeServerLoadFunc);
        }

        /// <summary>
        /// Replaces the sorting behavior for a field.
        /// </summary>
        /// <param name="field">The field to sort.</param>
        /// <param name="direction">The direction to sort results.</param>
        /// <param name="sortFunc">The function which sorts results.</param>
        /// <param name="comparer">The comparer used for custom comparisons.</param>
        public async Task SetSortAsync(string field, SortDirection direction, Func<T, object?> sortFunc, IComparer<object?>? comparer = null)
        {
            var removedSortDefinitions = new HashSet<string>(SortDefinitions.Keys);
            SortDefinitions.Clear();

            var newDefinition = new SortDefinition<T>(field, direction == SortDirection.Descending, 0, sortFunc, comparer);
            SortDefinitions[field] = newDefinition;

            // In case sort is just updated make sure to not mark the field as removed
            removedSortDefinitions.Remove(field);

            await InvokeSortUpdates(SortDefinitions, removedSortDefinitions);
        }

        /// <summary>
        /// Adds or replaces a sort behavior depending on the <see cref="SortMode"/>.
        /// </summary>
        /// <param name="field">The field to sort.</param>
        /// <param name="direction">The direction to sort results.</param>
        /// <param name="sortFunc">The function which sorts results.</param>
        /// <param name="comparer">The comparer used for custom comparisons.</param>
        /// <remarks>
        /// When the <see cref="SortMode"/> is <see cref="SortMode.Single"/>, this method replaces the sort column.  Otherwise, this sort is appended to any existing sort column.
        /// </remarks>
        public async Task ExtendSortAsync(string field, SortDirection direction, Func<T, object?> sortFunc, IComparer<object?>? comparer = null)
        {
            // If SortMode is not multiple, use the default set approach and don't extend.
            if (SortMode != SortMode.Multiple)
            {
                await SetSortAsync(field, direction, sortFunc, comparer);
                return;
            }

            // in case it already exists, just update the current entry
            if (SortDefinitions.TryGetValue(field, out var sortDefinition))
                SortDefinitions[field] = sortDefinition with { Descending = direction == SortDirection.Descending, SortFunc = sortFunc, Comparer = comparer };
            else
            {
                var newDefinition = new SortDefinition<T>(field, direction == SortDirection.Descending, SortDefinitions.Count, sortFunc, comparer);
                SortDefinitions[field] = newDefinition;
            }

            await InvokeSortUpdates(SortDefinitions, null);
        }

        /// <summary>
        /// Removes a sort behavior from the list of sort behaviors.
        /// </summary>
        /// <param name="field">The name of the field to remove.</param>
        public async Task RemoveSortAsync(string? field)
        {
            if (!string.IsNullOrWhiteSpace(field) && SortDefinitions.TryGetValue(field, out var definition))
            {
                SortDefinitions.Remove(field);
                foreach (var defToUpdate in SortDefinitions.Where(kvp => kvp.Value.Index > definition.Index).ToList())
                    SortDefinitions[defToUpdate.Key] = defToUpdate.Value with { Index = defToUpdate.Value.Index - 1 };

                await InvokeSortUpdates(SortDefinitions, new HashSet<string>() { field });
            }
        }

        /// <summary>
        /// Clears all current sort definitions.
        /// </summary>
        private async Task ClearCurrentSortings()
        {
            var removedSortDefinitions = new HashSet<string>(SortDefinitions.Keys);
            SortDefinitions.Clear();
            await InvokeSortUpdates(SortDefinitions, removedSortDefinitions);
        }

        private async Task InvokeSortUpdates(Dictionary<string, SortDefinition<T>> activeSortDefinitions, HashSet<string>? removedSortDefinitions)
        {
            SortChangedEvent?.Invoke(activeSortDefinitions, removedSortDefinitions);
            if (_isFirstRendered)
            {
                await SortChanged.InvokeAsync(new Dictionary<string, SortDefinition<T>>(activeSortDefinitions));
            }

            if (_isFirstRendered)
            {
                await InvokeServerLoadFunc();
                if (!HasServerData)
                    StateHasChanged();
            }
        }

        private void VirtualItemsProviderInitialize()
        {
            if (VirtualItemsProvider != null || VirtualizeServerData == null)
            {
                return;
            }

            VirtualItemsProvider = async request =>
            {
                var stateFunc = (int startIndex, int count) => new GridStateVirtualize<T>
                {
                    StartIndex = startIndex,
                    Count = count,
                    SortDefinitions = SortDefinitions.Values.OrderBy(sd => sd.Index).ToList(),
                    // Additional ToList() here to decouple clients from internal list avoiding runtime issues
                    FilterDefinitions = FilterDefinitions.ToList()
                };

                _serverData = await VirtualizeServerData(
                    stateFunc(request.StartIndex, request.Count),
                    request.CancellationToken
                );

                //do not reset cache, the current chunk load has been cancelled, we wait for the final load
                if (!request.CancellationToken.IsCancellationRequested)
                {
                    _currentRenderFilteredItemsCache = null;
                }

                return new ItemsProviderResult<IndexBag<T>>(
                    _serverData.Items.Select((item, index) => new IndexBag<T>(request.StartIndex + index, item)),
                    _serverData.TotalItems);
            };
        }

        /// <summary>
        /// Starts editing for the specified item.
        /// </summary>
        /// <param name="item">The item to edit.</param>
        public async Task SetEditingItemAsync(T item)
        {
            if (ReadOnly) return;

            _editingSourceItem = item;
            EditingCanceledEvent?.Invoke();
            _previousEditingItem = _editingItem;

            // In cell edit mode changes are written directly to the source item, so there is no working copy to clone and no edit form to open; only StartedEditingItem is raised.
            if (EditMode == DataGridEditMode.Cell)
            {
                _editingItem = default;
                StartedEditingItemEvent?.Invoke();
                await StartedEditingItem.InvokeAsync(item);
                return;
            }

            _editingItem = CloneStrategy.CloneObject(item);
            StartedEditingItemEvent?.Invoke();
            await StartedEditingItem.InvokeAsync(_editingItem);

            // Only open the dialog form for Form mode, not for Inline mode
            if (EditMode == DataGridEditMode.Form)
            {
                _isEditFormOpen = true;
            }
        }

        /// <summary>
        /// Raises <see cref="StartedEditingItem"/> when cell editing begins for an item, without re-raising it for subsequent cell changes on the same item.
        /// </summary>
        /// <param name="item">The item whose cell is being edited.</param>
        internal async Task BeginCellEditAsync(T item)
        {
            // Use the grid's Comparer (same identity used for selection) so a custom comparer can distinguish otherwise-equal rows, e.g. records.
            // The first edit always starts since nothing is being edited yet.
            var comparer = Comparer ?? EqualityComparer<T>.Default;
            if (_editingSourceItem is not null && comparer.Equals(_editingSourceItem, item))
                return;

            await SetEditingItemAsync(item);
        }

        /// <summary>
        /// Cancels the current editing of an item.
        /// </summary>
        public async Task CancelEditingItemAsync()
        {
            EditingCanceledEvent?.Invoke();
            var canceledItem = !EqualityComparer<T?>.Default.Equals(_editingItem, default)
                ? _editingItem
                : _editingSourceItem;
            await CanceledEditingItem.InvokeAsync(canceledItem);
            ClearEditingItem();
            _isEditFormOpen = false;
        }

        /// <summary>
        /// Opens or closes the filter panel.
        /// </summary>
        public void ToggleFiltersMenu()
        {
            _filtersMenuVisible = !_filtersMenuVisible;
            StateHasChanged();
        }

        /// <summary>
        /// Reloads grid data by calling the <see cref="ServerData"/> function.
        /// </summary>
        public Task ReloadServerData()
        {
            return InvokeServerLoadFunc();
        }

        /// <summary>
        /// Opens the filter panel.
        /// </summary>
        public void OpenFilters()
        {
            OpenFilters(FilterDefinitions.FirstOrDefault()?.Id);
        }

        internal void OpenFilters(Guid? filterDefinitionIdToFocus)
        {
            _filterDefinitionIdToFocus = filterDefinitionIdToFocus;
            _filtersMenuVisible = true;
            StateHasChanged();
        }

        private void OnFiltersPanelClosed() => CleanupIncompleteFilters();

        internal void CleanupIncompleteFilters() => FilterDefinitions.RemoveAll(p => p.Value == null && ValueRequired(p));
        internal void SetFiltersMenuPosition(double top, double left)
        {
            _filtersMenuPosition = (top, left);
        }

        private static bool ValueRequired(IFilterDefinition<T> filterDefinition) => filterDefinition.Operator is not
            FilterOperator.String.Empty and not FilterOperator.String.NotEmpty and not
            FilterOperator.Number.Empty and not FilterOperator.Number.NotEmpty and not
            FilterOperator.DateTime.Empty and not FilterOperator.DateTime.NotEmpty;

        internal async Task HideAllColumnsAsync()
        {
            foreach (var column in RenderedColumns)
            {
                if (column.hideable)
                    await column.HideAsync();
            }
            DropContainerHasChanged();
            StateHasChanged();
        }

        internal async Task ShowAllColumnsAsync()
        {
            foreach (var column in RenderedColumns)
            {
                if (column.hideable)
                    await column.ShowAsync();
            }
            DropContainerHasChanged();
            StateHasChanged();
        }

        /// <summary>
        /// Shows a panel that lets you show, hide, filter, groupedColumns, sort and re-arrange columns.
        /// </summary>
        public void ShowColumnsPanel(MouseEventArgs? args = null)
        {
            if (args != null)
            {
                _columnsPanelPosition = (args.PageY, args.PageX);
            }
            _columnsPanelVisible = true;
            StateHasChanged();
        }

        /// <summary>
        /// Hides the columns panel.
        /// </summary>
        public void HideColumnsPanel()
        {
            _columnsPanelVisible = false;
            StateHasChanged();
        }

        /// <summary>
        /// Hides the columns panel only when it is open.
        /// </summary>
        /// <remarks>
        /// A column filter menu lives in its header cell, so opening one renders that cell and needs a grid render only to close an open columns panel.
        /// </remarks>
        internal void HideColumnsPanelIfVisible()
        {
            if (_columnsPanelVisible)
            {
                HideColumnsPanel();
            }
        }

        private bool _isColumnChooserVisible;
        private string? _columnChooserAnchorId;

        /// <summary>
        /// Gets a value indicating whether the column chooser dialog is currently open.
        /// </summary>
        public bool IsColumnChooserVisible => _isColumnChooserVisible;

        /// <summary>
        /// Gets the anchor element identifier passed to the open column chooser dialog, if any.
        /// </summary>
        public string? ColumnChooserAnchorId => _columnChooserAnchorId;

        /// <summary>
        /// Shows the column chooser dialog, optionally anchored to the specified element ID.
        /// </summary>
        /// <param name="anchorId">The optional HTML element ID to anchor the chooser dialog to.</param>
        public Task ShowColumnChooserAsync(string? anchorId = null)
        {
            _columnChooserAnchorId = anchorId;
            _isColumnChooserVisible = true;
            StateHasChanged();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hides the column chooser dialog.
        /// </summary>
        public Task HideColumnChooserAsync()
        {
            _isColumnChooserVisible = false;
            _columnChooserAnchorId = null;
            StateHasChanged();
            return Task.CompletedTask;
        }

        private bool _isFilterBuilderVisible;

        /// <summary>
        /// Specifies whether the visual filter builder dialog is currently visible.
        /// </summary>
        public bool IsFilterBuilderVisible => _isFilterBuilderVisible;

        /// <summary>
        /// Shows the visual filter builder dialog.
        /// </summary>
        public Task ShowFilterBuilderAsync()
        {
            _isFilterBuilderVisible = true;
            StateHasChanged();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Hides the visual filter builder dialog.
        /// </summary>
        public Task HideFilterBuilderAsync()
        {
            _isFilterBuilderVisible = false;
            StateHasChanged();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Moves the specified column one position earlier in the rendered columns order.
        /// </summary>
        /// <param name="column">The column to move.</param>
        public void MoveColumnUp(Column<T> column)
        {
            ColumnUp(column);
            StateHasChanged();
        }

        /// <summary>
        /// Moves the specified column one position later in the rendered columns order.
        /// </summary>
        /// <param name="column">The column to move.</param>
        public void MoveColumnDown(Column<T> column)
        {
            ColumnDown(column);
            StateHasChanged();
        }

        private Task ColumnOrderUpdated(VekItemDropInfo<Column<T>> dropItem)
        {
            Debug.Assert(dropItem.Item is not null);
            RenderedColumns.Remove(dropItem.Item);
            RenderedColumns.Insert(dropItem.IndexInZone, dropItem.Item);
            DropContainerHasChanged();

            return Task.CompletedTask;
        }

        private void ColumnUp(Column<T> column)
        {
            var index = RenderedColumns.IndexOf(column);
            if (index > 0)
            {
                RenderedColumns.RemoveAt(index);
                RenderedColumns.Insert(index - 1, column);
            }
            DropContainerHasChanged();
        }

        private void ColumnDown(Column<T> column)
        {
            var index = RenderedColumns.IndexOf(column);
            if (index < RenderedColumns.Count - 1)
            {
                RenderedColumns.RemoveAt(index);
                RenderedColumns.Insert(index + 1, column);
            }
            DropContainerHasChanged();
        }

        internal void DropContainerHasChanged()
        {
            _dropContainer?.Refresh();
            _columnsPanelDropContainer?.Refresh();
        }
        /// <summary>
        /// Performs grouping of the current items.
        /// </summary>
        /// <param name="noStateChange">Defaults to <c>false</c>.  When <c>true</c>, calls to <c>StateHasChanged</c> will not occur.</param>
        /// <remarks>
        /// Applies when <see cref="Groupable"/> is <c>true</c>.
        /// </remarks>
        public void GroupItems(bool noStateChange = false)
        {
            if (!noStateChange)
                DropContainerHasChanged();

            _groupDefinition = default;

            // get all columns that have Groupable set to true
            var groupedColumns = RenderedColumns.Where(x => x.groupable).ToList();
            // is groupable on either DataGrid level or column level
            var isGroupable = Groupable || groupedColumns.Count > 0;
            // any columns that are groupable and have grouping set to true
            groupedColumns = [.. groupedColumns.Where(x => x.GroupingState.Value).OrderBy(x => x._groupByOrderState.Value)];
            // it's only groupable if a column can be grouped
            isGroupable = isGroupable && groupedColumns.Count > 0;

            if (!isGroupable || GetFilteredItemsCount() == 0)
            {
                if (_isFirstRendered && !noStateChange)
                    StateHasChanged();
                return;
            }

            // Initialize with the first group definition
            _groupDefinition = ProcessGroup(groupedColumns[0]);

            // Create a reference to build the hierarchy
            var currentGroupDef = _groupDefinition;

            // Start from index 1 since we've already processed the first column
            for (var i = 1; i < groupedColumns.Count; i++)
            {
                var nextGroupDef = ProcessGroup(groupedColumns[i]);
                // Connect it to the current level
                currentGroupDef.InnerGroup = nextGroupDef;
                // Move to the next level for the next iteration
                currentGroupDef = nextGroupDef;
            }

            if ((_isFirstRendered || HasServerData) && !noStateChange)
                StateHasChanged();
        }

        private IEnumerable<IGrouping<object?, T>> GroupItemsPage
        {
            get
            {
                return GetItemsOfGroup(_groupDefinition, CurrentPageItems);
            }
        }

        internal IEnumerable<IGrouping<object?, T>> GetItemsOfGroup(GroupDefinition<T>? parent, IEnumerable<T>? sourceList)
        {
            if (parent is null || sourceList is null)
            {
                return new List<IGrouping<object, T>>();
            }

            if (parent.Selector is not null)
            {
                return sourceList.GroupBy(parent.Selector).ToList();
            }

            return new List<IGrouping<object, T>>();
        }

        private GroupDefinition<T> ProcessGroup(Column<T> column)
        {
            var expanded = _groupInitialExpanded ?
                            (GroupExpanded || column._groupExpandedState.Value) :
                            column._groupExpandedState.Value;
            return new()
            {
                DataGrid = this,
                Selector = column.groupBy,
                Expanded = expanded,
                GroupTemplate = column.GetGroupTemplate(),
                Indentation = column.GroupIndented,
                Title = column.Title,
                Grouping = new EmptyGrouping<object?, T>(null), // Ensure Grouping is not null
                KeyPath = new GroupKeyPath([])
            };
        }

        internal IEnumerable<GroupDefinition<T>> GetGroupDefinitions(GroupDefinition<T> groupDef, IEnumerable<IGrouping<object?, T>> groups)
        {
            List<GroupDefinition<T>> result = new();
            foreach (var group in groups)
            {
                var expanded = false;
                var currentKeyPath = groupDef.Parent?.KeyPath.ToList() ?? [];
                GroupKeyPath? keyPath = null;
                if (group is not null)
                {
                    currentKeyPath.Add(group.Key);
                    keyPath = new GroupKeyPath(currentKeyPath);
                    var key = new GroupKey(groupDef.Title, keyPath);
                    expanded = _groupExpansionsDict.TryGetValue(key, out var value) ? value :
                                   groupDef.Expanded;
                    _groupExpansionsDict.TryAdd(key, expanded);
                }
                var newGroupDefinition = new GroupDefinition<T>
                {
                    DataGrid = this,
                    Selector = groupDef.Selector,
                    Expanded = expanded,
                    GroupTemplate = groupDef.GroupTemplate,
                    Indentation = groupDef.Indentation,
                    Title = groupDef.Title,
                    Parent = groupDef.Parent,
                    Grouping = group ?? new EmptyGrouping<object?, T>(null),
                    KeyPath = keyPath ?? new GroupKeyPath(currentKeyPath),
                };

                var innerGroup = groupDef.InnerGroup;
                if (innerGroup != null)
                {
                    // Create a new InnerGroup instance to prevent unwanted side effects from shared references at different grouping levels (e.g., tracking the Expanded state)
                    newGroupDefinition.InnerGroup = new GroupDefinition<T>
                    {
                        DataGrid = this,
                        Selector = innerGroup.Selector,
                        Expanded = innerGroup.Expanded,
                        GroupTemplate = innerGroup.GroupTemplate,
                        Indentation = innerGroup.Indentation,
                        Title = innerGroup.Title,
                        Parent = newGroupDefinition,
                        Grouping = innerGroup.Grouping,
                        KeyPath = new GroupKeyPath(innerGroup.KeyPath),
                        InnerGroup = innerGroup.InnerGroup
                    };
                }
                result.Add(newGroupDefinition);
            }
            return result;
        }

        internal async Task UpdateGroupingOrder(Column<T> column, bool added)
        {
            // if added then add to the end if no _groupByOrderState.Value
            if (added)
            {
                var groupedColumns = RenderedColumns.Where(x => x.GroupingState.Value && x != column);
                var newOrder = groupedColumns.Any() ? groupedColumns.Max(x => x._groupByOrderState.Value) + 1 : 0;
                await column._groupByOrderState.SetValueAsync(newOrder);
            }
            // if removed then reset _groupByOrderState.Value
            else
            {
                await column._groupByOrderState.SetValueAsync(default);
            }
            // expand all but last grouped column when changed
            await GroupExpansion();
        }

        private async Task GroupExpansion()
        {
            var groupedColumns = RenderedColumns.Where(x => x.GroupingState.Value).OrderBy(x => x._groupByOrderState.Value).SkipLast(1);
            foreach (var col in groupedColumns.OrderBy(x => x._groupByOrderState.Value))
            {
                await col._groupExpandedState.SetValueAsync(true);
            }
        }

        /// <summary>
        /// Toggles the expanded or collapsed state of the specified group by column name and key.
        /// </summary>
        /// <param name="columnName">The name of the grouped column.</param>
        /// <param name="key">The group key identifying the specific group to expand or collapse.</param>
        /// <param name="expanded">Whether the group should be expanded (true) or collapsed (false).</param>
        public void ToggleGroupExpand(string? columnName, object? key, bool expanded)
        {
            SetGroupExpanded(columnName, key, expanded);
            StateHasChanged();
        }

        /// <summary>
        /// Records whether a group is expanded without rendering the grid.
        /// </summary>
        /// <remarks>
        /// A group row renders its own rows, so a click on its expander only needs that row to render.
        /// The grid reads the recorded state the next time it renders.
        /// </remarks>
        internal void SetGroupExpanded(string? columnName, object? key, bool expanded)
        {
            // Record the state even when it matches the column default.
            // A group row builds its nested groups from definitions made at the last grid render, which can still carry the grid-level GroupExpanded default, so a missing entry would fall back to that stale value.
            _groupExpansionsDict[new GroupKey(columnName, key)] = expanded;

            _groupInitialExpanded = false;
        }

        /// <summary>
        /// Expands all groups async.
        /// </summary>
        /// <remarks>
        /// Applies when <see cref="Groupable"/> is <c>true</c>.
        /// </remarks>
        public async Task ExpandAllGroupsAsync()
        {
            if (_groupDefinition != null && _groupable)
            {
                await ToggleGroupExpandRecursively(true);
            }
        }

        /// <summary>
        /// Collapses all groups async.
        /// </summary>
        /// <remarks>
        /// Applies when <see cref="Groupable"/> is <c>true</c>.
        /// </remarks>
        public async Task CollapseAllGroupsAsync()
        {
            if (_groupDefinition != null && _groupable)
            {
                await ToggleGroupExpandRecursively(false);
            }
        }

        private async Task ToggleGroupExpandRecursively(bool expanded)
        {
            _groupExpansionsDict.Clear();
            foreach (var column in RenderedColumns)
            {
                if (column.GroupingState.Value)
                {
                    await column._groupExpandedState.SetValueAsync(expanded);
                }
            }
            _groupInitialExpanded = false;
            GroupItems();
        }

        /// <summary>
        /// Expands all Hierarchy columns
        /// </summary>
        public async Task ExpandAllHierarchy()
        {
            Debug.Assert(_buttonDisabledFunc is not null);
            var expandedItems = FilteredItems.Where(x => !_buttonDisabledFunc(x) && _openHierarchies.Add(x));
            foreach (var item in expandedItems)
            {
                await InvokeHierarchyVisibilityToggledAsync(new(item, true));
            }
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Collapses all Hierarchy columns
        /// </summary>
        public async Task CollapseAllHierarchy()
        {
            Debug.Assert(_buttonDisabledFunc is not null);
            foreach (var openedHierarchy in _openHierarchies.Where(x => !_buttonDisabledFunc(x)).ToList())
            {
                await InvokeHierarchyVisibilityToggledAsync(new(openedHierarchy, false));
                _openHierarchies.Remove(openedHierarchy);
            }
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Collapses or expands the hierarchy of the specified item.
        /// </summary>
        /// <param name="item">The item whose hierarchy visibility is to be toggled.</param>
        public async Task ToggleHierarchyVisibilityAsync(T item)
        {
            // if ExpandSingleRow is true, clear all open hierarchies, which will immediately add the item that was clicked.
            if (_expandSingleRowState.Value)
            {
                var itemWasOpen = _openHierarchies.Contains(item);
                foreach (var openedHierarchy in _openHierarchies.Where(x => x != null && !x.Equals(item)))
                {
                    await InvokeHierarchyVisibilityToggledAsync(new(openedHierarchy, false));
                }
                _openHierarchies.Clear();
                _openHierarchies.Add(item);
                if (!itemWasOpen)
                {
                    await InvokeHierarchyVisibilityToggledAsync(new(item, true));
                }
                await InvokeAsync(StateHasChanged);
                return;
            }

            // if item doesn't exist remove will return false and add the item
            if (!_openHierarchies.Remove(item))
            {
                _openHierarchies.Add(item);
                await InvokeHierarchyVisibilityToggledAsync(new(item, true));
            }
            else
            {
                await InvokeHierarchyVisibilityToggledAsync(new(item, false));
            }

            await InvokeAsync(StateHasChanged);
        }

        private async Task InvokeHierarchyVisibilityToggledAsync(DataGridHierarchyVisibilityToggledEventArgs<T> args)
        {
            await HierarchyVisibilityToggled.InvokeAsync(args);
            await _hierarchyColumnVisibilityToggled.InvokeAsync(args);
        }

        #region Resize feature

        internal bool IsResizing { get; set; }

        private ElementReference _gridElement;

        internal async Task<double> GetActualHeight()
        {
            var gridRect = await _gridElement.VekGetBoundingClientRectAsync();
            var gridHeight = gridRect.Height;
            return gridHeight;
        }

        #endregion

        #region Column Pinning & Sticky Offsets

        /// <summary>
        /// Pins or unpins a column to the left or right edge of the grid viewport.
        /// </summary>
        /// <param name="column">The column to pin or unpin.</param>
        /// <param name="fixedStyle">The desired fixed pinning style.</param>
        public async Task PinColumnAsync(Column<T> column, DataGridFixedStyle fixedStyle)
        {
            if (column is null || !RenderedColumns.Contains(column))
            {
                return;
            }

            await column.SetFixedStyleAsync(fixedStyle);
            ReorderPinnedColumns(column, fixedStyle);
            UpdateStickyColumnOffsets();
            StateHasChanged();

            if (ColumnPinChanged.HasDelegate)
            {
                await ColumnPinChanged.InvokeAsync(new DataGridColumnPinChangedEventArgs<T>(column, fixedStyle));
            }
        }

        internal void OnColumnFixedStyleChanged(Column<T> column)
        {
            ReorderPinnedColumns(column, column.EffectiveFixedStyle);
            UpdateStickyColumnOffsets();
            StateHasChanged();

            if (ColumnPinChanged.HasDelegate)
            {
                _ = ColumnPinChanged.InvokeAsync(new DataGridColumnPinChangedEventArgs<T>(column, column.EffectiveFixedStyle));
            }
        }

        internal void UpdateStickyColumnOffsets()
        {
            var visibleColumns = RenderedColumns.Where(c => !c.HiddenState.Value).ToList();

            // 1. Calculate Left-pinned offsets
            double currentLeftOffset = 0;
            var leftColumns = visibleColumns.Where(c => c.IsPinnedLeft).ToList();
            var lastLeftPinned = leftColumns.LastOrDefault();

            foreach (var col in visibleColumns)
            {
                if (col.IsPinnedLeft)
                {
                    col.StickyLeftOffset = currentLeftOffset;
                    col.IsLastStickyLeft = (col == lastLeftPinned);
                    currentLeftOffset += col.GetEstimatedWidthPx();
                }
                else
                {
                    col.StickyLeftOffset = null;
                    col.IsLastStickyLeft = false;
                }
            }

            // 2. Calculate Right-pinned offsets (from right to left)
            double currentRightOffset = 0;
            var rightColumns = visibleColumns.Where(c => c.IsPinnedRight).ToList();
            var firstRightPinned = rightColumns.FirstOrDefault();

            for (var i = visibleColumns.Count - 1; i >= 0; i--)
            {
                var col = visibleColumns[i];
                if (col.IsPinnedRight)
                {
                    col.StickyRightOffset = currentRightOffset;
                    col.IsFirstStickyRight = (col == firstRightPinned);
                    currentRightOffset += col.GetEstimatedWidthPx();
                }
                else
                {
                    col.StickyRightOffset = null;
                    col.IsFirstStickyRight = false;
                }
            }
        }

        private void ReorderPinnedColumns(Column<T> column, DataGridFixedStyle targetFixedStyle)
        {
            if (!RenderedColumns.Contains(column))
            {
                return;
            }

            RenderedColumns.Remove(column);

            if (targetFixedStyle == DataGridFixedStyle.Left)
            {
                var lastLeftIndex = -1;
                for (var i = 0; i < RenderedColumns.Count; i++)
                {
                    if (RenderedColumns[i].IsPinnedLeft)
                    {
                        lastLeftIndex = i;
                    }
                }

                if (lastLeftIndex >= 0)
                {
                    RenderedColumns.Insert(lastLeftIndex + 1, column);
                }
                else
                {
                    var insertIndex = RenderedColumns.Any(x => x.Tag?.ToString() == "hierarchy-column") ? 1 : 0;
                    RenderedColumns.Insert(Math.Min(insertIndex, RenderedColumns.Count), column);
                }
            }
            else if (targetFixedStyle == DataGridFixedStyle.Right)
            {
                RenderedColumns.Add(column);
            }
            else
            {
                var firstRightIndex = -1;
                for (var i = 0; i < RenderedColumns.Count; i++)
                {
                    if (RenderedColumns[i].IsPinnedRight)
                    {
                        firstRightIndex = i;
                        break;
                    }
                }

                if (firstRightIndex >= 0)
                {
                    RenderedColumns.Insert(firstRightIndex, column);
                }
                else
                {
                    RenderedColumns.Add(column);
                }
            }
        }

        private void UpdateColumnPinningAfterDrop(Column<T> draggedColumn)
        {
            var index = RenderedColumns.IndexOf(draggedColumn);
            if (index < 0) return;

            var prevColumn = index > 0 ? RenderedColumns[index - 1] : null;
            var nextColumn = index < RenderedColumns.Count - 1 ? RenderedColumns[index + 1] : null;

            if ((prevColumn is null || prevColumn.IsPinnedLeft || prevColumn.Tag?.ToString() == "hierarchy-column") &&
                nextColumn is not null && nextColumn.IsPinnedLeft)
            {
                if (draggedColumn.EffectiveFixedStyle != DataGridFixedStyle.Left)
                {
                    _ = draggedColumn.SetFixedStyleAsync(DataGridFixedStyle.Left);
                }
            }
            else if ((nextColumn is null || nextColumn.IsPinnedRight) &&
                     prevColumn is not null && prevColumn.IsPinnedRight)
            {
                if (draggedColumn.EffectiveFixedStyle != DataGridFixedStyle.Right)
                {
                    _ = draggedColumn.SetFixedStyleAsync(DataGridFixedStyle.Right);
                }
            }
            else if ((prevColumn is null || !prevColumn.IsPinnedLeft) &&
                     (nextColumn is null || !nextColumn.IsPinnedRight))
            {
                if (draggedColumn.EffectiveFixedStyle != DataGridFixedStyle.None)
                {
                    _ = draggedColumn.SetFixedStyleAsync(DataGridFixedStyle.None);
                }
            }
        }

        #endregion

        /// <summary>
        /// Releases resources used by this data grid.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Cancel a load still in flight so its data source can stop and its result is discarded.
            var source = _serverDataCancellationTokenSource;
            try
            {
                source?.Cancel();
            }
            catch
            {
                /*ignored*/
            }
            finally
            {
                source?.Dispose();
            }
        }

        private sealed class EmptyGrouping<TKey, TElement> : IGrouping<TKey, TElement>
        {
            public TKey Key { get; }

            public EmptyGrouping(TKey key)
            {
                Key = key;
            }

            public IEnumerator<TElement> GetEnumerator() => Enumerable.Empty<TElement>().GetEnumerator();

            Collections.IEnumerator Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        }

        internal record GroupKey(string? Title, object? ItemsKey);
    }
}
