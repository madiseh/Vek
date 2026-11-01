// Copyright (c) 2026 AMIN SABAGHI
// Licensed under the MIT License. See the LICENSE file in the project root for more information.
// Created: 2026-11-01

namespace Vek.WPF.DataGrid.Components;

public partial class VekDataGrid
{
    private bool _projecting;
    private bool _nativeEditStarting;
    private bool _completingNativeEdit;
    private bool _suppressRowActivationSelection;

    /// <summary>Gets or sets the shared editing policy.</summary>
    public VekDataGridEditMode EditMode { get; set; } = VekDataGridEditMode.Row;
    /// <summary>Gets or sets the draft clone operation.</summary>
    public Func<object, object>? CloneItem { get; set; }
    /// <summary>Gets or sets row selection eligibility.</summary>
    public Func<object, bool>? IsRowSelectable { get; set; }
    /// <summary>Gets or sets whether row activation selects rows.</summary>
    public bool RowClickSelection { get; set; } = true;
    /// <summary>Gets or sets equality for selection reconciliation.</summary>
    public IEqualityComparer<object>? SelectionComparer { get; set; }
    /// <summary>Gets or sets equality for row-detail reconciliation.</summary>
    public IEqualityComparer<object>? RowDetailsComparer { get; set; }
    /// <summary>Gets or sets the single-expanded-row policy.</summary>
    public bool SingleExpandedRow { get; set; }
    /// <summary>Gets or sets whether filtered-out selections are retained.</summary>
    public VekDataGridFilteredSelectionMode FilteredSelectionMode { get; set; }
    /// <summary>Gets or sets caller-owned persistence for an edited draft.</summary>
    public Func<object, Task<bool>>? CommitEditHandler { get; set; }
    /// <summary>Occurs after a shared edit starts.</summary>
    public event Action<object>? EditStarted;
    /// <summary>Occurs after a shared draft is committed.</summary>
    public event Action<object>? ItemCommitted;
    /// <summary>Occurs after a shared edit is cancelled.</summary>
    public event Action<object>? EditCancelled;

    private void PrepareHostState()
    {
        _sourceItems ??= ItemsSource;
        var rows = _sourceItems?.Cast<object>() ?? [];
        var state = _engine.Interaction;
        state.Items = rows;
        state.FilteredItemsProvider = () => _engine.GetFilteredAndSortedItems(state.Items);
        state.VisibleItemsProvider = () => _engine.GetLocalData(state.Items).Items;
        state.TotalItemsProvider = () => state.FilteredItems.Count;
        state.SelectionMode = SelectionMode == WpfDataGridSelectionMode.Single ? VekDataGridSelectionMode.Single : VekDataGridSelectionMode.Multiple;
        state.RowClickSelection = RowClickSelection;
        state.IsRowSelectable = IsRowSelectable;
        state.SelectionComparer = SelectionComparer ?? EqualityComparer<object>.Default;
        state.SelectedItems = state.SelectedItems.Where(item => !Items.Contains(item)).Concat(SelectedItems.Cast<object>()).Distinct(state.SelectionComparer).ToArray();
        state.SelectedItem = SelectedItem ?? state.SelectedItems.FirstOrDefault();
        state.FilteredSelectionMode = FilteredSelectionMode;
        state.DetailsComparer = RowDetailsComparer ?? state.SelectionComparer;
        state.SingleExpandedRow = SingleExpandedRow;
        state.EditMode = IsReadOnly ? VekDataGridEditMode.None : EditMode;
        state.CloneItem = CloneItem;
        state.ColumnResizingEnabled = CanUserResizeColumns;
    }

    private ValueTask UpdateNativeAsync(Action action)
    {
        if (Dispatcher.CheckAccess()) { action(); return ValueTask.CompletedTask; }
        return new ValueTask(Dispatcher.InvokeAsync(action).Task);
    }

    /// <summary>Refreshes native rows after shared paging.</summary>
    public ValueTask AfterPageChangeAsync(bool pageSizeChanged) => UpdateNativeAsync(RefreshLocalItems);

