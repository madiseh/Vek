// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;

namespace Vek;

/// <summary>
/// Implements W3C WAI-ARIA Grid keyboard navigation, roving tabindex, selection toggling,
/// and keyboard-driven edit lifecycle handling for <see cref="VekDataGrid{T}"/>.
/// </summary>
/// <typeparam name="T">The data grid item type.</typeparam>
public class DataGridAriaKeyboardHandler<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
{
    private readonly VekDataGrid<T> _dataGrid;

    public DataGridAriaKeyboardHandler(VekDataGrid<T> dataGrid)
    {
        _dataGrid = dataGrid ?? throw new ArgumentNullException(nameof(dataGrid));
    }

    /// <summary>
    /// Gets the zero-based row index of the currently active cell, or <c>-1</c> if no rows are available.
    /// </summary>
    public int ActiveRowIndex => _dataGrid.GetEffectiveFocusedRowIndex();

    /// <summary>
    /// Gets the zero-based visible column index of the currently active cell, or <c>-1</c> if no columns are visible.
    /// </summary>
    public int ActiveColumnIndex => _dataGrid.GetEffectiveFocusedColumnIndex();

    /// <summary>
    /// Computes the roving <c>tabindex</c> attribute value for the specified table cell.
    /// </summary>
    /// <param name="rowIndex">The zero-based row index of the cell.</param>
    /// <param name="columnIndex">The zero-based visible column index of the cell.</param>
    /// <returns><c>0</c> for the active cell, <c>-1</c> for inactive cells, or <c>null</c> when keyboard navigation is disabled.</returns>
    public int? GetCellTabIndex(int rowIndex, int columnIndex)
    {
        if (!_dataGrid.IsKeyboardNavigationEnabled)
        {
            return null;
        }

        var activeRow = _dataGrid.GetEffectiveFocusedRowIndex();
        var activeColumn = _dataGrid.GetEffectiveFocusedColumnIndex();
        if (activeRow < 0 || activeColumn < 0)
        {
            return null;
        }

        return rowIndex == activeRow && columnIndex == activeColumn ? 0 : -1;
    }

    /// <summary>
    /// Handles WAI-ARIA Grid keyboard interactions including cell navigation, editing lifecycle, and selection toggling.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    /// <returns><c>true</c> if the key event was handled; otherwise, <c>false</c>.</returns>
    public async Task<bool> HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (args?.Key is null || !_dataGrid.IsKeyboardNavigationEnabled)
        {
            return false;
        }

        if (args.Key == "Tab")
        {
            // Allow Tab to exit the grid naturally toward adjacent interactive elements.
            return false;
        }

        var visibleItems = _dataGrid.GetVisibleRowItems();
        var rowCount = visibleItems.Count;
        var columnCount = _dataGrid.GetVisibleColumnCount();
        if (rowCount == 0 || columnCount == 0)
        {
            return false;
        }

