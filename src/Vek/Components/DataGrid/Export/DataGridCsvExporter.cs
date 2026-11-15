// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Vek.Components.DataGrid.Export
{
    internal static class DataGridCsvExporter
    {
        public static async Task WriteAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
            Stream stream,
            IReadOnlyList<Column<T>> columns,
            IEnumerable<T> items,
            DataGridExportOptions options)
        {
            await using var writer = new StreamWriter(stream, new UTF8Encoding(false), 1024, leaveOpen: true);

            if (options.IncludeHeaders)
            {
                await WriteRowAsync(writer, columns.Select(DataGridExportFormatter.GetHeader), options.Delimiter);
            }

            foreach (var item in items)
            {
                await WriteRowAsync(writer, columns.Select(column => DataGridExportFormatter.Format(column, item, options)), options.Delimiter);
            }

            await writer.FlushAsync();
        }

        private static Task WriteRowAsync(TextWriter writer, IEnumerable<string> values, char delimiter)
        {
            var row = string.Join(delimiter, values.Select(value => Escape(value, delimiter)));
            return writer.WriteLineAsync(row);
        }

        private static string Escape(string value, char delimiter)
        {
            if (!value.Contains(delimiter) && !value.Contains('"') && !value.Contains('\r') && !value.Contains('\n'))
            {
                return value;
            }

            return $"\"{value.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
        }
    }
}
