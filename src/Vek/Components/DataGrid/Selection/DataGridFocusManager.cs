// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;

namespace Vek;

/// <summary>
/// Manages focused row state, keyboard navigation traversal, and focus transitions for <see cref="VekDataGrid{T}"/>.
/// </summary>
/// <typeparam name="T">The data grid item type.</typeparam>
public class DataGridFocusManager<T>
{
    private readonly VekDataGrid<T> _dataGrid;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridFocusManager{T}"/> class.
    /// </summary>
    /// <param name="dataGrid">The parent data grid instance.</param>
    public DataGridFocusManager(VekDataGrid<T> dataGrid)
    {
        _dataGrid = dataGrid ?? throw new ArgumentNullException(nameof(dataGrid));
    }

    /// <summary>
    /// Handles keyboard navigation events when focused row mode is enabled.
    /// Supports ArrowDown, ArrowUp, PageDown, PageUp, Home, and End keys.
    /// </summary>
    /// <param name="args">The keyboard event arguments.</param>
    /// <returns><c>true</c> if the key event was handled; otherwise, <c>false</c>.</returns>
    public async Task<bool> HandleKeyDownAsync(KeyboardEventArgs args)
    {
        if (args?.Key is null || !_dataGrid.HasFocusedRow)
        {
            return false;
        }

        var visibleItems = _dataGrid.GetVisibleRowItems();
        var count = visibleItems.Count;
        if (count == 0)
        {
            return false;
        }

        var pageSize = _dataGrid.RowsPerPage > 0 ? _dataGrid.RowsPerPage : 10;

        switch (args.Key)
        {
            case "ArrowDown":
                return await MoveNextAsync();

            case "ArrowUp":
                return await MovePreviousAsync();

            case "PageDown":
                return await MovePageDownAsync(pageSize);

            case "PageUp":
                return await MovePageUpAsync(pageSize);

            case "Home":
                return await MoveFirstAsync();

            case "End":
                return await MoveLastAsync();

            default:
                return false;
        }
    }

    /// <summary>
    /// Moves the focus to the next row in the visible items sequence.
    /// </summary>
    /// <returns><c>true</c> if focus changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MoveNextAsync()
    {
        var count = _dataGrid.GetVisibleRowItems().Count;
        if (count == 0)
        {
            return Task.FromResult(false);
        }

        var currentIndex = _dataGrid.CurrentFocusedIndex;
        var newIndex = currentIndex < 0 ? 0 : Math.Min(currentIndex + 1, count - 1);
        if (newIndex == currentIndex)
        {
            return Task.FromResult(false);
        }

        return SetFocusIndexInternalAsync(newIndex);
    }

    /// <summary>
    /// Moves the focus to the previous row in the visible items sequence.
    /// </summary>
    /// <returns><c>true</c> if focus changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MovePreviousAsync()
    {
        var count = _dataGrid.GetVisibleRowItems().Count;
        if (count == 0)
        {
            return Task.FromResult(false);
        }

        var currentIndex = _dataGrid.CurrentFocusedIndex;
        var newIndex = currentIndex < 0 ? 0 : Math.Max(currentIndex - 1, 0);
        if (newIndex == currentIndex)
        {
            return Task.FromResult(false);
        }

        return SetFocusIndexInternalAsync(newIndex);
    }

    /// <summary>
    /// Moves the focus to the first row in the visible items sequence.
    /// </summary>
    /// <returns><c>true</c> if focus changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MoveFirstAsync()
    {
        var count = _dataGrid.GetVisibleRowItems().Count;
        if (count == 0)
        {
            return Task.FromResult(false);
        }

        var currentIndex = _dataGrid.CurrentFocusedIndex;
        if (currentIndex == 0)
        {
            return Task.FromResult(false);
        }

        return SetFocusIndexInternalAsync(0);
    }

