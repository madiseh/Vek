// Copyright (c) Vek 2021
// Vek licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.IO.Compression;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using NUnit.Framework;
using Vek.Components.DataGrid.Export;

namespace Vek.UnitTests.Components;

#nullable enable

[TestFixture]
public class DataGridExportTests : BunitTest
{
    public record Person(string Name, decimal Salary, DateTime Joined, string Department);

    private static readonly Person[] People =
    [
        new("Alice, \"A\"", 12.5m, new DateTime(2024, 1, 2), "Engineering"),
        new("Bob", 20m, new DateTime(2023, 5, 4), "Operations")
    ];

    private static void RenderColumns(RenderTreeBuilder builder)
    {
        builder.OpenComponent<PropertyColumn<Person, string>>(0);
        builder.AddAttribute(1, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(person => person.Name));
        builder.AddAttribute(2, nameof(PropertyColumn<Person, string>.Title), "Employee, name");
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<Person, decimal>>(3);
        builder.AddAttribute(4, nameof(PropertyColumn<Person, decimal>.Property), (Expression<Func<Person, decimal>>)(person => person.Salary));
        builder.AddAttribute(5, nameof(PropertyColumn<Person, decimal>.Format), "C2");
        builder.AddAttribute(6, nameof(PropertyColumn<Person, decimal>.Culture), CultureInfo.GetCultureInfo("en-US"));
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<Person, DateTime>>(7);
        builder.AddAttribute(8, nameof(PropertyColumn<Person, DateTime>.Property), (Expression<Func<Person, DateTime>>)(person => person.Joined));
        builder.AddAttribute(9, nameof(PropertyColumn<Person, DateTime>.Format), "yyyy-MM-dd");
        builder.CloseComponent();

        builder.OpenComponent<PropertyColumn<Person, string>>(10);
        builder.AddAttribute(11, nameof(PropertyColumn<Person, string>.Property), (Expression<Func<Person, string>>)(person => person.Department));
        builder.AddAttribute(12, nameof(PropertyColumn<Person, string>.Hidden), true);
        builder.CloseComponent();
    }

    /// <summary>
    /// CSV output quotes delimited values and formats values with the column culture.
    /// </summary>
    [Test]
    public async Task CsvExport_QuotesValuesAndAppliesColumnFormats()
    {
        var grid = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(parameter => parameter.Items, People)
            .Add(parameter => parameter.Columns, RenderColumns));
        await using var stream = new MemoryStream();
        var options = new DataGridExportOptions { Delimiter = ';' };

        await grid.Instance.ExportToCsvAsync(stream, options);

        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var csv = await reader.ReadToEndAsync();
        csv.Should().Contain("Employee, name;Salary;Joined");
        csv.Should().Contain("\"Alice, \"\"A\"\"\";$12.50;2024-01-02");
        csv.Should().NotContain("Department");
    }

    /// <summary>
    /// CSV output uses the active filtered items and supports custom column formatters.
    /// </summary>
    [Test]
    public async Task CsvExport_UsesFilteredItemsAndCustomFormatter()
    {
        var grid = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(parameter => parameter.Items, People)
            .Add(parameter => parameter.QuickFilter, person => person.Name.StartsWith("Alice", StringComparison.Ordinal))
            .Add(parameter => parameter.Columns, RenderColumns));
        await using var stream = new MemoryStream();
        var options = new DataGridExportOptions
        {
            CustomFormatters = new Dictionary<string, Func<object?, string?>>
            {
                ["Salary"] = value => $"custom:{value}"
            }
        };

        await grid.Instance.ExportToCsvAsync(stream, options);

        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var csv = await reader.ReadToEndAsync();
        csv.Should().Contain("custom:12.5");
        csv.Should().NotContain("Bob");
    }

    /// <summary>
    /// Export options can omit headers, include hidden columns, and select the raw item source.
    /// </summary>
    [Test]
    public async Task CsvExport_OptionsIncludeHiddenColumnsAndUnfilteredItems()
    {
        var grid = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(parameter => parameter.Items, People)
            .Add(parameter => parameter.QuickFilter, person => person.Name.StartsWith("Alice", StringComparison.Ordinal))
            .Add(parameter => parameter.Columns, RenderColumns));
        await using var stream = new MemoryStream();
        var options = new DataGridExportOptions
        {
            IncludeHeaders = false,
            OnlyFilteredItems = false,
            ExportHiddenColumns = true
        };

        await grid.Instance.ExportToCsvAsync(stream, options);

        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        var csv = await reader.ReadToEndAsync();
        csv.Should().Contain("Engineering");
        csv.Should().Contain("Operations");
        csv.Should().NotContain("Department");
    }

    /// <summary>
    /// File-name exports request a browser download using the matching media type.
    /// </summary>
    [Test]
    public async Task CsvExport_FileNameStartsBrowserDownload()
    {
        var grid = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(parameter => parameter.Items, People)
            .Add(parameter => parameter.Columns, RenderColumns));
        Context.JSInterop.SetupVoid("vekDataGridExport.downloadFileFromStream");

        await grid.Instance.ExportToCsvAsync("people.csv");

        var invocations = Context.JSInterop.Invocations
            .Where(invocation => invocation.Identifier == "vekDataGridExport.downloadFileFromStream")
            .ToArray();
        var invocation = invocations.Should().ContainSingle().Which;
        invocation.Arguments[0].Should().Be("people.csv");
        invocation.Arguments[1].Should().Be("text/csv;charset=utf-8");
    }

    /// <summary>
    /// XLSX output is a valid workbook package with headers, formatted cells, and visible columns.
    /// </summary>
    [Test]
    public async Task XlsxExport_WritesStreamingWorksheetPackage()
    {
        var grid = Context.Render<VekDataGrid<Person>>(parameters => parameters
            .Add(parameter => parameter.Items, People)
            .Add(parameter => parameter.Columns, RenderColumns));
        await using var stream = new MemoryStream();

        await grid.Instance.ExportToXlsxAsync(stream);

        stream.Position = 0;
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        archive.GetEntry("xl/workbook.xml").Should().NotBeNull();
        foreach (var entry in archive.Entries.Where(entry => entry.FullName.EndsWith(".xml", StringComparison.Ordinal) || entry.FullName.EndsWith(".rels", StringComparison.Ordinal)))
        {
            using var partStream = entry.Open();
            XDocument.Load(partStream).Should().NotBeNull();
        }
        var worksheet = archive.GetEntry("xl/worksheets/sheet1.xml");
        worksheet.Should().NotBeNull();
        using var worksheetStream = worksheet!.Open();
        var document = XDocument.Load(worksheetStream);
        var spreadsheetNamespace = (XNamespace)"http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var values = document.Descendants(spreadsheetNamespace + "t").Select(element => element.Value).ToArray();
        values.Should().Contain("Employee, name");
        values.Should().Contain("$12.50");
        values.Should().Contain("2024-01-02");
        values.Should().NotContain("Department");
    }
}
