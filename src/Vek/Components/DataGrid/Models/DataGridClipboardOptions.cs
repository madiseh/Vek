// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace Vek;

/// <summary>
/// Specifies the delimiter used when copying data grid content to the clipboard.
/// </summary>
public enum DataGridClipboardFormat
{
    /// <summary>
    /// Tab-separated values, optimized for direct paste into spreadsheet applications such as Excel and Google Sheets.
    /// </summary>
    Tsv,

    /// <summary>
    /// Comma-separated values.
    /// </summary>
    Csv
}

/// <summary>
/// Specifies the selection scope used for clipboard copy operations.
/// </summary>
public enum DataGridClipboardCopyScope
{
    /// <summary>
    /// Automatically determines scope: copies selected cell range if present, otherwise selected rows, or active/focused row.
    /// </summary>
    Auto,

    /// <summary>
    /// Copies the currently selected rectangular cell range.
    /// </summary>
    CellRange,

    /// <summary>
    /// Copies the currently selected rows.
    /// </summary>
    SelectedRows
}

/// <summary>
/// Configures clipboard copy behavior for <see cref="VekDataGrid{T}"/>.
/// </summary>
public sealed class DataGridClipboardOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether column headers are included in the copied clipboard content.
    /// </summary>
    /// <remarks>
    /// Defaults to <c>true</c>.
    /// </remarks>
    public bool IncludeHeaders { get; set; } = true;

    /// <summary>
    /// Gets or sets the text format used when generating clipboard data.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="DataGridClipboardFormat.Tsv"/>.
    /// </remarks>
    public DataGridClipboardFormat Format { get; set; } = DataGridClipboardFormat.Tsv;

    /// <summary>
    /// Gets or sets the selection scope to copy.
    /// </summary>
    /// <remarks>
    /// Defaults to <see cref="DataGridClipboardCopyScope.Auto"/>.
    /// </remarks>
    public DataGridClipboardCopyScope Scope { get; set; } = DataGridClipboardCopyScope.Auto;

    /// <summary>
    /// Gets or sets optional custom cell formatters mapped by column property name or identifier.
    /// </summary>
    public IReadOnlyDictionary<string, Func<object?, string?>>? CustomFormatters { get; set; }

    /// <summary>
    /// Gets the delimiter character corresponding to <see cref="Format"/>.
    /// </summary>
    public char Delimiter => Format switch
    {
        DataGridClipboardFormat.Csv => ',',
        _ => '\t'
    };
}
