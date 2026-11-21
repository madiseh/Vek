// Copyright (c) Vek 2026
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.Web;
using Vek.Interfaces;

namespace Vek;

/// <summary>
/// Identifies the region of the <see cref="VekDataGrid{T}"/> that triggered a context menu action.
/// </summary>
public enum DataGridContextMenuTarget
{
    /// <summary>
    /// Triggered on a column header cell.
    /// </summary>
    Header,

    /// <summary>
    /// Triggered on a data row.
    /// </summary>
    Row,

    /// <summary>
    /// Triggered on a specific cell within a data row.
    /// </summary>
    Cell,

    /// <summary>
    /// Triggered on a summary or footer cell.
    /// </summary>
    Footer
}

/// <summary>
/// Event arguments and action context for <see cref="VekDataGrid{T}"/> right-click context menu events.
/// </summary>
/// <typeparam name="T">The type of item managed by the grid.</typeparam>
public class DataGridContextMenuArgs<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T> : EventArgs
{
    /// <summary>
    /// The pointer event arguments containing mouse coordinates and button states.
    /// </summary>
    public MouseEventArgs MouseEventArgs { get; }

    /// <summary>
    /// The grid area where the context menu was requested.
    /// </summary>
    public DataGridContextMenuTarget Target { get; }

    /// <summary>
    /// The row data item, or <c>default</c> when triggered on a header or footer.
    /// </summary>
    public T? Item { get; }

    /// <summary>
    /// The zero-based row index, or <c>null</c> when triggered outside data rows.
    /// </summary>
    public int? RowIndex { get; }

    /// <summary>
    /// The zero-based column index, or <c>null</c> when triggered outside columns.
    /// </summary>
    public int? ColumnIndex { get; }

    /// <summary>
    /// The target column, or <c>null</c> when triggered outside column boundaries.
    /// </summary>
    public Column<T>? Column { get; }

    /// <summary>
    /// The parent <see cref="VekDataGrid{T}"/> instance.
    /// </summary>
    public VekDataGrid<T> DataGrid { get; }

    /// <summary>
    /// The raw cell value, or <c>null</c> when not available.
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// The formatted display text of the target cell or header.
    /// </summary>
    public string? FormattedValue { get; }

    /// <summary>
    /// When set to <c>true</c>, prevents default context menu display.
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Indicates whether the target column has an active filter applied.
    /// </summary>
    public bool HasFilter => Column is not null && DataGrid.HasFilter(Column);

    /// <summary>
    /// Indicates whether the target column can be pinned.
    /// </summary>
    public bool CanPin => Column is not null && (Column.AllowPinning ?? DataGrid.AllowColumnPinning);

    /// <summary>
    /// Gets the current sort direction applied to the target column.
    /// </summary>
    public SortDirection SortDirection => Column?.PropertyName is not null ? DataGrid.GetColumnSortDirection(Column.PropertyName) : SortDirection.None;

    /// <summary>
    /// Initializes a new instance of <see cref="DataGridContextMenuArgs{T}"/>.
    /// </summary>
    public DataGridContextMenuArgs(
        MouseEventArgs mouseEventArgs,
        DataGridContextMenuTarget target,
        T? item,
        int? rowIndex,
        int? columnIndex,
        Column<T>? column,
        VekDataGrid<T> dataGrid,
        object? value,
        string? formattedValue)
    {
        MouseEventArgs = mouseEventArgs;
        Target = target;
        Item = item;
        RowIndex = rowIndex;
        ColumnIndex = columnIndex;
        Column = column;
        DataGrid = dataGrid;
        Value = value;
        FormattedValue = formattedValue;
    }

    /// <summary>
    /// Copies the cell text or formatted value to the system clipboard.
    /// </summary>
    public async Task CopyCellAsync()
    {
        var text = FormattedValue ?? Value?.ToString() ?? string.Empty;
        await DataGrid.CopyTextToClipboardAsync(text);
        await CloseMenuAsync();
    }

    /// <summary>
    /// Sorts the target column in ascending order.
    /// </summary>
    public async Task SortAscendingAsync()
    {
        if (Column?.PropertyName is not null)
        {
            await DataGrid.SetSortAsync(Column.PropertyName, SortDirection.Ascending, Column.GetLocalSortFunc(), Column.Comparer);
        }
        await CloseMenuAsync();
    }

    /// <summary>
    /// Sorts the target column in descending order.
    /// </summary>
    public async Task SortDescendingAsync()
    {
        if (Column?.PropertyName is not null)
        {
            await DataGrid.SetSortAsync(Column.PropertyName, SortDirection.Descending, Column.GetLocalSortFunc(), Column.Comparer);
        }
        await CloseMenuAsync();
    }

    /// <summary>
    /// Removes sorting from the target column.
    /// </summary>
    public async Task ClearSortAsync()
    {
        if (Column?.PropertyName is not null)
        {
            await DataGrid.RemoveSortAsync(Column.PropertyName);
        }
        await CloseMenuAsync();
    }