        switch (args.Key)
        {
            case "ArrowRight":
                return await MoveRightAsync();

            case "ArrowLeft":
                return await MoveLeftAsync();

            case "ArrowDown":
                return await MoveDownAsync();

            case "ArrowUp":
                return await MoveUpAsync();

            case "Home":
                return args.CtrlKey ? await MoveFirstCellAsync() : await MoveRowStartAsync();

            case "End":
                return args.CtrlKey ? await MoveLastCellAsync() : await MoveRowEndAsync();

            case "PageDown":
                return await MovePageDownAsync();

            case "PageUp":
                return await MovePageUpAsync();

            case "F2":
                return await ActivateEditAsync();

            case "Enter":
                return await ActivateOrCommitEditAsync();

            case "Escape":
                return await CancelEditAsync();

            case " ":
            case "Spacebar":
                return await ToggleRowSelectionAsync();

            default:
                return false;
        }
    }

    /// <summary>
    /// Moves cell focus one visible column to the right, clamped to the last column.
    /// </summary>
    /// <returns><c>true</c> if the active cell changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MoveRightAsync()
    {
        return MoveByOffsetAsync(0, 1);
    }

    /// <summary>
    /// Moves cell focus one visible column to the left, clamped to the first column.
    /// </summary>
    /// <returns><c>true</c> if the active cell changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MoveLeftAsync()
    {
        return MoveByOffsetAsync(0, -1);
    }

    /// <summary>
    /// Moves cell focus one row down in the same column, clamped to the last visible row.
    /// </summary>
    /// <returns><c>true</c> if the active cell changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MoveDownAsync()
    {
        return MoveByOffsetAsync(1, 0);
    }

    /// <summary>
    /// Moves cell focus one row up in the same column, clamped to the first visible row.
    /// </summary>
    /// <returns><c>true</c> if the active cell changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MoveUpAsync()
    {
        return MoveByOffsetAsync(-1, 0);
    }

    /// <summary>
    /// Moves cell focus to the first visible column in the active row.
    /// </summary>
    /// <returns><c>true</c> if the active cell changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MoveRowStartAsync()
    {
        return MoveToCellAsync(_dataGrid.GetEffectiveFocusedRowIndex(), 0);
    }

    /// <summary>
    /// Moves cell focus to the last visible column in the active row.
    /// </summary>
    /// <returns><c>true</c> if the active cell changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MoveRowEndAsync()
    {
        var columnCount = _dataGrid.GetVisibleColumnCount();
        return MoveToCellAsync(_dataGrid.GetEffectiveFocusedRowIndex(), columnCount - 1);
    }

    /// <summary>
    /// Moves cell focus to the first cell (<c>(0, 0)</c>) in the grid.
    /// </summary>
    /// <returns><c>true</c> if the active cell changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MoveFirstCellAsync()
    {
        return MoveToCellAsync(0, 0);
    }

    /// <summary>
    /// Moves cell focus to the last cell in the grid.
    /// </summary>
    /// <returns><c>true</c> if the active cell changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MoveLastCellAsync()
    {
        var rowCount = _dataGrid.GetVisibleRowItems().Count;
        var columnCount = _dataGrid.GetVisibleColumnCount();
        return MoveToCellAsync(rowCount - 1, columnCount - 1);
    }

    /// <summary>
    /// Moves cell focus down by the specified page size or the grid's rows per page.
    /// </summary>
    /// <param name="pageSize">The row count to jump. When <c>null</c>, uses <see cref="VekDataGrid{T}.RowsPerPage"/> or <c>10</c>.</param>
    /// <returns><c>true</c> if the active cell changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MovePageDownAsync(int? pageSize = null)
    {
        var step = GetPageStep(pageSize);
        return MoveByOffsetAsync(step, 0);
    }

    /// <summary>
    /// Moves cell focus up by the specified page size or the grid's rows per page.
    /// </summary>
    /// <param name="pageSize">The row count to jump. When <c>null</c>, uses <see cref="VekDataGrid{T}.RowsPerPage"/> or <c>10</c>.</param>
    /// <returns><c>true</c> if the active cell changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MovePageUpAsync(int? pageSize = null)
    {
        var step = GetPageStep(pageSize);
        return MoveByOffsetAsync(-step, 0);
    }

    /// <summary>
    /// Activates editing on the currently focused row when the grid is not read-only.
    /// </summary>
    /// <returns><c>true</c> if editing was started; otherwise, <c>false</c>.</returns>
    public async Task<bool> ActivateEditAsync()
    {
        if (!_dataGrid.IsKeyboardNavigationEnabled || _dataGrid.ReadOnly)
        {
            return false;
        }

        var visibleItems = _dataGrid.GetVisibleRowItems();
        var activeRow = _dataGrid.GetEffectiveFocusedRowIndex();
        if (activeRow < 0 || activeRow >= visibleItems.Count)
        {
            return false;
        }

        var item = visibleItems[activeRow];
        if (_dataGrid.EditMode == DataGridEditMode.Inline && _dataGrid.IsEditingItem(item))
        {
            return false;
        }

        if (_dataGrid.EditMode == DataGridEditMode.Form && _dataGrid._isEditFormOpen)
        {
            return false;
        }

        if (_dataGrid.EditMode == DataGridEditMode.Cell)
        {
            await _dataGrid.BeginCellEditAsync(item);
        }
        else
        {
            await _dataGrid.SetEditingItemAsync(item);
        }

        await _dataGrid.NotifyStateChangedAsync();
        return true;
    }

    /// <summary>
    /// Commits an active inline edit on the focused row, or activates editing when not already editing.
    /// </summary>
    /// <returns><c>true</c> if an edit operation was committed or started; otherwise, <c>false</c>.</returns>
    public async Task<bool> ActivateOrCommitEditAsync()
    {
        if (!_dataGrid.IsKeyboardNavigationEnabled || _dataGrid.ReadOnly)
        {
            return false;
        }

        var visibleItems = _dataGrid.GetVisibleRowItems();
        var activeRow = _dataGrid.GetEffectiveFocusedRowIndex();
        if (activeRow < 0 || activeRow >= visibleItems.Count)
        {
            return false;
        }

        var item = visibleItems[activeRow];
        if (_dataGrid.EditMode == DataGridEditMode.Inline && _dataGrid.IsEditingItem(item))
        {
            await _dataGrid.CommitInlineEditAsync();
            await _dataGrid.NotifyStateChangedAsync();
            return true;
        }

        return await ActivateEditAsync();
    }

    /// <summary>
    /// Cancels the active edit session when an item is currently being edited.
    /// </summary>
    /// <returns><c>true</c> if an active edit was canceled; otherwise, <c>false</c>.</returns>
    public async Task<bool> CancelEditAsync()
    {
        if (!_dataGrid.IsKeyboardNavigationEnabled || !_dataGrid.HasActiveEdit)
        {
            return false;
        }

        await _dataGrid.CancelEditingItemAsync();
        await _dataGrid.NotifyStateChangedAsync();
        return true;
    }

    /// <summary>
    /// Toggles selection for the row containing the active cell when selection is enabled and the row is not inline-editing.
    /// </summary>
    /// <returns><c>true</c> if selection was toggled; otherwise, <c>false</c>.</returns>
    public async Task<bool> ToggleRowSelectionAsync()
    {
        if (!_dataGrid.IsKeyboardNavigationEnabled || !_dataGrid.SelectionChangeable)
        {
            return false;
        }

        var visibleItems = _dataGrid.GetVisibleRowItems();
        var activeRow = _dataGrid.GetEffectiveFocusedRowIndex();
        if (activeRow < 0 || activeRow >= visibleItems.Count)
        {
            return false;
        }

        var item = visibleItems[activeRow];
        if (_dataGrid.EditMode == DataGridEditMode.Inline && _dataGrid.IsEditingItem(item))
        {
            return false;
        }

        if (_dataGrid.IsRowSelectionDisabled(item))
        {
            return false;
        }

        var isSelected = _dataGrid.Selection.Contains(item);
        await _dataGrid.SetSelectedItemAsync(!isSelected, item);
        return true;
    }

    private int GetPageStep(int? pageSize)
    {
        var resolved = pageSize ?? (_dataGrid.RowsPerPage > 0 ? _dataGrid.RowsPerPage : 10);
        return resolved > 0 ? resolved : 10;
    }

    private Task<bool> MoveByOffsetAsync(int rowDelta, int columnDelta)
    {
        if (!_dataGrid.IsKeyboardNavigationEnabled)
        {
            return Task.FromResult(false);
        }

        var rowCount = _dataGrid.GetVisibleRowItems().Count;
        var columnCount = _dataGrid.GetVisibleColumnCount();
        if (rowCount == 0 || columnCount == 0)
        {
            return Task.FromResult(false);
        }

        var currentRow = _dataGrid.GetEffectiveFocusedRowIndex();
        var currentColumn = _dataGrid.GetEffectiveFocusedColumnIndex();
        var targetRow = Math.Clamp(currentRow + rowDelta, 0, rowCount - 1);
        var targetColumn = Math.Clamp(currentColumn + columnDelta, 0, columnCount - 1);

        if (targetRow == currentRow && targetColumn == currentColumn)
        {
            return Task.FromResult(false);
        }

        return SetFocusedCellInternalAsync(targetRow, targetColumn);
    }

    private Task<bool> MoveToCellAsync(int targetRow, int targetColumn)
    {
        if (!_dataGrid.IsKeyboardNavigationEnabled)
        {
            return Task.FromResult(false);
        }

        var rowCount = _dataGrid.GetVisibleRowItems().Count;
        var columnCount = _dataGrid.GetVisibleColumnCount();
        if (rowCount == 0 || columnCount == 0)
        {
            return Task.FromResult(false);
        }

        var clampedRow = Math.Clamp(targetRow, 0, rowCount - 1);
        var clampedColumn = Math.Clamp(targetColumn, 0, columnCount - 1);
        var currentRow = _dataGrid.GetEffectiveFocusedRowIndex();
        var currentColumn = _dataGrid.GetEffectiveFocusedColumnIndex();

        if (clampedRow == currentRow && clampedColumn == currentColumn)
        {
            return Task.FromResult(false);
        }

        return SetFocusedCellInternalAsync(clampedRow, clampedColumn);
    }

    private async Task<bool> SetFocusedCellInternalAsync(int rowIndex, int columnIndex)
    {
        await _dataGrid.SetFocusedCellAsync(rowIndex, columnIndex);
        return true;
    }
}
