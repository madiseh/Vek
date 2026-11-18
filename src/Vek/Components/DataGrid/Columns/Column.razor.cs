// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Vek.Extensions;
using Vek.Interfaces;
using Vek.State;
using Vek.Utilities;

namespace Vek
{
    /// <summary>
    /// Base class for columns in a <see cref="VekDataGrid{T}"/> such as <see cref="PropertyColumn{T, TProperty}"/>, <see cref="TemplateColumn{T}"/>, and <see cref="SelectColumn{T}"/>.
    /// </summary>
    /// <typeparam name="T">The kind of item for this column.</typeparam>
    /// <seealso cref="HierarchyColumn{T}" />
    /// <seealso cref="VekDataGrid{T}" />
    /// <seealso cref="PropertyColumn{T, TProperty}" />
    /// <seealso cref="SelectColumn{T}" />
    /// <seealso cref="TemplateColumn{T}" />
    public abstract partial class Column<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : VekComponentBase, IDisposable
    {
        private static readonly RenderFragment<CellContext<T>> EmptyChildContent = _ => builder => { };
        internal ParameterState<bool> HiddenState { get; }
        internal ParameterState<bool> GroupingState { get; }
        internal ParameterState<bool> _groupExpandedState;
        internal ParameterState<int> _groupByOrderState;
        private readonly ParameterState<bool> _allowSearchState;
        private readonly ParameterState<bool> _allowHidingState;
        private readonly ParameterState<bool?> _allowFilteringState;
        private readonly ParameterState<DataGridFilterOperator> _filterOperatorState;
        private readonly ParameterState<DataGridGroupInterval> _groupIntervalState;
        private readonly ParameterState<decimal> _groupIntervalSizeState;
        private readonly ParameterState<GroupIntervalCalculator?> _groupIntervalCalculatorState;
        private readonly ParameterState<DataGridFixedStyle> _fixedStyleState;
        private readonly ParameterState<string?> _widthState;

        internal ParameterState<bool> AllowSearchState => _allowSearchState;
        internal ParameterState<bool> AllowHidingState => _allowHidingState;
        internal ParameterState<bool?> AllowFilteringState => _allowFilteringState;
        internal ParameterState<DataGridFilterOperator> FilterOperatorState => _filterOperatorState;

        /// <summary>
        /// The data grid which owns this column.
        /// </summary>
        [CascadingParameter]
        public VekDataGrid<T> DataGrid { get; set; } = null!;

        /// <summary>
        /// The value stored in this column.
        /// </summary>
        [Parameter] public T? Value { get; set; }

        /// <summary>
        /// Occurs when the <see cref="Value"/> has changed.
        /// </summary>
        [Parameter]
        public EventCallback<T> ValueChanged { get; set; }

        /// <summary>
        /// The display text for this column.
        /// </summary>
        [Parameter]
        public string? Title { get; set; }

        /// <summary>
        /// Hides this column.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter]
        public bool HideSmall { get; set; }

        /// <summary>
        /// The number of columns spanned by this column in the footer.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>.
        /// </remarks>
        [Parameter] public int FooterColSpan { get; set; } = 1;

        /// <summary>
        /// The number of columns spanned by this column in the header.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>1</c>.
        /// </remarks>
        [Parameter] public int HeaderColSpan { get; set; } = 1;

        /// <summary>
        /// The template used to display this column's header.
        /// </summary>
        [Parameter]
        public RenderFragment<HeaderContext<T>>? HeaderTemplate { get; set; }

        public virtual RenderFragment<HeaderContext<T>>? GetHeaderTemplate() => HeaderTemplate;

        /// <summary>
        /// The template used to display this column's value cells.
        /// </summary>
        [Parameter]
        public RenderFragment<CellContext<T>>? CellTemplate { get; set; }

        public virtual RenderFragment<CellContext<T>>? GetCellTemplate() => CellTemplate;

        /// <summary>
        /// The template used to display this column's footer.
        /// </summary>
        [Parameter]
        public RenderFragment<FooterContext<T>>? FooterTemplate { get; set; }

        public virtual RenderFragment<FooterContext<T>>? GetFooterTemplate() => FooterTemplate;