    /// <summary>
    /// Groups the grid by the target column.
    /// </summary>
    public async Task GroupByAsync()
    {
        if (Column is not null)
        {
            await Column.SetGroupingAsync(true);
            await DataGrid.UpdateGroupingOrder(Column, true);
            DataGrid.GroupItems();
            DataGrid.DropContainerHasChanged();
        }
        await CloseMenuAsync();
    }

    /// <summary>
    /// Removes grouping on the target column.
    /// </summary>
    public async Task UngroupAsync()
    {
        if (Column is not null)
        {
            await Column.SetGroupingAsync(false);
            await DataGrid.UpdateGroupingOrder(Column, false);
            DataGrid.GroupItems();
            DataGrid.DropContainerHasChanged();
        }
        await CloseMenuAsync();
    }

    /// <summary>
    /// Toggles grouping state on the target column.
    /// </summary>
    public Task ToggleGroupingAsync()
    {
        return Column?.GroupingState.Value == true ? UngroupAsync() : GroupByAsync();
    }

    /// <summary>
    /// Applies a filter matching the clicked cell value.
    /// </summary>
    public async Task FilterByCellValueAsync()
    {
        if (Column is not null && Value is not null)
        {
            DataGrid.FilterDefinitions.RemoveAll(x =>
                ReferenceEquals(x.Column, Column) ||
                (Column.PropertyName is not null && x.Column?.PropertyName == Column.PropertyName));

            string op = Value switch
            {
                sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal => FilterOperator.Number.Equal,
                System.DateTime or DateTimeOffset or DateOnly => FilterOperator.DateTime.Is,
                bool => FilterOperator.Boolean.Is,
                _ => FilterOperator.String.Equal
            };

            var filterDefinition = new FilterDefinition<T>
            {
                Id = Guid.NewGuid(),
                Column = Column,
                Operator = op,
                Value = Value
            };

            DataGrid.FilterDefinitions.Add(filterDefinition);
            await DataGrid.InvokeServerLoadFunc();
            DataGrid.GroupItems();
            await DataGrid.NotifyFilterChangedAsync();
            DataGrid.DropContainerHasChanged();
            ((IVekStateHasChanged)DataGrid).StateHasChanged();
        }
        await CloseMenuAsync();
    }

    /// <summary>
    /// Opens the filter interface for the target column.
    /// </summary>
    public async Task OpenFilterAsync()
    {
        if (Column?.HeaderCell is not null)
        {
            Column.HeaderCell.OpenFilters(MouseEventArgs);
        }
        await CloseMenuAsync();
    }

    /// <summary>
    /// Clears any active filter on the target column.
    /// </summary>
    public async Task ClearFilterAsync()
    {
        if (Column is not null)
        {
            DataGrid.FilterDefinitions.RemoveAll(x =>
                ReferenceEquals(x.Column, Column) ||
                (Column.PropertyName is not null && x.Column?.PropertyName == Column.PropertyName));

            await DataGrid.InvokeServerLoadFunc();
            DataGrid.GroupItems();
            await DataGrid.NotifyFilterChangedAsync();
            DataGrid.DropContainerHasChanged();
            ((IVekStateHasChanged)DataGrid).StateHasChanged();
        }
        await CloseMenuAsync();
    }

    /// <summary>
    /// Automatically fits the target column width to its content.
    /// </summary>
    public async Task AutoFitAsync()
    {
        if (Column is not null)
        {
            await DataGrid.AutoFitColumnWidths(Column);
        }
        await CloseMenuAsync();
    }

    /// <summary>
    /// Automatically fits all visible column widths to their content.
    /// </summary>
    public async Task AutoFitAllAsync()
    {
        await DataGrid.AutoFitColumnWidths();
        await CloseMenuAsync();
    }

    /// <summary>
    /// Pins the target column to the left side of the grid.
    /// </summary>
    public async Task PinLeftAsync()
    {
        if (Column is not null)
        {
            await Column.SetFixedStyleAsync(DataGridFixedStyle.Left);
            await DataGrid.PinColumnAsync(Column, DataGridFixedStyle.Left);
        }
        await CloseMenuAsync();
    }

    /// <summary>
    /// Pins the target column to the right side of the grid.
    /// </summary>
    public async Task PinRightAsync()
    {
        if (Column is not null)
        {
            await Column.SetFixedStyleAsync(DataGridFixedStyle.Right);
            await DataGrid.PinColumnAsync(Column, DataGridFixedStyle.Right);
        }
        await CloseMenuAsync();
    }

    /// <summary>
    /// Unpins the target column from fixed sides.
    /// </summary>
    public async Task UnpinAsync()
    {
        if (Column is not null)
        {
            await Column.SetFixedStyleAsync(DataGridFixedStyle.None);
            await DataGrid.PinColumnAsync(Column, DataGridFixedStyle.None);
        }
        await CloseMenuAsync();
    }

    /// <summary>
    /// Closes the currently active context menu.
    /// </summary>
    public Task CloseMenuAsync()
    {
        return DataGrid.CloseContextMenuAsync();
    }
}
