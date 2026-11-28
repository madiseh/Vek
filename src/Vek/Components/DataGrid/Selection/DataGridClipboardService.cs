// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vek.Components.DataGrid.Export;
using Vek.Extensions;

namespace Vek;

/// <summary>
/// Provides clipboard serialization and copy operations for <see cref="VekDataGrid{T}"/>.
/// </summary>
/// <typeparam name="T">The data grid item type.</typeparam>
public class DataGridClipboardService<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>
{
    private readonly VekDataGrid<T> _dataGrid;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridClipboardService{T}"/> class.
    /// </summary>
    /// <param name="dataGrid">The parent data grid instance.</param>
    public DataGridClipboardService(VekDataGrid<T> dataGrid)
    {
        _dataGrid = dataGrid ?? throw new ArgumentNullException(nameof(dataGrid));
    }

    /// <summary>
    /// Generates formatted text representing the currently selected rows or cell range according to the provided options.
    /// </summary>
    /// <param name="options">Options controlling delimiters, headers, scope, and cell formatting.</param>
    /// <returns>A formatted string ready for clipboard insertion.</returns>
    public string GenerateClipboardText(DataGridClipboardOptions? options = null)
    {
        options ??= _dataGrid.GetState(x => x.ClipboardOptions) ?? new DataGridClipboardOptions();
        var delimiter = options.Delimiter;
        var renderedColumns = _dataGrid.RenderedColumns.Where(c => !c.HiddenState.Value).ToList();
        if (renderedColumns.Count == 0)
        {
            return string.Empty;
        }

        var visibleRows = _dataGrid.GetVisibleRowItems();

        // 1. Cell range scope or auto with active cell range
        var cellRange = _dataGrid.GetState(x => x.SelectedCellRange);
        if ((options.Scope == DataGridClipboardCopyScope.CellRange || options.Scope == DataGridClipboardCopyScope.Auto)
            && cellRange is { } range)
        {
            return FormatCellRange(range, renderedColumns, visibleRows, options, delimiter);
        }

        // 2. Selected rows scope or auto with selected items
        var selectedItems = GetItemsToCopy(options, visibleRows);
        if (selectedItems.Count > 0)
        {
            return FormatRows(selectedItems, renderedColumns, options, delimiter);
        }

        return string.Empty;
    }

    /// <summary>
    /// Copies the selected cells or rows to the system clipboard asynchronously.
    /// </summary>
    /// <param name="options">Optional options controlling copy scope and formatting.</param>
    /// <returns>The copied text, or <see langword="null"/> if nothing was copied.</returns>
    public async Task<string?> CopyToClipboardAsync(DataGridClipboardOptions? options = null)
    {
        if (!_dataGrid.GetState(x => x.ClipboardCopyEnabled))
        {
            return null;
        }

        var text = GenerateClipboardText(options);
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        await _dataGrid.CopyTextToClipboardAsync(text);
        return text;
    }

    private List<T> GetItemsToCopy(DataGridClipboardOptions options, IReadOnlyList<T> visibleRows)
    {
        if (options.Scope == DataGridClipboardCopyScope.CellRange)
        {
            return [];
        }

        var selected = _dataGrid.Selection;
        if (selected.Count > 0)
        {
            // Maintain visible row order
            return visibleRows.Where(r => selected.Contains(r)).ToList();
        }

        var selectedItem = _dataGrid.GetState(x => x.SelectedItem);
        if (selectedItem is not null)
        {
            return [selectedItem];
        }

        // Fallback to focused item if enabled
        var focusedItem = _dataGrid.GetState(x => x.FocusedItem);
        if (focusedItem is not null)
        {
            return [focusedItem];
        }

        return [];
    }

    private static string FormatCellRange(
        DataGridCellRange range,
        IReadOnlyList<Column<T>> allColumns,
        IReadOnlyList<T> visibleRows,
        DataGridClipboardOptions options,
        char delimiter)
    {
        var minCol = Math.Clamp(range.MinColumn, 0, allColumns.Count - 1);
        var maxCol = Math.Clamp(range.MaxColumn, 0, allColumns.Count - 1);
        var minRow = Math.Clamp(range.MinRow, 0, visibleRows.Count - 1);
        var maxRow = Math.Clamp(range.MaxRow, 0, visibleRows.Count - 1);

        if (minCol > maxCol || minRow > maxRow || visibleRows.Count == 0)
        {
            return string.Empty;
        }

        var targetColumns = new List<Column<T>>();
        for (var c = minCol; c <= maxCol; c++)
        {
            targetColumns.Add(allColumns[c]);
        }

        var sb = new StringBuilder();

        if (options.IncludeHeaders)
        {
            var headerValues = targetColumns.Select(DataGridExportFormatter.GetHeader);
            sb.AppendLine(string.Join(delimiter, headerValues.Select(v => EscapeValue(v, delimiter))));
        }

        for (var r = minRow; r <= maxRow; r++)
        {
            var item = visibleRows[r];
            var rowValues = targetColumns.Select(c => FormatCellValue(c, item, options));
            sb.AppendLine(string.Join(delimiter, rowValues.Select(v => EscapeValue(v, delimiter))));
        }

        // Return without trailing newline for clean clipboard paste
        return sb.ToString().TrimEnd('\r', '\n');
    }

    private static string FormatRows(
        IReadOnlyList<T> items,
        IReadOnlyList<Column<T>> columns,
        DataGridClipboardOptions options,
        char delimiter)
    {
        var sb = new StringBuilder();

        if (options.IncludeHeaders)
        {
            var headerValues = columns.Select(DataGridExportFormatter.GetHeader);
            sb.AppendLine(string.Join(delimiter, headerValues.Select(v => EscapeValue(v, delimiter))));
        }

        foreach (var item in items)
        {
            var rowValues = columns.Select(c => FormatCellValue(c, item, options));
            sb.AppendLine(string.Join(delimiter, rowValues.Select(v => EscapeValue(v, delimiter))));
        }

        return sb.ToString().TrimEnd('\r', '\n');
    }

    private static string FormatCellValue(Column<T> column, T item, DataGridClipboardOptions options)
    {
        var value = column.GetCellContent(item);
        var key = column.PropertyName ?? column.Identifier;
        if (key is not null && options.CustomFormatters?.TryGetValue(key, out var formatter) == true)
        {
            return formatter(value) ?? string.Empty;
        }

        if (value is null)
        {
            return string.Empty;
        }

        var culture = column.Culture ?? CultureInfo.CurrentCulture;
        return value is IFormattable formattable
            ? formattable.ToString(column.ContentFormat, culture) ?? string.Empty
            : value.ToString() ?? string.Empty;
    }

    private static string EscapeValue(string value, char delimiter)
    {
        if (delimiter == '\t')
        {
            // TSV standard: if value contains tabs or newlines, quote and escape double quotes
            if (!value.Contains('\t') && !value.Contains('\r') && !value.Contains('\n') && !value.Contains('"'))
            {
                return value;
            }

            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }

        // CSV standard: if value contains delimiter, quotes, or newlines, quote and escape
        if (!value.Contains(delimiter) && !value.Contains('"') && !value.Contains('\r') && !value.Contains('\n'))
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
    }
}
