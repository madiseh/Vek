// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace Vek.Components.DataGrid.Export
{
    internal static class DataGridXlsxExporter
    {
        private const string SpreadsheetNamespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private const string RelationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private const string PackageRelationshipNamespace = "http://schemas.openxmlformats.org/package/2006/relationships";
        private const string ContentTypesNamespace = "http://schemas.openxmlformats.org/package/2006/content-types";

        public static async Task WriteAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
            Stream stream,
            IReadOnlyList<Column<T>> columns,
            IEnumerable<T> items,
            DataGridExportOptions options)
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true);
            await WriteEntryAsync(archive, "[Content_Types].xml", WriteContentTypesAsync);
            await WriteEntryAsync(archive, "_rels/.rels", WritePackageRelationshipsAsync);
            await WriteEntryAsync(archive, "xl/workbook.xml", WriteWorkbookAsync);
            await WriteEntryAsync(archive, "xl/_rels/workbook.xml.rels", WriteWorkbookRelationshipsAsync);
            await WriteWorksheetAsync(archive, columns, items, options);
        }

        private static async Task WriteWorksheetAsync<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] T>(
            ZipArchive archive,
            IReadOnlyList<Column<T>> columns,
            IEnumerable<T> items,
            DataGridExportOptions options)
        {
            var entry = archive.CreateEntry("xl/worksheets/sheet1.xml", CompressionLevel.Fastest);
            using var stream = entry.Open();
            using var writer = CreateXmlWriter(stream);

            await writer.WriteStartDocumentAsync();
            await writer.WriteStartElementAsync(null, "worksheet", SpreadsheetNamespace);
            await writer.WriteStartElementAsync(null, "sheetData", SpreadsheetNamespace);

            var rowIndex = 1;
            if (options.IncludeHeaders)
            {
                await WriteRowAsync(writer, rowIndex++, columns.Select(DataGridExportFormatter.GetHeader));
            }

            foreach (var item in items)
            {
                await WriteRowAsync(writer, rowIndex++, columns.Select(column => DataGridExportFormatter.Format(column, item, options)));
            }

            await writer.WriteEndElementAsync();
            await writer.WriteEndElementAsync();
            await writer.WriteEndDocumentAsync();
            await writer.FlushAsync();
        }

        private static async Task WriteRowAsync(XmlWriter writer, int rowIndex, IEnumerable<string> values)
        {
            await writer.WriteStartElementAsync(null, "row", SpreadsheetNamespace);
            await writer.WriteAttributeStringAsync(null, "r", null, rowIndex.ToString(CultureInfo.InvariantCulture));

            var columnIndex = 0;
            foreach (var value in values)
            {
                await writer.WriteStartElementAsync(null, "c", SpreadsheetNamespace);
                await writer.WriteAttributeStringAsync(null, "r", null, $"{GetColumnName(++columnIndex)}{rowIndex}");
                await writer.WriteAttributeStringAsync(null, "t", null, "inlineStr");
                await writer.WriteStartElementAsync(null, "is", SpreadsheetNamespace);
                await writer.WriteStartElementAsync(null, "t", SpreadsheetNamespace);
                if (value.Length > 0 && (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[^1])))
                {
                    await writer.WriteAttributeStringAsync("xml", "space", "http://www.w3.org/XML/1998/namespace", "preserve");
                }
                await writer.WriteStringAsync(value);
                await writer.WriteEndElementAsync();
                await writer.WriteEndElementAsync();
                await writer.WriteEndElementAsync();
            }

            await writer.WriteEndElementAsync();
        }

        private static string GetColumnName(int columnIndex)
        {
            var result = new StringBuilder();
            while (columnIndex > 0)
            {
                columnIndex--;
                result.Insert(0, (char)('A' + (columnIndex % 26)));
                columnIndex /= 26;
            }
            return result.ToString();
        }

        private static Task WriteContentTypesAsync(XmlWriter writer)
        {
            return WriteXmlAsync(writer, async () =>
            {
                await writer.WriteStartElementAsync(null, "Types", ContentTypesNamespace);
                await WriteEmptyElementAsync(writer, "Default", "Extension", "rels", "ContentType", "application/vnd.openxmlformats-package.relationships+xml");
                await WriteEmptyElementAsync(writer, "Default", "Extension", "xml", "ContentType", "application/xml");
                await WriteEmptyElementAsync(writer, "Override", "PartName", "/xl/workbook.xml", "ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");
                await WriteEmptyElementAsync(writer, "Override", "PartName", "/xl/worksheets/sheet1.xml", "ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml");
                await writer.WriteEndElementAsync();
            });
        }

        private static Task WritePackageRelationshipsAsync(XmlWriter writer)
        {
            return WriteXmlAsync(writer, async () =>
            {
                await writer.WriteStartElementAsync(null, "Relationships", PackageRelationshipNamespace);
                await writer.WriteStartElementAsync(null, "Relationship", PackageRelationshipNamespace);
                await writer.WriteAttributeStringAsync(null, "Id", null, "rId1");
                await writer.WriteAttributeStringAsync(null, "Type", null, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument");
                await writer.WriteAttributeStringAsync(null, "Target", null, "xl/workbook.xml");
                await writer.WriteEndElementAsync();
                await writer.WriteEndElementAsync();
            });
        }

        private static Task WriteWorkbookAsync(XmlWriter writer)
        {
            return WriteXmlAsync(writer, async () =>
            {
                await writer.WriteStartElementAsync(null, "workbook", SpreadsheetNamespace);
                await writer.WriteAttributeStringAsync("xmlns", "r", null, RelationshipNamespace);
                await writer.WriteStartElementAsync(null, "sheets", SpreadsheetNamespace);
                await writer.WriteStartElementAsync(null, "sheet", SpreadsheetNamespace);
                await writer.WriteAttributeStringAsync(null, "name", null, "Data");
                await writer.WriteAttributeStringAsync(null, "sheetId", null, "1");
                await writer.WriteAttributeStringAsync("r", "id", RelationshipNamespace, "rId1");
                await writer.WriteEndElementAsync();
                await writer.WriteEndElementAsync();
                await writer.WriteEndElementAsync();
            });
        }

        private static Task WriteWorkbookRelationshipsAsync(XmlWriter writer)
        {
            return WriteXmlAsync(writer, async () =>
            {
                await writer.WriteStartElementAsync(null, "Relationships", PackageRelationshipNamespace);
                await writer.WriteStartElementAsync(null, "Relationship", PackageRelationshipNamespace);
                await writer.WriteAttributeStringAsync(null, "Id", null, "rId1");
                await writer.WriteAttributeStringAsync(null, "Type", null, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet");
                await writer.WriteAttributeStringAsync(null, "Target", null, "worksheets/sheet1.xml");
                await writer.WriteEndElementAsync();
                await writer.WriteEndElementAsync();
            });
        }

        private static async Task WriteEntryAsync(ZipArchive archive, string name, Func<XmlWriter, Task> write)
        {
            var entry = archive.CreateEntry(name, CompressionLevel.Fastest);
            using var stream = entry.Open();
            using var writer = CreateXmlWriter(stream);
            await writer.WriteStartDocumentAsync();
            await write(writer);
            await writer.WriteEndDocumentAsync();
            await writer.FlushAsync();
        }

        private static async Task WriteXmlAsync(XmlWriter writer, Func<Task> write)
        {
            await write();
        }

        private static async Task WriteEmptyElementAsync(XmlWriter writer, string name, string firstAttribute, string firstValue, string secondAttribute, string secondValue)
        {
            await writer.WriteStartElementAsync(null, name, ContentTypesNamespace);
            await writer.WriteAttributeStringAsync(null, firstAttribute, null, firstValue);
            await writer.WriteAttributeStringAsync(null, secondAttribute, null, secondValue);
            await writer.WriteEndElementAsync();
        }

        private static XmlWriter CreateXmlWriter(Stream stream)
        {
            return XmlWriter.Create(stream, new XmlWriterSettings
            {
                Async = true,
                CloseOutput = false,
                Encoding = new UTF8Encoding(false),
                Indent = false
            });
        }
    }
}