    /// <summary>
    /// Moves the focus to the last row in the visible items sequence.
    /// </summary>
    /// <returns><c>true</c> if focus changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MoveLastAsync()
    {
        var count = _dataGrid.GetVisibleRowItems().Count;
        if (count == 0)
        {
            return Task.FromResult(false);
        }

        var targetIndex = count - 1;
        var currentIndex = _dataGrid.CurrentFocusedIndex;
        if (currentIndex == targetIndex)
        {
            return Task.FromResult(false);
        }

        return SetFocusIndexInternalAsync(targetIndex);
    }

    /// <summary>
    /// Moves the focus down by the specified page size or default page jump.
    /// </summary>
    /// <param name="pageSize">The page size or row count to jump. If <c>null</c>, uses grid rows per page or 10.</param>
    /// <returns><c>true</c> if focus changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MovePageDownAsync(int? pageSize = null)
    {
        var count = _dataGrid.GetVisibleRowItems().Count;
        if (count == 0)
        {
            return Task.FromResult(false);
        }

        var size = pageSize.GetValueOrDefault(_dataGrid.RowsPerPage > 0 ? _dataGrid.RowsPerPage : 10);
        var currentIndex = _dataGrid.CurrentFocusedIndex;
        var newIndex = currentIndex < 0 ? 0 : Math.Min(currentIndex + size, count - 1);
        if (newIndex == currentIndex)
        {
            return Task.FromResult(false);
        }

        return SetFocusIndexInternalAsync(newIndex);
    }

    /// <summary>
    /// Moves the focus up by the specified page size or default page jump.
    /// </summary>
    /// <param name="pageSize">The page size or row count to jump. If <c>null</c>, uses grid rows per page or 10.</param>
    /// <returns><c>true</c> if focus changed; otherwise, <c>false</c>.</returns>
    public Task<bool> MovePageUpAsync(int? pageSize = null)
    {
        var count = _dataGrid.GetVisibleRowItems().Count;
        if (count == 0)
        {
            return Task.FromResult(false);
        }

        var size = pageSize.GetValueOrDefault(_dataGrid.RowsPerPage > 0 ? _dataGrid.RowsPerPage : 10);
        var currentIndex = _dataGrid.CurrentFocusedIndex;
        var newIndex = currentIndex < 0 ? 0 : Math.Max(currentIndex - size, 0);
        if (newIndex == currentIndex)
        {
            return Task.FromResult(false);
        }

        return SetFocusIndexInternalAsync(newIndex);
    }

    /// <summary>
    /// Programmatically sets focus to the row at the specified index.
    /// </summary>
    /// <param name="rowIndex">The zero-based index of the row to focus.</param>
    /// <param name="item">The data item corresponding to the row, or <c>default</c> to resolve from index.</param>
    /// <param name="autoScroll">Whether to scroll the focused row into view.</param>
    public Task SetFocusAsync(int rowIndex, T? item = default, bool autoScroll = true)
    {
        return _dataGrid.SetFocusedRowAsync(rowIndex, item, autoScroll);
    }

    /// <summary>
    /// Clears any focused row state, resetting <see cref="VekDataGrid{T}.FocusedRowIndex"/> to <c>-1</c>.
    /// </summary>
    public Task ClearFocusAsync()
    {
        return _dataGrid.SetFocusedRowIndexAsync(-1, false);
    }

    /// <summary>
    /// Determines whether the specified row item or index is currently focused.
    /// </summary>
    /// <param name="item">The data item to evaluate.</param>
    /// <param name="rowIndex">The zero-based row index to evaluate.</param>
    /// <returns><c>true</c> if the row is focused; otherwise, <c>false</c>.</returns>
    public bool IsFocused(T? item, int rowIndex)
    {
        return _dataGrid.IsRowFocused(item, rowIndex);
    }

    private async Task<bool> SetFocusIndexInternalAsync(int index)
    {
        await _dataGrid.SetFocusedRowIndexAsync(index);
        return true;
    }
}