        /// <summary>
        /// The template used to display this column's grouping.
        /// </summary>
        [Parameter]
        public RenderFragment<GroupDefinition<T>>? GroupTemplate { get; set; }

        public virtual RenderFragment<GroupDefinition<T>>? GetGroupTemplate() => GroupTemplate;

        /// <summary>
        /// The template used to display this column's aggregate.
        /// </summary>
        [Parameter]
        public RenderFragment<IEnumerable<T>>? AggregateTemplate { get; set; }

        public virtual RenderFragment<IEnumerable<T>>? GetAggregateTemplate() => AggregateTemplate;

        /// <summary>
        /// The function which groups values in this column.
        /// </summary>
        [Parameter]
        public Func<T, object>? GroupBy { get; set; }

        /// <summary>
        /// Groups column values into built-in date, alphabetic, or numeric intervals.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="DataGridGroupInterval.None"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public DataGridGroupInterval GroupInterval { get; set; }

        /// <summary>
        /// Sets the width of numeric ranges when <see cref="GroupInterval"/> is <see cref="DataGridGroupInterval.NumericRange"/>.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>10</c> and must be greater than zero.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public decimal GroupIntervalSize { get; set; } = 10;

        /// <summary>
        /// Calculates a custom grouping key from each column value.
        /// </summary>
        /// <remarks>
        /// When supplied, this calculator takes precedence over <see cref="GroupInterval"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public GroupIntervalCalculator? GroupIntervalCalculator { get; set; }

        /// <summary>
        /// The order in which values are grouped when there are more than one group
        /// </summary>
        /// <remarks>
        /// Defaults to 0.
        /// </remarks>
        [Parameter, ParameterState]
        public int GroupByOrder { get; set; }

        /// <summary>
        /// Occurs when the <see cref="GroupByOrder"/> property has changed.
        /// </summary>
        [Parameter]
        public EventCallback<int> GroupByOrderChanged { get; set; }

        /// <summary>
        /// Indents the column <c>48px</c> beyond its parent when grouped.
        /// </summary>
        [Parameter]
        public bool GroupIndented { get; set; } = true;

        /// <summary>
        /// Whether groups created from this column are expanded. Toggling the value will Toggle all grouped rows of this column.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        public bool GroupExpanded { get; set; }

        /// <summary>
        /// Occurs when the <see cref="GroupExpanded"/> property has changed.
        /// </summary>
        [Parameter]
        public EventCallback<bool> GroupExpandedChanged { get; set; }

        /// <summary>
        /// Requires a value to be set.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        public bool Required { get; set; } = true;

        #region HeaderCell Properties

        /// <summary>
        /// The CSS class applied to the header.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  Separate multiple classes with spaces.
        /// </remarks>
        [Parameter]
        public string? HeaderClass { get; set; }

        /// <summary>
        /// The function which calculates CSS classes for the header.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  Separate multiple classes with spaces.
        /// </remarks>
        [Parameter]
        public Func<IEnumerable<T>, string>? HeaderClassFunc { get; set; }

        /// <summary>
        /// The CSS style applied to this column's header.
        /// </summary>
        [Parameter]
        public string? HeaderStyle { get; set; }

        /// <summary>
        /// The function which calculates CSS styles for the header.
        /// </summary>
        [Parameter]
        public Func<IEnumerable<T>, string>? HeaderStyleFunc { get; set; }

        /// <summary>
        /// Sorts values in this column.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  When set, this overrides the <see cref="VekDataGrid{T}.SortMode"/> property.
        /// </remarks>
        [Parameter]
        public virtual bool? Sortable { get; set; }

        /// <summary>
        /// Allows this column's width to be changed.
        /// </summary>
        [Parameter]
        public virtual bool? Resizable { get; set; }

        /// <summary>
        /// Allows this column to be reordered via drag-and-drop operations.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  When set, this overrides the <see cref="VekDataGrid{T}.DragDropColumnReordering"/> property.
        /// </remarks>
        [Parameter]
        public virtual bool? DragAndDropEnabled { get; set; }

        /// <summary>
        /// Whether this column is included in data grid search operations and highlighting.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Filtering)]
        public bool AllowSearch { get; set; } = true;

        /// <summary>
        /// Allows filters to be used on this column.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  When set, this overrides the <see cref="VekDataGrid{T}.Filterable"/> property.
        /// </remarks>
        [Parameter]
        public virtual bool? Filterable { get; set; }

