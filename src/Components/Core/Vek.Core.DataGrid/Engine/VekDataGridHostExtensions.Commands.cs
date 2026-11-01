// Copyright (c) 2026 AMIN SABAGHI
// Licensed under the MIT License. See the LICENSE file in the project root for more information.
// Created: 2026-11-01

namespace Vek.Core.DataGrid;

public static partial class VekDataGridHostExtensions
{
    /// <summary>Reconciles selection with refreshed data and filtering, notifying the host only when it changes.</summary>
    public static async Task ReconcileSelectionAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, bool removeMissing = false)
    {
        var engine = grid.Engine;
        var state = engine.Interaction;
        if (state.ReconcileSelection(state.Items, item => (state.QuickFilter?.Invoke(item) ?? true) && engine.MatchesFilters(item), removeMissing))
        {
            SynchronizeSelection(engine);
            await grid.AfterSelectionChangeAsync();
        }
    }

    /// <summary>Requests a page clamped to the current known result count.</summary>
    public static async Task SetCurrentPageAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, int page)
    {
        var engine = grid.Engine;
        engine.Page = Math.Clamp(page, 1, engine.GetPageCount(engine.Interaction.TotalItems ?? engine.Interaction.FilteredItems.Count));
        await grid.AfterPageChangeAsync(false);
    }

    /// <summary>Changes page size and restarts at the first page.</summary>
    public static async Task SetPageSizeAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, int pageSize)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        grid.Engine.SetPage(1, pageSize);
        await grid.AfterPageChangeAsync(true);
    }

    /// <summary>Changes visibility through the canonical column contract.</summary>
    public static async Task SetColumnVisibilityAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, string columnKey, bool visible)
    {
        var column = FindColumn(grid.Engine, columnKey);
        if (column.Visible == visible) return;
        await column.SetVisibleAsync(visible);
        await grid.AfterColumnChangeAsync(false);
    }

    /// <summary>Moves a registered column to a clamped insertion position.</summary>
    public static async Task ReorderColumnAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, string columnKey, int insertIndex)
    {
        if (grid.Engine.ReorderColumn(columnKey, insertIndex)) await grid.AfterColumnChangeAsync(true);
    }

    /// <summary>Changes column width within configured constraints when resizing is enabled.</summary>
    public static async Task ResizeColumnAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, string columnKey, int width)
    {
        var engine = grid.Engine;
        if (!engine.Interaction.ColumnResizingEnabled) throw new InvalidOperationException("Column resizing is disabled.");
        var column = FindColumn(engine, columnKey);
        var minimum = Math.Max(column.MinWidth, engine.Interaction.MinimumColumnWidth);
        var maximum = Math.Max(column.MaxWidth, minimum);
        await column.SetWidthAsync(Math.Clamp(width, minimum, maximum));
        await grid.AfterColumnChangeAsync(false);
    }

    /// <summary>Checks selection eligibility using the shared selection policy.</summary>
    public static bool CanSelectItem<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, TItem item) => CanSelect(grid.Engine.Interaction, item);

    /// <summary>Checks selected-key membership for a row.</summary>
    public static bool IsItemSelected<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, TItem item)
    {
        var engine = grid.Engine;
        SynchronizeSelection(engine);
        return engine.IsSelected(item);
    }

    /// <summary>Selects or deselects an eligible row while preserving selections outside the visible page.</summary>
    public static async Task ToggleSelectionAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, TItem item, bool selected)
    {
        var engine = grid.Engine;
        var state = engine.Interaction;
        if (!CanSelect(state, item)) return;
        SynchronizeSelection(engine);
        if (engine.IsSelected(item) != selected) engine.ToggleSelection(item, state.SelectionMode);
        ProjectSelection(engine, state.SelectedItems.Concat(state.Items).Append(item));
        await grid.AfterSelectionChangeAsync();
    }

    /// <summary>Toggles an eligible row, keeping the active row selected in single-selection mode.</summary>
    public static Task SelectItemAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, TItem item)
    {
        var engine = grid.Engine;
        SynchronizeSelection(engine);
        return grid.ToggleSelectionAsync(item, engine.Interaction.SelectionMode == VekDataGridSelectionMode.Single || !engine.IsSelected(item));
    }

    /// <summary>Handles row activation when row-activation selection is enabled.</summary>
    public static Task SelectRowAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, TItem item) =>
        grid.Engine.Interaction.RowClickSelection ? grid.SelectItemAsync(item) : Task.CompletedTask;

    /// <summary>Checks whether every eligible row in the visible scope is selected.</summary>
    public static bool AreAllVisibleItemsSelected<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid)
    {
        var engine = grid.Engine;
        SynchronizeSelection(engine);
        var items = engine.Interaction.VisibleItems.Where(item => CanSelect(engine.Interaction, item)).ToArray();
        return items.Length > 0 && items.All(engine.IsSelected);
    }

    /// <summary>Selects or clears eligible rows in the visible scope.</summary>
    public static async Task ToggleAllVisibleItemsAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, bool selected)
    {
        var engine = grid.Engine;
        var state = engine.Interaction;
        SynchronizeSelection(engine);
        var rows = state.VisibleItems.Where(item => CanSelect(state, item)).ToArray();
        foreach (var row in rows)
            if (engine.IsSelected(row) != selected) engine.ToggleSelection(row, state.SelectionMode);
        ProjectSelection(engine, state.SelectedItems.Concat(rows));
        await grid.AfterSelectionChangeAsync();
    }

    /// <summary>Selects or clears the current visible scope.</summary>
    public static Task SelectAllAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, bool selected) => grid.ToggleAllVisibleItemsAsync(selected);

    /// <summary>Checks shared row-detail expansion state.</summary>
    public static bool IsRowExpanded<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, TItem item)
    {
        var state = grid.Engine.Interaction;
        return state.ExpandedItems.Any(row => state.DetailsComparer.Equals(row, item));
    }

    /// <summary>Expands the current result scope, honoring the single-expanded-row policy.</summary>
    public static async Task ExpandAllAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid)
    {
        var state = grid.Engine.Interaction;
        state.SetExpandedItems(state.FilteredItems);
        await grid.AfterRowDetailsChangeAsync();
    }

    /// <summary>Collapses all expanded row details.</summary>
    public static async Task CollapseAllAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid)
    {
        grid.Engine.Interaction.SetExpandedItems([]);
        await grid.AfterRowDetailsChangeAsync();
    }

    /// <summary>Toggles row details while honoring equality and single-row expansion.</summary>
    public static async Task ToggleRowDetailsAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, TItem item)
    {
        var state = grid.Engine.Interaction;
        var rows = state.ExpandedItems.ToList();
        var index = rows.FindIndex(row => state.DetailsComparer.Equals(row, item));
        if (index >= 0) rows.RemoveAt(index);
        else
        {
            if (state.SingleExpandedRow) rows.Clear();
            rows.Add(item);
        }
        state.SetExpandedItems(rows);
        await grid.AfterRowDetailsChangeAsync();
    }

    /// <summary>Checks whether all rows in the current result scope have expanded details.</summary>
    public static bool AreAllRowsExpanded<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid)
    {
        var state = grid.Engine.Interaction;
        return state.FilteredItems.Count > 0 && state.FilteredItems.All(item => state.ExpandedItems.Any(row => state.DetailsComparer.Equals(row, item)));
    }

    /// <summary>Toggles expansion of the complete current result scope.</summary>
    public static Task ToggleAllRowDetailsAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid) =>
        grid.AreAllRowsExpanded() ? grid.CollapseAllAsync() : grid.ExpandAllAsync();

    /// <summary>Toggles one shared group-expansion key.</summary>
    public static async Task ToggleGroupAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, string key)
    {
        var engine = grid.Engine;
        engine.SetGroupExpanded(key, !engine.IsGroupExpanded(key));
        await grid.AfterGroupChangeAsync();
    }

    /// <summary>Checks whether the shared editing policy permits row editing.</summary>
    public static bool CanEditRows<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid)
    {
        var engine = grid.Engine;
        return engine.Interaction.EditMode != VekDataGridEditMode.None && engine.Columns.Any(column => column.Visible && column.Editable && column.EditTemplate is not null);
    }

    /// <summary>Starts an isolated row draft under the shared editing policy.</summary>
    public static Task EditItemAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, TItem item) => StartEditAsync(grid, item, null);

    /// <summary>Starts an isolated draft for a registered editable column.</summary>
    public static Task EditCellAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, TItem item, string columnKey)
    {
        var column = FindColumn(grid.Engine, columnKey);
        if (!column.Editable || column.EditTemplate is null) throw new InvalidOperationException($"Column '{columnKey}' does not have an editable template.");
        return StartEditAsync(grid, item, columnKey);
    }

    /// <summary>Replaces the active draft and notifies caller-owned validation state.</summary>
    public static void UpdateEditDraft<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid, TItem item)
    {
        var engine = grid.Engine;
        if (!engine.IsEditing) return;
        engine.UpdateDraft(item);
        grid.AfterEditDraftChanged();
    }

    /// <summary>Validates and commits the active draft, retaining rejected sessions.</summary>
    public static async Task<bool> CommitEditAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid)
    {
        var engine = grid.Engine;
        var draft = engine.DraftEditItem;
        if (!await engine.CommitEditAsync(new HostValidator<TItem, TTemplate>(grid), new HostCommitHandler<TItem, TTemplate>(grid))) return false;
        engine.Interaction.EditingColumnKey = null;
        await grid.AfterEditCommittedAsync(draft!);
        return true;
    }

    /// <summary>Cancels the active edit and notifies the host with the original row.</summary>
    public static async Task CancelEditAsync<TItem, TTemplate>(this IVekDataGridHost<TItem, TTemplate> grid)
    {
        var engine = grid.Engine;
        if (!engine.IsEditing) return;
        var original = engine.OriginalEditItem;
        engine.CancelEdit();
        engine.Interaction.EditingColumnKey = null;
        await grid.AfterEditCancelledAsync(original!);
    }

    private static async Task StartEditAsync<TItem, TTemplate>(IVekDataGridHost<TItem, TTemplate> grid, TItem item, string? key)
    {
        var engine = grid.Engine;
        if (engine.IsEditing || engine.Interaction.EditMode == VekDataGridEditMode.None
            || !engine.Columns.Any(column => column.Visible && column.Editable && column.EditTemplate is not null)
            || (engine.Interaction.EditMode == VekDataGridEditMode.Cell && key is null)) return;
        var clone = engine.Interaction.CloneItem;
        var draft = clone is not null ? clone(item) : CloneItem(item);
        engine.BeginEdit(item, draft);
        engine.Interaction.EditingColumnKey = key;
        await grid.AfterEditStartedAsync(item);
    }

    private static TItem CloneItem<TItem>(TItem item)
    {
        if (item is null || typeof(TItem).IsValueType) return item;
        var method = typeof(object).GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        return (TItem)method.Invoke(item, null)!;
    }

    private static IVekDataGridColumn<TItem, TTemplate> FindColumn<TItem, TTemplate>(VekDataGridEngine<TItem, TTemplate> engine, string key) =>
        engine.Columns.FirstOrDefault(column => string.Equals(column.Key, key, StringComparison.OrdinalIgnoreCase))
        ?? throw new ArgumentException($"No column with key '{key}' is registered.", nameof(key));

    private static bool CanSelect<TItem>(VekDataGridInteractionState<TItem> state, TItem item) =>
        state.SelectionMode != VekDataGridSelectionMode.None && (state.IsRowSelectable?.Invoke(item) ?? true);

    private static void SynchronizeSelection<TItem, TTemplate>(VekDataGridEngine<TItem, TTemplate> engine) =>
        engine.SetSelectedKeys(engine.Interaction.SelectedItems.Select(engine.GetItemKey));

    private static void ProjectSelection<TItem, TTemplate>(VekDataGridEngine<TItem, TTemplate> engine, IEnumerable<TItem> candidates)
    {
        var state = engine.Interaction;
        state.SelectedItems = candidates.Distinct(state.SelectionComparer).Where(engine.IsSelected).ToArray();
        state.SelectedItem = state.SelectedItems.FirstOrDefault();
    }

    private sealed class HostValidator<TItem, TTemplate>(IVekDataGridHost<TItem, TTemplate> host) : IVekDataGridValidator<TItem>
    {
        public async ValueTask<VekDataGridValidationResult> ValidateAsync(TItem item, CancellationToken cancellationToken = default) =>
            await host.ValidateEditDraftAsync(item) ? VekDataGridValidationResult.Success : VekDataGridValidationResult.Failed("Edit validation failed.");
    }

    private sealed class HostCommitHandler<TItem, TTemplate>(IVekDataGridHost<TItem, TTemplate> host) : IVekDataGridCommitHandler<TItem>
    {
        public ValueTask<bool> CommitAsync(TItem originalItem, TItem draftItem, CancellationToken cancellationToken = default) =>
            host.CommitEditDraftAsync(originalItem, draftItem);
    }
}
