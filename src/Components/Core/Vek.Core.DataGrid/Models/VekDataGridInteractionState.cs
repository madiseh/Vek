// Copyright (c) 2026 AMIN SABAGHI
// Licensed under the MIT License. See the LICENSE file in the project root for more information.
// Created: 2026-11-01

namespace Vek.Core.DataGrid;

/// <summary>Owns neutral interaction policies and row state, independent of presentation and input devices.</summary>
/// <typeparam name="TItem">The row item type.</typeparam>
public sealed class VekDataGridInteractionState<TItem>
{
    private IReadOnlyList<TItem> _filteredItems = [];
    private IReadOnlyList<TItem> _visibleItems = [];
    private int? _totalItems;
    /// <summary>Gets or sets the current source, including rows outside the visible page.</summary>
    public IEnumerable<TItem> Items { get; set; } = [];
    /// <summary>Gets or sets the current filtered and ordered result scope.</summary>
    public IReadOnlyList<TItem> FilteredItems { get => FilteredItemsProvider?.Invoke() ?? _filteredItems; set => _filteredItems = value; }
    /// <summary>Gets or sets the currently available page or window.</summary>
    public IReadOnlyList<TItem> VisibleItems { get => VisibleItemsProvider?.Invoke() ?? _visibleItems; set => _visibleItems = value; }
    /// <summary>Gets or sets a known total count when only part of a result is available.</summary>
    public int? TotalItems { get => TotalItemsProvider?.Invoke() ?? _totalItems; set => _totalItems = value; }
    /// <summary>Gets or sets lazy access to the current filtered result scope.</summary>
    public Func<IReadOnlyList<TItem>>? FilteredItemsProvider { get; set; }
    /// <summary>Gets or sets lazy access to the current available page or window.</summary>
    public Func<IReadOnlyList<TItem>>? VisibleItemsProvider { get; set; }
    /// <summary>Gets or sets lazy access to a known total result count.</summary>
    public Func<int>? TotalItemsProvider { get; set; }
    /// <summary>Gets or sets the selection policy.</summary>
    public VekDataGridSelectionMode SelectionMode { get; set; }
    /// <summary>Gets or sets whether row activation changes selection.</summary>
    public bool RowClickSelection { get; set; }
    /// <summary>Gets or sets the eligibility predicate.</summary>
    public Func<TItem, bool>? IsRowSelectable { get; set; }
    /// <summary>Gets or sets row equality for selection reconciliation.</summary>
    public IEqualityComparer<TItem> SelectionComparer { get; set; } = EqualityComparer<TItem>.Default;
    /// <summary>Gets or sets the selected row projection synchronized by the host.</summary>
    public IReadOnlyList<TItem> SelectedItems { get; set; } = [];
    /// <summary>Gets or sets the primary selected row.</summary>
    public TItem? SelectedItem { get; set; }
    /// <summary>Gets or sets whether a filtered-out selection is retained.</summary>
    public VekDataGridFilteredSelectionMode FilteredSelectionMode { get; set; }
    /// <summary>Gets or sets an additional shared filter predicate.</summary>
    public Func<TItem, bool>? QuickFilter { get; set; }
    /// <summary>Gets or sets whether only one row may have expanded details.</summary>
    public bool SingleExpandedRow { get; set; }
    /// <summary>Gets or sets row equality for detail expansion.</summary>
    public IEqualityComparer<TItem> DetailsComparer { get; set; } = EqualityComparer<TItem>.Default;
    /// <summary>Gets the authoritative expanded-row list.</summary>
    public IReadOnlyList<TItem> ExpandedItems { get; private set; } = [];
    /// <summary>Gets or sets the editing policy.</summary>
    public VekDataGridEditMode EditMode { get; set; }
    /// <summary>Gets or sets a caller-supplied draft clone operation.</summary>
    public Func<TItem, TItem>? CloneItem { get; set; }
    /// <summary>Gets the active edit column key, or null for a whole-row edit.</summary>
    public string? EditingColumnKey { get; internal set; }
    /// <summary>Gets or sets whether changing column widths is enabled.</summary>
    public bool ColumnResizingEnabled { get; set; } = true;
    /// <summary>Gets or sets an additional minimum width constraint supplied by the caller.</summary>
    public int MinimumColumnWidth { get; set; }

    /// <summary>Replaces expanded rows while honoring equality and single-row policy.</summary>
    public void SetExpandedItems(IEnumerable<TItem> rows) => ExpandedItems =
        (SingleExpandedRow ? rows.Take(1) : rows).Distinct(DetailsComparer).ToArray();

    /// <summary>Replaces expanded rows with equivalent rows from a refreshed source, dropping removed rows.</summary>
    public void ReconcileExpandedItems(IEnumerable<TItem> rows)
    {
        var available = rows.ToArray();
        var reconciled = new List<TItem>();
        foreach (var expanded in ExpandedItems)
        {
            var index = Array.FindIndex(available, row => DetailsComparer.Equals(row, expanded));
            if (index >= 0) reconciled.Add(available[index]);
        }
        SetExpandedItems(reconciled);
    }

    /// <summary>Reconciles selected rows against refreshed data and filtering, optionally dropping removed rows.</summary>
    public bool ReconcileSelection(IEnumerable<TItem> rows, Func<TItem, bool> matches, bool removeMissing)
    {
        if (SelectedItems.Count == 0 && SelectedItem is null) return false;
        var current = rows.ToArray();
        TItem? Resolve(TItem selected) => current.FirstOrDefault(row => SelectionComparer.Equals(row, selected));
        var reconciled = SelectedItems.Select(selected => Resolve(selected) is { } found ? found : removeMissing ? default : selected)
            .Where(row => row is not null).Cast<TItem>().Distinct(SelectionComparer).ToList();
        var selectedItem = SelectedItem is null ? reconciled.FirstOrDefault()
            : Resolve(SelectedItem) is { } found ? found : removeMissing ? default : SelectedItem;
        if (FilteredSelectionMode == VekDataGridFilteredSelectionMode.Clear)
        {
            reconciled.RemoveAll(item => !matches(item));
            if (selectedItem is not null && !matches(selectedItem)) selectedItem = default;
        }
        var changed = !reconciled.SequenceEqual(SelectedItems, SelectionComparer)
            || (SelectedItem is not null && !ReferenceEquals(SelectedItem, selectedItem));
        SelectedItems = reconciled;
        SelectedItem = selectedItem;
        return changed;
    }
}