        /// <summary>
        /// Specifies whether filtering is permitted for this column.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>. When set, this overrides <see cref="VekDataGrid{T}.Filterable"/> and <see cref="VekDataGrid{T}.ShowFilterRow"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Filtering)]
        public bool? AllowFiltering { get; set; }

        /// <summary>
        /// The default comparison operator used when filtering this column in the filter row.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="DataGridFilterOperator.Default"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Filtering)]
        public DataGridFilterOperator FilterOperator { get; set; } = DataGridFilterOperator.Default;

        /// <summary>
        /// Shows the filter icon.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  When set, this overrides the <see cref="VekDataGrid{T}.ShowFilterIcons"/> property.
        /// </remarks>
        [Parameter]
        public bool? ShowFilterIcon { get; set; }

        /// <summary>
        /// Allows this column to be hidden.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  When set, this overrides the <see cref="VekDataGrid{T}.Hideable"/> property.
        /// </remarks>
        [Parameter]
        public bool? Hideable { get; set; }

        /// <summary>
        /// Specifies whether this column can be hidden via the column chooser or visibility customization.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>. When set to <c>false</c>, this column cannot be concealed.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public bool AllowHiding { get; set; } = true;

        /// <summary>
        /// Hides this column.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>false</c>.
        /// </remarks>
        [Parameter, ParameterState]
        public bool Hidden { get; set; }

        /// <summary>
        /// Occurs when the <see cref="Hidden"/> property has changed.
        /// </summary>
        [Parameter]
        public EventCallback<bool> HiddenChanged { get; set; }

        /// <summary>
        /// Shows options for this column.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  When set, this overrides the <see cref="VekDataGrid{T}.ShowColumnOptions"/> property.
        /// </remarks>
        [Parameter] public virtual bool? ShowColumnOptions { get; set; }

        /// <summary>
        /// The comparison used for values in this column.
        /// </summary>
        [Parameter]
        public IComparer<object?>? Comparer { get; set; } = null;

        /// <summary>
        /// The function used to sort values in this column.
        /// </summary>
        [Parameter]
        public Func<T, object?> SortBy
        {
            get
            {
                return GetLocalSortFunc();
            }
            set
            {
                _sortBy = value;
            }
        }

        /// <summary>
        /// The sorting direction applied when <see cref="Sortable"/> is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="SortDirection.None"/>.
        /// </remarks>
        [Parameter]
        public SortDirection InitialDirection { get; set; } = SortDirection.None;

        /// <summary>
        /// The sort direction applied when this column is unsorted and the header is clicked for the first time.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="SortDirection.Ascending"/>.
        /// </remarks>
        [Parameter]
        public SortDirection InitialSortDirection { get; set; } = SortDirection.Ascending;

        /// <summary>
        /// The icon shown when <see cref="Sortable"/> is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>, which falls back to <see cref="VekDataGrid{T}.SortIcon"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Appearance)]
        [Obsolete("Column-level sort icon customization is no longer supported. Configure VekDataGrid.SortIcon or use HeaderTemplate for full header customization.", true)]
        public string? SortIcon { get; set; }

        /// <summary>
        /// Allows values in this column to be grouped.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.  When set, this overrides the <see cref="VekDataGrid{T}.Groupable"/> property.
        /// </remarks>
        [Parameter]
        public bool? Groupable { get; set; }

        /// <summary>
        /// Indicates whether this column is currently grouped.
        /// </summary>
        [Parameter, ParameterState]
        public bool Grouping { get; set; }

        /// <summary>
        /// Occurs when the <see cref="Grouping"/> property has changed.
        /// </summary>
        [Parameter]
        public EventCallback<bool> GroupingChanged { get; set; }

        /// <summary>
        /// Fixes this column to the left side.
        /// </summary>
        /// <remarks>
        /// When <c>true</c>, this column will be visible even as the container is scrolled horizontally.
        /// </remarks>
        [Parameter]
        public bool StickyLeft { get; set; }

        /// <summary>
        /// Fixes this column to the right side.
        /// </summary>
        /// <remarks>
        /// When <c>true</c>, this column will be visible even as the container is scrolled horizontally.
        /// </remarks>
        [Parameter]
        public bool StickyRight { get; set; }