    /// <summary>Projects shared visibility, width, and order into native columns.</summary>
    public ValueTask AfterColumnChangeAsync(bool orderChanged) => UpdateNativeAsync(() =>
    {
        var index = 0;
        foreach (var definition in _engine.Columns)
        {
            var column = Columns.FirstOrDefault(candidate => candidate.SortMemberPath == definition.Key);
            if (column is null) continue;
            column.Visibility = definition.Visible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            if (definition.Width is int width) column.Width = new WpfDataGridLength(width);
            if (orderChanged) column.DisplayIndex = index;
            index++;
        }
    });

    /// <summary>Projects shared selection into native controls without treating the projection as a user gesture.</summary>
    public ValueTask AfterSelectionChangeAsync() => UpdateNativeAsync(ApplyNativeSelection);

    private void ApplyNativeSelection()
    {
        _projecting = true;
        try
        {
            if (SelectionMode == WpfDataGridSelectionMode.Single)
                SelectedItem = _engine.Interaction.SelectedItem is { } selected && Items.Contains(selected) ? selected : null;
            else
            {
                SelectedItems.Clear();
                foreach (var item in _engine.Interaction.SelectedItems)
                    if (Items.Contains(item)) SelectedItems.Add(item);
            }
        }
        finally { _projecting = false; }
    }

    /// <summary>Projects shared detail state into native templates and realized rows.</summary>
    public ValueTask AfterRowDetailsChangeAsync() => UpdateNativeAsync(() =>
    {
        var column = _engine.Columns.FirstOrDefault(candidate => candidate.IsHierarchyColumn && candidate.DetailTemplate is not null);
        if (column is not null)
            RowDetailsTemplateSelector = new WpfTemplateSelector(item => item is null ? null : column.DetailTemplate!(
                new VekDataGridRowDetailContext<object>(item, this.IsRowExpanded(item), () => this.ToggleRowDetailsAsync(item))));
        foreach (var item in Items)
            if (ItemContainerGenerator.ContainerFromItem(item) is WpfDataGridRow row)
                row.DetailsVisibility = this.IsRowExpanded(item) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    });

    /// <summary>Refreshes native grouping after a shared group transition.</summary>
    public ValueTask AfterGroupChangeAsync() => UpdateNativeAsync(RefreshLocalItems);

    /// <summary>Opens the native editor for a shared draft.</summary>
    public ValueTask AfterEditStartedAsync(object item) => UpdateNativeAsync(() =>
    {
        EditStarted?.Invoke(item);
        if (_nativeEditStarting) return;
        var column = Columns.FirstOrDefault(candidate => candidate.SortMemberPath == _engine.Interaction.EditingColumnKey)
            ?? Columns.FirstOrDefault(candidate => !candidate.IsReadOnly);
        if (column is not null && Items.Contains(item))
        {
            CurrentCell = new WpfDataGridCellInfo(item, column);
            BeginEdit();
        }
    });

