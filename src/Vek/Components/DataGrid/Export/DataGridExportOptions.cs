// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Vek.Components.DataGrid.Export
{
    /// <summary>
    /// Configures which data and columns a data grid export contains.
    /// </summary>
    public sealed class DataGridExportOptions
    {
        /// <summary>
        /// Includes a header row in the exported data.
        /// </summary>
        public bool IncludeHeaders { get; init; } = true;

        /// <summary>
        /// Exports the grid's filtered and sorted items instead of its unfiltered item source.
        /// </summary>
        public bool OnlyFilteredItems { get; init; } = true;

        /// <summary>
        /// Includes columns hidden from the grid in the exported data.
        /// </summary>
        public bool ExportHiddenColumns { get; init; }

        /// <summary>
        /// Sets the delimiter used between CSV values.
        /// </summary>
        public char Delimiter { get; init; } = ',';

        /// <summary>
        /// Formats cell values by column property name.
        /// </summary>
        public IReadOnlyDictionary<string, Func<object?, string?>>? CustomFormatters { get; init; }
    }

    internal static class DataGridExportFormatter
    {
        public static string Format<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Column<T> column, T item, DataGridExportOptions options)
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

        public static string GetHeader<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(Column<T> column)
        {
            return string.IsNullOrWhiteSpace(column.Title)
                ? column.PropertyName ?? string.Empty
                : column.Title;
        }
    }
}