        /// <summary>
        /// Fixes or pins this column to the left or right edge of the viewport.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="DataGridFixedStyle.None"/>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public DataGridFixedStyle FixedStyle { get; set; }

        /// <summary>
        /// Occurs when the <see cref="FixedStyle"/> property has changed.
        /// </summary>
        [Parameter]
        public EventCallback<DataGridFixedStyle> FixedStyleChanged { get; set; }

        /// <summary>
        /// Allows this column to be pinned to the left or right edge of the viewport.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>, inheriting from <see cref="VekDataGrid{T}.AllowColumnPinning"/>.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.DataGrid.Behavior)]
        public bool? AllowPinning { get; set; }

        /// <summary>
        /// The width of this column (e.g. "120px", "15%").
        /// </summary>
        /// <remarks>
        /// Defaults to <c>null</c>.
        /// </remarks>
        [Parameter, ParameterState]
        [Category(CategoryTypes.DataGrid.Appearance)]
        public string? Width { get; set; }

        /// <summary>
        /// Sets the fixed pinning style of this column.
        /// </summary>
        /// <param name="fixedStyle">The target fixed style.</param>
        public Task SetFixedStyleAsync(DataGridFixedStyle fixedStyle)
        {
            return _fixedStyleState.SetValueAsync(fixedStyle);
        }

        /// <summary>
        /// Resolves the effective fixed pinning style, taking into account <see cref="FixedStyle"/>, <see cref="StickyLeft"/>, and <see cref="StickyRight"/>.
        /// </summary>
        public DataGridFixedStyle EffectiveFixedStyle
        {
            get
            {
                var fixedStyle = _fixedStyleState.Value;
                if (fixedStyle != DataGridFixedStyle.None)
                {
                    return fixedStyle;
                }

                if (StickyLeft)
                {
                    return DataGridFixedStyle.Left;
                }

                if (StickyRight)
                {
                    return DataGridFixedStyle.Right;
                }

                return DataGridFixedStyle.None;
            }
        }

        /// <summary>
        /// Indicates whether this column is pinned to the left edge of the grid.
        /// </summary>
        public bool IsPinnedLeft => EffectiveFixedStyle == DataGridFixedStyle.Left;

        /// <summary>
        /// Indicates whether this column is pinned to the right edge of the grid.
        /// </summary>
        public bool IsPinnedRight => EffectiveFixedStyle == DataGridFixedStyle.Right;

        internal double? StickyLeftOffset { get; set; }

        internal double? StickyRightOffset { get; set; }

        internal string? StickyLeftOffsetPx => StickyLeftOffset.HasValue ? $"{StickyLeftOffset.Value.ToString("0.##", CultureInfo.InvariantCulture)}px" : null;

        internal string? StickyRightOffsetPx => StickyRightOffset.HasValue ? $"{StickyRightOffset.Value.ToString("0.##", CultureInfo.InvariantCulture)}px" : null;

        internal bool IsLastStickyLeft { get; set; }

        internal bool IsFirstStickyRight { get; set; }

        internal double GetEstimatedWidthPx()
        {
            if (HeaderCell?.Width.HasValue == true && HeaderCell.Width.Value > 0)
            {
                return HeaderCell.Width.Value;
            }

            var widthStr = _widthState.Value;
            if (!string.IsNullOrWhiteSpace(widthStr))
            {
                if (widthStr.EndsWith("px", StringComparison.OrdinalIgnoreCase) &&
                    double.TryParse(widthStr.Substring(0, widthStr.Length - 2).Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedPx))
                {
                    return parsedPx;
                }

                if (double.TryParse(widthStr.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedRaw))
                {
                    return parsedRaw;
                }
            }

            if (!string.IsNullOrWhiteSpace(HeaderStyle))
            {
                var match = Regex.Match(HeaderStyle, @"width\s*:\s*([0-9.]+)(px)?", RegexOptions.IgnoreCase);
                if (match.Success && double.TryParse(match.Groups[1].Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsedStylePx))
                {
                    return parsedStylePx;
                }
            }

            if (Tag?.ToString() == "hierarchy-column" || Tag?.ToString() == "select-column")
            {
                return 48.0;
            }

            return 120.0;
        }

