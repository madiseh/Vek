// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.AspNetCore.Components;
using Vek.State;

namespace Vek;

/// <summary>
/// Defines a column within a <see cref="VekTreeList{T}"/>.
/// </summary>
/// <typeparam name="T">The type of data item displayed in the tree list.</typeparam>
public partial class VekTreeListColumn<T> : VekComponentBase, IDisposable
{
    private readonly ParameterState<string?> _widthState;
    private readonly ParameterState<SortDirection> _sortDirectionState;
    private readonly ParameterState<bool> _visibleState;

    public VekTreeListColumn()
    {
        using var registerScope = CreateRegisterScope();
        _widthState = registerScope.RegisterParameter<string?>(nameof(Width))
            .WithParameter(() => Width)
            .WithEventCallback(() => WidthChanged)
            .WithChangeHandler(OnWidthStateChangedAsync);
        _sortDirectionState = registerScope.RegisterParameter<SortDirection>(nameof(SortDirection))
            .WithParameter(() => SortDirection)
            .WithEventCallback(() => SortDirectionChanged)
            .WithChangeHandler(OnSortDirectionStateChangedAsync);
        _visibleState = registerScope.RegisterParameter<bool>(nameof(Visible))
            .WithParameter(() => Visible)
            .WithChangeHandler(OnVisibleStateChangedAsync);
    }

    internal ElementReference HeaderElement { get; set; }

    internal ElementReference ResizerElement { get; set; }

    internal double? RuntimeWidthPx { get; set; }

    internal int SortOrderIndex { get; set; } = -1;

    [CascadingParameter]
    private VekTreeList<T>? TreeList { get; set; }

    /// <summary>
    /// The header text displayed for this column.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public string? Title { get; set; }

    /// <summary>
    /// Selects the value to display and sort for each item in this column.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Data)]
    public Func<T, object?>? Field { get; set; }

    /// <summary>
    /// Selects a custom sort key for this column. When omitted, <see cref="Field"/> is used.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Behavior)]
    public Func<T, object?>? SortBy { get; set; }

    /// <summary>
    /// The format string applied when rendering <see cref="IFormattable"/> cell values.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public string? Format { get; set; }

    /// <summary>
    /// The CSS width of the column (for example, <c>240px</c> or <c>30%</c>).
    /// </summary>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Appearance)]
    public string? Width { get; set; }

    /// <summary>
    /// Occurs when <see cref="Width"/> changes.
    /// </summary>
    [Parameter]
    public EventCallback<string?> WidthChanged { get; set; }

    /// <summary>
    /// The minimum width of the column in pixels during interactive resizing.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>60</c>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public int MinWidth { get; set; } = 60;

    /// <summary>
    /// The optional maximum width of the column in pixels during interactive resizing.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public int? MaxWidth { get; set; }

    /// <summary>
    /// Marks this column as the primary hierarchical column that displays indentation and expand/collapse controls.
    /// </summary>
    /// <remarks>
    /// When no column sets this property to <c>true</c>, the first visible column automatically serves as the tree column.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Behavior)]
    public bool IsTreeColumn { get; set; }

    /// <summary>
    /// Controls whether this column can be sorted by clicking its header.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>, inheriting <see cref="VekTreeList{T}.Sortable"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Behavior)]
    public bool? Sortable { get; set; }

    /// <summary>
    /// The active sort direction for this column.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="SortDirection.None"/>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Behavior)]
    public SortDirection SortDirection { get; set; } = SortDirection.None;

    /// <summary>
    /// Occurs when <see cref="SortDirection"/> changes.
    /// </summary>
    [Parameter]
    public EventCallback<SortDirection> SortDirectionChanged { get; set; }

    /// <summary>
    /// Controls whether this column can be interactively resized.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>null</c>, inheriting <see cref="VekTreeList{T}.Resizable"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Behavior)]
    public bool? Resizable { get; set; }

    /// <summary>
    /// Controls whether this column is visible.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    [Parameter, ParameterState]
    [Category(CategoryTypes.TreeView.Appearance)]
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Sets the horizontal alignment of header and cell content in this column.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="HorizontalAlignment.Start"/>.
    /// </remarks>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public HorizontalAlignment Alignment { get; set; } = HorizontalAlignment.Start;

    /// <summary>
    /// Customizes the header cell content for this column.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Data)]
    public RenderFragment<VekTreeListColumn<T>>? HeaderTemplate { get; set; }

    /// <summary>
    /// Customizes the data cell content rendered for each item in this column.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Data)]
    public RenderFragment<ITreeItemData<T>>? CellTemplate { get; set; }

    /// <summary>
    /// Additional CSS classes applied to this column's header cell.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public string? HeaderClass { get; set; }

    /// <summary>
    /// Additional inline CSS styles applied to this column's header cell.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public string? HeaderStyle { get; set; }

    /// <summary>
    /// Additional CSS classes applied to data cells in this column.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public string? CellClass { get; set; }

    /// <summary>
    /// Additional inline CSS styles applied to data cells in this column.
    /// </summary>
    [Parameter]
    [Category(CategoryTypes.TreeView.Appearance)]
    public string? CellStyle { get; set; }

    internal string? GetWidth() => _widthState.Value;

    internal SortDirection GetSortDirection() => _sortDirectionState.Value;

    internal bool GetVisible() => _visibleState.Value;

    internal Task SetSortDirectionAsync(SortDirection direction) => _sortDirectionState.SetValueAsync(direction);

    internal Task SetWidthAsync(string? width) => _widthState.SetValueAsync(width);

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        TreeList?.RegisterColumn(this);
    }

    private Task OnWidthStateChangedAsync(ParameterChangedEventArgs<string?> args)
    {
        RuntimeWidthPx = null;
        TreeList?.NotifyColumnChanged();
        return Task.CompletedTask;
    }

    private Task OnSortDirectionStateChangedAsync(ParameterChangedEventArgs<SortDirection> args)
    {
        TreeList?.SyncColumnSortDirection(this, args.Value);
        return Task.CompletedTask;
    }

    private Task OnVisibleStateChangedAsync(ParameterChangedEventArgs<bool> args)
    {
        TreeList?.NotifyColumnChanged();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Unregisters this column from its parent <see cref="VekTreeList{T}"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases managed resources used by this column.
    /// </summary>
    /// <param name="disposing">When <c>true</c>, managed resources should be released.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            TreeList?.UnregisterColumn(this);
        }
    }
}
