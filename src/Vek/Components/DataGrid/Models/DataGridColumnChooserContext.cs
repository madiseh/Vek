// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using Vek.Extensions;
using Vek.Interfaces;

namespace Vek;

/// <summary>
/// Context passed to a <see cref="VekDataGrid{T}"/> column chooser template or component.
/// </summary>
/// <typeparam name="T">The kind of item managed by the data grid.</typeparam>
public class DataGridColumnChooserContext<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
{
    /// <summary>
    /// Gets the data grid instance associated with this column chooser context.
    /// </summary>
    public VekDataGrid<T> DataGrid { get; }

    /// <summary>
    /// Gets the optional anchor element identifier for positioning the chooser dialog.
    /// </summary>
    public string? AnchorId { get; }

    /// <summary>
    /// Gets a value indicating whether the column chooser dialog is currently open.
    /// </summary>
    public bool IsOpen => DataGrid.IsColumnChooserVisible;

    /// <summary>
    /// Gets the list of rendered columns in the data grid.
    /// </summary>
    public IReadOnlyList<Column<T>> Columns => DataGrid.RenderedColumns;

    /// <summary>
    /// Gets the list of column chooser items providing metadata and visibility state.
    /// </summary>
    public IReadOnlyList<DataGridColumnChooserItem<T>> Items =>
        DataGrid.RenderedColumns.Select(c => new DataGridColumnChooserItem<T>(c)).ToList();

    /// <summary>
    /// Toggles the visibility of the specified column asynchronously.
    /// </summary>
    /// <param name="column">The column whose visibility is to be toggled.</param>
    public async Task ToggleColumnVisibilityAsync(Column<T> column)
    {
        ArgumentNullException.ThrowIfNull(column);

        if (!column.GetState(x => x.Hidden) && !column.GetState(x => x.AllowHiding))
        {
            return;
        }

        await column.ToggleAsync();
    }

    /// <summary>
    /// Shows all columns asynchronously.
    /// </summary>
    public async Task ShowAllColumnsAsync()
    {
        foreach (var column in DataGrid.RenderedColumns)
        {
            await column.HiddenState.SetValueAsync(false);
        }

        DataGrid.DropContainerHasChanged();
        ((IVekStateHasChanged)DataGrid).StateHasChanged();
    }

    /// <summary>
    /// Hides all hideable columns asynchronously.
    /// </summary>
    public async Task HideAllColumnsAsync()
    {
        foreach (var column in DataGrid.RenderedColumns)
        {
            if (column.GetState(x => x.AllowHiding))
            {
                await column.HiddenState.SetValueAsync(true);
            }
        }

        DataGrid.DropContainerHasChanged();
        ((IVekStateHasChanged)DataGrid).StateHasChanged();
    }

    /// <summary>
    /// Moves the specified column one position earlier in the rendered columns order.
    /// </summary>
    /// <param name="column">The column to move.</param>
    public void MoveColumnUp(Column<T> column) => DataGrid.MoveColumnUp(column);

    /// <summary>
    /// Moves the specified column one position later in the rendered columns order.
    /// </summary>
    /// <param name="column">The column to move.</param>
    public void MoveColumnDown(Column<T> column) => DataGrid.MoveColumnDown(column);

    /// <summary>
    /// Closes the column chooser dialog asynchronously.
    /// </summary>
    public Task CloseAsync() => DataGrid.HideColumnChooserAsync();

    public DataGridColumnChooserContext(VekDataGrid<T> dataGrid, string? anchorId = null)
    {
        DataGrid = dataGrid;
        AnchorId = anchorId;
    }
}

/// <summary>
/// Represents column metadata and visibility state inside a <see cref="DataGridColumnChooserContext{T}"/>.
/// </summary>
/// <typeparam name="T">The kind of item managed by the data grid.</typeparam>
public class DataGridColumnChooserItem<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
{
    /// <summary>
    /// Gets the underlying column instance.
    /// </summary>
    public Column<T> Column { get; }

    /// <summary>
    /// Gets the title of the column.
    /// </summary>
    public string? Title => Column.Title;

    /// <summary>
    /// Gets a value indicating whether the column is currently visible.
    /// </summary>
    public bool Visible => !Column.GetState(x => x.Hidden);

    /// <summary>
    /// Gets a value indicating whether the column allows hiding.
    /// </summary>
    public bool AllowHiding => Column.GetState(x => x.AllowHiding);

    /// <summary>
    /// Toggles the visibility of this column asynchronously.
    /// </summary>
    public Task ToggleAsync() => Column.ToggleAsync();

    public DataGridColumnChooserItem(Column<T> column)
    {
        Column = column;
    }
}