        /// <summary>
        /// The template used to display this column's filter.
        /// </summary>
        [Parameter]
        public RenderFragment<FilterContext<T>>? FilterTemplate { get; set; }

        /// <summary>
        /// The operators to use for this column's filter.
        /// </summary>
        [Parameter]
        public HashSet<string> FilterOperators { get; set; } = [];

        /// <summary>
        /// The unique identifier for this column.
        /// </summary>
        public string? Identifier { get; set; }

        private CultureInfo? _culture;

        /// <summary>
        /// The culture used to parse, filter, and display values in this column.
        /// </summary>
        /// <remarks>
        /// Defaults to <see cref="VekDataGrid{T}.Culture"/>.  When neither value is set, formatting uses the current culture.
        /// </remarks>
        [Parameter]
        [Category(CategoryTypes.Table.Appearance)]
        public CultureInfo? Culture
        {
            get => _culture ?? DataGrid?.Culture;
            set
            {
                _culture = value;
            }
        }

        #endregion

        #region Cell Properties

        /// <summary>
        /// The CSS classes to apply to the cell.
        /// </summary>
        /// <remarks>
        /// Multiple classes must be separated by spaces.
        /// </remarks>
        [Parameter]
        public string? CellClass { get; set; }

        /// <summary>
        /// The function used to determine CSS classes for this cell.
        /// </summary>
        /// <remarks>
        /// Multiple classes must be separated by spaces.
        /// </remarks>
        [Parameter]
        public Func<T, string>? CellClassFunc { get; set; }

        /// <summary>
        /// The CSS styles to apply to this cell.
        /// </summary>
        [Parameter]
        public string? CellStyle { get; set; }

        /// <summary>
        /// The function which calculates CSS styles for this cell.
        /// </summary>
        [Parameter]
        public Func<T, string>? CellStyleFunc { get; set; }

        /// <summary>
        /// Allows editing for this cell.
        /// </summary>
        /// <remarks>
        /// Defaults to <c>true</c>.
        /// </remarks>
        [Parameter]
        public bool Editable { get; set; } = true;

        /// <summary>
        /// The template for editing values in this cell.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When <see cref="VekDataGrid{T}.EditMode"/> is <see cref="DataGridEditMode.Form"/>, the built-in Save button
        /// automatically invokes <see cref="VekDataGrid{T}.CommittedItemChanges"/> — no extra handling is required.
        /// </para>
        /// <para>
        /// When <see cref="VekDataGrid{T}.EditMode"/> is <see cref="DataGridEditMode.Cell"/>, standard columns commit
        /// their value automatically on change. Custom controls inside an <see cref="EditTemplate"/> do not trigger
        /// <see cref="VekDataGrid{T}.CommittedItemChanges"/> automatically. Track changes directly in a value-changed
        /// handler instead:
        /// </para>
        /// <code>
        /// &lt;EditTemplate&gt;
        ///     &lt;VekDatePicker Date="@context.Item.Date"
        ///         DateChanged="@(d => { context.Item.Date = d; TrackChange(context.Item); })" /&gt;
        /// &lt;/EditTemplate&gt;
        /// </code>
        /// </remarks>
        [Parameter]
        public RenderFragment<CellContext<T>>? EditTemplate { get; set; }

        #endregion

        #region FooterCell Properties

        /// <summary>
        /// The CSS classes applied to this column's footer.
        /// </summary>
        /// <remarks>
        /// Multiple classes must be separated by spaces.
        /// </remarks>
        [Parameter]
        public string? FooterClass { get; set; }

        /// <summary>
        /// The function which calculates CSS classes for this column's footer.
        /// </summary>
        /// <remarks>
        /// Multiple classes must be separated by spaces.
        /// </remarks>
        [Parameter]
        public Func<IEnumerable<T>, string>? FooterClassFunc { get; set; }

        /// <summary>
        /// The CSS styles to apply to this column's footer.
        /// </summary>
        [Parameter]
        public string? FooterStyle { get; set; }

        /// <summary>
        /// The function which calculates CSS styles for this column's footer.
        /// </summary>
        [Parameter]
        public Func<IEnumerable<T>, string>? FooterStyleFunc { get; set; }