    /// <summary>Validates native binding state before committing the shared draft.</summary>
    public ValueTask<bool> ValidateEditDraftAsync(object item) => ValueTask.FromResult(
        Dispatcher.CheckAccess() ? BindingGroup?.ValidateWithoutUpdate() ?? true
            : Dispatcher.Invoke(() => BindingGroup?.ValidateWithoutUpdate() ?? true));
    /// <summary>Invokes caller-owned persistence for the shared draft.</summary>
    public async ValueTask<bool> CommitEditDraftAsync(object original, object draft)
    {
        var handler = CommitEditHandler;
        if (handler is null) return true;
        return Dispatcher.CheckAccess() ? await handler(draft)
            : await Dispatcher.InvokeAsync(() => handler(draft)).Task.Unwrap();
    }
    /// <summary>Completes native editing and publishes the committed draft.</summary>
    public ValueTask AfterEditCommittedAsync(object item) => UpdateNativeAsync(() =>
    {
        _completingNativeEdit = true;
        try
        {
            CommitEdit(System.Windows.Controls.DataGridEditingUnit.Cell, true);
            CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);
        }
        finally { _completingNativeEdit = false; }
        ItemCommitted?.Invoke(item);
    });
    /// <summary>Cancels native editing and publishes the original row.</summary>
    public ValueTask AfterEditCancelledAsync(object item) => UpdateNativeAsync(() =>
    {
        _completingNativeEdit = true;
        try
        {
            CancelEdit(System.Windows.Controls.DataGridEditingUnit.Cell);
            CancelEdit(System.Windows.Controls.DataGridEditingUnit.Row);
        }
        finally { _completingNativeEdit = false; }
        EditCancelled?.Invoke(item);
    });

    /// <summary>Starts a shared draft when the editor for a registered shared column is activated.</summary>
    protected override async void OnBeginningEdit(WpfDataGridBeginningEditEventArgs e)
    {
        base.OnBeginningEdit(e);
        if (e.Cancel) return;
        var definition = _engine.Columns.FirstOrDefault(column => column.Key == e.Column.SortMemberPath);
        if (definition is null || !definition.Editable || definition.EditTemplate is null) return;
        if (_engine.IsEditing)
        {
            if (!ReferenceEquals(_engine.OriginalEditItem, e.Row.Item)) e.Cancel = true;
            return;
        }
        _nativeEditStarting = true;
        try { await this.EditCellAsync(e.Row.Item, e.Column.SortMemberPath); }
        finally { _nativeEditStarting = false; }
    }

    /// <summary>Routes native row completion and cancellation through the shared edit lifecycle.</summary>
    protected override async void OnRowEditEnding(WpfDataGridRowEditEndingEventArgs e)
    {
        base.OnRowEditEnding(e);
        if (e.Cancel || _completingNativeEdit || !_engine.IsEditing) return;
        e.Cancel = true;
        if (e.EditAction == System.Windows.Controls.DataGridEditAction.Cancel) await this.CancelEditAsync();
        else await this.CommitEditAsync();
    }

    /// <summary>Routes native cell completion through Core when the shared editing policy is cell based.</summary>
    protected override async void OnCellEditEnding(WpfDataGridCellEditEndingEventArgs e)
    {
        base.OnCellEditEnding(e);
        if (e.Cancel || _completingNativeEdit || !_engine.IsEditing || EditMode != VekDataGridEditMode.Cell) return;
        e.Cancel = true;
        if (e.EditAction == System.Windows.Controls.DataGridEditAction.Cancel) await this.CancelEditAsync();
        else await this.CommitEditAsync();
    }

    /// <summary>Marks plain row activation so selection changes can honor the shared activation policy.</summary>
    protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (!RowClickSelection && IsRowSurface(e.OriginalSource as WpfDependencyObject))
        {
            PrepareHostState();
            _suppressRowActivationSelection = true;
            Dispatcher.BeginInvoke(() => _suppressRowActivationSelection = false,
                System.Windows.Threading.DispatcherPriority.Background);
        }
        base.OnPreviewMouseLeftButtonDown(e);
    }

    private static bool IsRowSurface(WpfDependencyObject? source)
    {
        while (source is not null)
        {
            if (source is System.Windows.Controls.Primitives.ButtonBase or System.Windows.Controls.Primitives.TextBoxBase
                or System.Windows.Controls.ComboBox) return false;
            if (source is WpfDataGridRow) return true;
            source = source is System.Windows.Media.Visual or System.Windows.Media.Media3D.Visual3D
                ? System.Windows.Media.VisualTreeHelper.GetParent(source)
                : System.Windows.LogicalTreeHelper.GetParent(source);
        }
        return false;
    }

    /// <summary>Applies shared expansion to a newly realized native row.</summary>
    protected override void OnLoadingRow(WpfDataGridRowEventArgs e)
    {
        base.OnLoadingRow(e);
        e.Row.DetailsVisibility = this.IsRowExpanded(e.Row.Item) ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    /// <summary>Captures external sources while ignoring the control's own result projections.</summary>
    protected override void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
    {
        if (!_projecting) _sourceItems = newValue;
        base.OnItemsSourceChanged(oldValue, newValue);
    }
}