        /// <summary>
        /// Allows the footer to be selected.
        /// </summary>
        [Parameter]
        public bool EnableFooterSelection { get; set; }

        /// <summary>
        /// The function which calculates aggregates for this column.
        /// </summary>
        [Parameter]
        public AggregateDefinition<T>? AggregateDefinition { get; set; }

        #endregion

        internal string HeaderClassname =>
            new CssBuilder("vek-table-cell")
                .AddClass("vek-table-cell-hide", HideSmall)
                .AddClass("sticky-left", IsPinnedLeft)
                .AddClass("sticky-right", IsPinnedRight)
                .AddClass("sticky-left-last", IsLastStickyLeft)
                .AddClass("sticky-right-first", IsFirstStickyRight)
                .AddClass(Class)
                .Build();

        internal string FooterClassname =>
            new CssBuilder("vek-table-cell")
                .AddClass("footer-cell")
                .AddClass("vek-table-cell-hide", HideSmall)
                .AddClass("sticky-left", IsPinnedLeft)
                .AddClass("sticky-right", IsPinnedRight)
                .AddClass("sticky-left-last", IsLastStickyLeft)
                .AddClass("sticky-right-first", IsFirstStickyRight)
                .AddClass(Class)
                .Build();

        #region Computed Properties

        internal Type dataType
        {
            get
            {
                Debug.Assert(PropertyType is not null);
                return PropertyType;
            }
        }

        internal bool isNumber
        {
            get
            {
                return TypeIdentifier.IsNumber(PropertyType);
            }
        }

        internal bool hideable
        {
            get
            {
                if (!_allowHidingState.Value)
                {
                    return false;
                }
                return Hideable ?? DataGrid?.Hideable ?? false;
            }
        }

        internal bool sortable
        {
            get
            {
                return Sortable ?? (DataGrid?.SortMode != SortMode.None);
            }
        }

        internal bool groupable
        {
            get
            {
                return Groupable ?? DataGrid?.Groupable ?? false;
            }
        }

        internal bool filterable
        {
            get
            {
                if (_allowFilteringState.Value.HasValue)
                {
                    return _allowFilteringState.Value.Value;
                }

                if (Filterable.HasValue)
                {
                    return Filterable.Value;
                }

                return (DataGrid?.GetState(x => x.ShowFilterRow) == true) || (DataGrid?.Filterable == true);
            }
        }

        #endregion

        internal int SortIndex { get; set; } = -1;
        internal HeaderCell<T> HeaderCell { get; set; } = null!;

        private Func<T, object?>? _sortBy;
        internal Func<T, object?>? groupBy;

        // These are set in OnInitialized() so they can't be null
        internal HeaderContext<T> headerContext = null!;
        private FilterContext<T> filterContext = null!;

        // Cached filter definition to avoid repeated lookups during rendering
        private IFilterDefinition<T>? _cachedFilterDefinition;

        /// <summary>
        /// The context used for filtering values in this column.
        /// </summary>
        public FilterContext<T> FilterContext
        {
            get
            {
                Debug.Assert(DataGrid is not null);

                // Check if the cached filter definition is still valid in the grid's FilterDefinitions
                var existingFilterDefinition = DataGrid.FilterDefinitions.FirstOrDefault(fd => fd.Column == this);

                if (existingFilterDefinition != null)
                {
                    // Use the existing filter definition from the grid
                    if (_cachedFilterDefinition != existingFilterDefinition)
                    {
                        _cachedFilterDefinition = existingFilterDefinition;
                        filterContext.FilterDefinition = existingFilterDefinition;
                    }
                }
                else
                {
                    // No filter exists in the grid - check if we have a stale reference or need to create a new one
                    if (_cachedFilterDefinition != null || filterContext.FilterDefinition == null)
                    {
                        // Clear the stale cached reference and create a new filter definition
                        _cachedFilterDefinition = null;
                        var operators = GetFilterOperators(FieldType.Identify(PropertyType));
                        var filterDefinition = DataGrid.CreateFilterDefinitionInstance();
                        filterDefinition.Title = Title;
                        filterDefinition.Operator = operators.FirstOrDefault();
                        filterDefinition.Column = this;
                        filterContext.FilterDefinition = filterDefinition;
                    }
                }

                return filterContext;
            }
        }

        protected Column()
        {
            using var registerScope = CreateRegisterScope();
            _allowSearchState = registerScope.RegisterParameter<bool>(nameof(AllowSearch))
                .WithParameter(() => AllowSearch);
            _allowHidingState = registerScope.RegisterParameter<bool>(nameof(AllowHiding))
                .WithParameter(() => AllowHiding);
            _allowFilteringState = registerScope.RegisterParameter<bool?>(nameof(AllowFiltering))
                .WithParameter(() => AllowFiltering);
            _filterOperatorState = registerScope.RegisterParameter<DataGridFilterOperator>(nameof(FilterOperator))
                .WithParameter(() => FilterOperator);
            _groupIntervalState = registerScope.RegisterParameter<DataGridGroupInterval>(nameof(GroupInterval))
                .WithParameter(() => GroupInterval)
                .WithChangeHandler(OnGroupIntervalParameterChanged);
            _groupIntervalSizeState = registerScope.RegisterParameter<decimal>(nameof(GroupIntervalSize))
                .WithParameter(() => GroupIntervalSize)
                .WithChangeHandler(OnGroupIntervalParameterChanged);
            _groupIntervalCalculatorState = registerScope.RegisterParameter<GroupIntervalCalculator?>(nameof(GroupIntervalCalculator))
                .WithParameter(() => GroupIntervalCalculator)
                .WithChangeHandler(OnGroupIntervalParameterChanged);
            HiddenState = registerScope.RegisterParameter<bool>(nameof(Hidden))
                .WithParameter(() => Hidden)
                .WithEventCallback(() => HiddenChanged);
            GroupingState = registerScope.RegisterParameter<bool>(nameof(Grouping))
                .WithParameter(() => Grouping)
                .WithEventCallback(() => GroupingChanged)
                .WithChangeHandler(OnGroupingParameterChangedAsync);
            _groupExpandedState = registerScope.RegisterParameter<bool>(nameof(GroupExpanded))
                .WithParameter(() => GroupExpanded)
                .WithEventCallback(() => GroupExpandedChanged)
                .WithChangeHandler(OnGroupExpandedChangedAsync);
            _groupByOrderState = registerScope.RegisterParameter<int>(nameof(GroupByOrder))
                .WithParameter(() => GroupByOrder)
                .WithEventCallback(() => GroupByOrderChanged)
                .WithChangeHandler(OnGroupByOrderChangedAsync);
            _fixedStyleState = registerScope.RegisterParameter<DataGridFixedStyle>(nameof(FixedStyle))
                .WithParameter(() => FixedStyle)
                .WithEventCallback(() => FixedStyleChanged)
                .WithChangeHandler(OnFixedStyleParameterChangedAsync);
            _widthState = registerScope.RegisterParameter<string?>(nameof(Width))
                .WithParameter(() => Width)
                .WithChangeHandler(OnWidthParameterChangedAsync);
        }

        private void OnFixedStyleParameterChangedAsync() => DataGrid?.OnColumnFixedStyleChanged(this);

        private void OnWidthParameterChangedAsync() => DataGrid?.UpdateStickyColumnOffsets();

        private void OnGroupingParameterChangedAsync() => DataGrid?.GroupItems();

        private void OnGroupIntervalParameterChanged() => DataGrid?.GroupItems();

        private void OnGroupByOrderChangedAsync() => DataGrid?.GroupItems();

        private void OnGroupExpandedChangedAsync() => DataGrid?.GroupItems();

        protected override void OnInitialized()
        {
            Debug.Assert(DataGrid is not null);

            if (FilterOperators.Count > 0)
            {
                var defaultOperators = Vek.FilterOperator.GetOperatorByDataType(PropertyType);
                var invalidOperators = FilterOperators.Where(@operator => !defaultOperators.Contains(@operator)).ToArray();

                if (invalidOperators.Length > 0)
                {
                    throw new ArgumentException($"Invalid filter operators for {PropertyType?.Name}: {string.Join(", ", invalidOperators)}");
                }
            }

            base.OnInitialized();

            groupBy = GroupBy;

            DataGrid.AddColumn(this);

            // Add the HeaderContext
            headerContext = new HeaderContext<T>(DataGrid);

            // Add the FilterContext
            filterContext = new FilterContext<T>(DataGrid);
        }

        internal IReadOnlyCollection<string> GetFilterOperators(FieldType fieldType)
        {
            if (FilterOperators.Count == 0)
            {
                return Vek.FilterOperator.GetOperatorByDataType(fieldType);
            }
            else
            {
                return FilterOperators;
            }
        }

        internal Func<T, object?> GetLocalSortFunc()
        {
            if (_sortBy == null)
            {
                if (this is TemplateColumn<T>)
                {
                    _sortBy = x => true;
                }
                else
                    _sortBy = PropertyFunc;
            }

            return _sortBy;
        }

        internal void CompileGroupBy()
        {
            if (GroupBy is not null)
            {
                groupBy = GroupBy;
            }
            else if (_groupIntervalCalculatorState.Value is not null || _groupIntervalState.Value != DataGridGroupInterval.None)
            {
                groupBy = item =>
                {
                    object? value;
                    if (typeof(T) == typeof(IDictionary<string, object>))
                    {
                        value = PropertyName is not null && item is IDictionary<string, object> values && values.TryGetValue(PropertyName, out var fieldValue)
                            ? fieldValue
                            : null;
                    }
                    else
                    {
                        value = PropertyFunc(item);
                    }

                    var calculator = _groupIntervalCalculatorState.Value;
                    return calculator is not null
                        ? calculator(value)!
                        : DataGridGroupIntervalEvaluator.Evaluate(value, _groupIntervalState.Value, _groupIntervalSizeState.Value)!;
                };
            }
            else if (groupBy == null && !string.IsNullOrWhiteSpace(PropertyName))
            {
                var type = typeof(T);
                // set the default GroupBy
                if (type == typeof(IDictionary<string, object>))
                {
                    groupBy = x => (x as IDictionary<string, object>)?[PropertyName];
                }
                else
                {
                    groupBy = PropertyFunc;
                }
            }
        }

        // Allows child components to change column grouping.
        internal async Task SetGroupingAsync(bool group)
        {
            await GroupingState.SetValueAsync(group);
        }

        /// <summary>
        /// This method's sole purpose is for the DataGrid to remove grouping in mass.
        /// </summary>
        internal async Task RemoveGrouping()
        {
            if (GroupingState.Value)
            {
                await GroupingState.SetValueAsync(false);
            }
        }

        /// <summary>
        /// Hides this column.
        /// </summary>
        public Task HideAsync()
        {
            if (!_allowHidingState.Value)
            {
                return Task.CompletedTask;
            }
            return HiddenState.SetValueAsync(true);
        }

        /// <summary>
        /// Shows this column.
        /// </summary>
        public Task ShowAsync()
        {
            return HiddenState.SetValueAsync(false);
        }

        /// <summary>
        /// Hides or shows this column.
        /// </summary>
        public async Task ToggleAsync()
        {
            Debug.Assert(DataGrid is not null);
            if (!HiddenState.Value && !_allowHidingState.Value)
            {
                return;
            }
            await HiddenState.SetValueAsync(!HiddenState.Value);
            ((IVekStateHasChanged)DataGrid).StateHasChanged();
        }

        /// <summary>
        /// Releases resources used by this column.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by this column.
        /// </summary>
        /// <param name="disposing">When <c>true</c>, managed resources should be released.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (DataGrid != null)
                DataGrid.RemoveColumn(this);
        }

        #region Abstract Members

        protected internal virtual LambdaExpression? PropertyExpression { get; }

        protected internal virtual Func<T, bool> GetFilterExpression()
        {
            return x => true;
        }

        /// <summary>
        /// The name of the property used for sorting this column's values.
        /// </summary>
        public virtual string? PropertyName { get; }

        protected internal virtual string? ContentFormat { get; }

        protected internal abstract object? CellContent(T item);

        /// <summary>
        /// Gets the cell content value for the specified item.
        /// </summary>
        /// <param name="item">The item to retrieve the cell content for.</param>
        /// <returns>The cell content value, or <c>null</c> if not available.</returns>
        public object? GetCellContent(T item) => CellContent(item);

        protected internal abstract object? PropertyFunc(T item);

        protected internal virtual Type? PropertyType { get; }

        protected internal abstract void SetProperty(object? item, object? value);

        #endregion
    }
}
