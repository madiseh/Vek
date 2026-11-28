#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using NUnit.Framework;

namespace Vek.UnitTests.Components.DataGrid;

[TestFixture]
public class DataGridClipboardTests : BunitTest
{
    private sealed record Item(string Name, int Quantity, decimal Price);

    private static readonly Item[] SampleItems =
    [
        new("Widget A", 10, 19.99m),
        new("Widget B", 5, 29.50m),
        new("Gadget C", 2, 99.00m)
    ];

    [Test]
    public void GenerateClipboardText_CellRange_FormatsTsvCorrectly()
    {
        var comp = RenderGrid(
            selectionMode: DataGridSelectionMode.CellRange,
            clipboardEnabled: true,
            initialRange: new DataGridCellRange(0, 0, 1, 1));
        var grid = comp.Instance;

        var tsv = grid.ClipboardService.GenerateClipboardText(new DataGridClipboardOptions
        {
            IncludeHeaders = true,
            Format = DataGridClipboardFormat.Tsv
        });

        tsv.Should().Be("Name\tQuantity\r\nWidget A\t10\r\nWidget B\t5");
    }

    [Test]
    public void GenerateClipboardText_CellRange_WithoutHeaders_FormatsCorrectly()
    {
        var comp = RenderGrid(
            selectionMode: DataGridSelectionMode.CellRange,
            clipboardEnabled: true,
            initialRange: new DataGridCellRange(0, 1, 1, 2));
        var grid = comp.Instance;

        var tsv = grid.ClipboardService.GenerateClipboardText(new DataGridClipboardOptions
        {
            IncludeHeaders = false,
            Format = DataGridClipboardFormat.Tsv
        });

        tsv.Should().Contain("10\t19.99");
        tsv.Should().Contain("5\t29.50");
        tsv.Should().NotContain("Name");
    }

    [Test]
    public void GenerateClipboardText_CellRange_CsvFormat_FormatsDelimiters()
    {
        var comp = RenderGrid(
            selectionMode: DataGridSelectionMode.CellRange,
            clipboardEnabled: true,
            initialRange: new DataGridCellRange(0, 0, 1, 1));
        var grid = comp.Instance;

        var csv = grid.ClipboardService.GenerateClipboardText(new DataGridClipboardOptions
        {
            IncludeHeaders = true,
            Format = DataGridClipboardFormat.Csv
        });

        csv.Should().Be("Name,Quantity\r\nWidget A,10\r\nWidget B,5");
    }

    [Test]
    public void GenerateClipboardText_SelectedRows_FormatsAllColumns()
    {
        var comp = RenderGrid(multiSelection: true, clipboardEnabled: true);
        var grid = comp.Instance;

        grid.Selection.Add(SampleItems[0]);
        grid.Selection.Add(SampleItems[2]);

        var tsv = grid.ClipboardService.GenerateClipboardText();

        tsv.Should().Contain("Name\tQuantity\tPrice");
        tsv.Should().Contain("Widget A\t10\t19.99");
        tsv.Should().Contain("Gadget C\t2\t99.00");
        tsv.Should().NotContain("Widget B");
    }

    [Test]
    public async Task KeyDown_CtrlC_CopiesToClipboardWhenEnabled()
    {
        var comp = RenderGrid(
            selectionMode: DataGridSelectionMode.CellRange,
            clipboardEnabled: true,
            initialRange: new DataGridCellRange(0, 0, 0, 1));

        var table = comp.Find("table.vek-table-root");
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "c", CtrlKey = true });

        var invocation = Context.JSInterop.Invocations.FirstOrDefault(x => x.Identifier == "navigator.clipboard.writeText" || x.Identifier == "vekWindow.copyToClipboard");
        invocation.Should().NotBeNull();
        invocation!.Arguments[0].Should().Be("Name\tQuantity\r\nWidget A\t10");
    }

    [Test]
    public async Task KeyDown_CtrlC_DoesNothingWhenDisabled()
    {
        var comp = RenderGrid(
            selectionMode: DataGridSelectionMode.CellRange,
            clipboardEnabled: false,
            initialRange: new DataGridCellRange(0, 0, 0, 1));

        var table = comp.Find("table.vek-table-root");
        await table.KeyDownAsync(new KeyboardEventArgs { Key = "c", CtrlKey = true });

        var invocation = Context.JSInterop.Invocations.Where(x => x.Identifier == "navigator.clipboard.writeText" || x.Identifier == "vekWindow.copyToClipboard").ToList();
        invocation.Should().BeEmpty();
    }

    [Test]
    public async Task CopyToClipboardAsync_ReturnsNullWhenDisabledOrEmpty()
    {
        var comp = RenderGrid(selectionMode: DataGridSelectionMode.CellRange, clipboardEnabled: false);
        var resultDisabled = await comp.Instance.CopyToClipboardAsync();
        resultDisabled.Should().BeNull();

        var compEnabled = RenderGrid(selectionMode: DataGridSelectionMode.CellRange, clipboardEnabled: true);
        var resultEmpty = await compEnabled.Instance.CopyToClipboardAsync();
        resultEmpty.Should().BeNull();
    }

    [Test]
    public void CustomFormatters_FormatsCustomValues()
    {
        var comp = RenderGrid(
            selectionMode: DataGridSelectionMode.CellRange,
            clipboardEnabled: true,
            initialRange: new DataGridCellRange(0, 2, 0, 2));
        var grid = comp.Instance;

        var tsv = grid.ClipboardService.GenerateClipboardText(new DataGridClipboardOptions
        {
            IncludeHeaders = false,
            CustomFormatters = new Dictionary<string, System.Func<object?, string?>>
            {
                ["Price"] = val => $"${val:F2}"
            }
        });

        tsv.Should().Be("$19.99");
    }

    private IRenderedComponent<VekDataGrid<Item>> RenderGrid(
        DataGridSelectionMode selectionMode = DataGridSelectionMode.Row,
        bool multiSelection = false,
        bool clipboardEnabled = false,
        DataGridCellRange? initialRange = null)
    {
        return Context.Render<VekDataGrid<Item>>(parameters => parameters
            .Add(g => g.Items, SampleItems)
            .Add(g => g.SelectionMode, selectionMode)
            .Add(g => g.MultiSelection, multiSelection)
            .Add(g => g.ClipboardCopyEnabled, clipboardEnabled)
            .Add(g => g.SelectedCellRange, initialRange)
            .Add(g => g.Columns, (RenderFragment)(builder =>
            {
                builder.OpenComponent<PropertyColumn<Item, string>>(0);
                builder.AddAttribute(1, nameof(PropertyColumn<Item, string>.Property), (System.Linq.Expressions.Expression<System.Func<Item, string>>)(x => x.Name));
                builder.AddAttribute(2, nameof(PropertyColumn<Item, string>.Title), "Name");
                builder.CloseComponent();

                builder.OpenComponent<PropertyColumn<Item, int>>(3);
                builder.AddAttribute(4, nameof(PropertyColumn<Item, int>.Property), (System.Linq.Expressions.Expression<System.Func<Item, int>>)(x => x.Quantity));
                builder.AddAttribute(5, nameof(PropertyColumn<Item, int>.Title), "Quantity");
                builder.CloseComponent();

                builder.OpenComponent<PropertyColumn<Item, decimal>>(6);
                builder.AddAttribute(7, nameof(PropertyColumn<Item, decimal>.Property), (System.Linq.Expressions.Expression<System.Func<Item, decimal>>)(x => x.Price));
                builder.AddAttribute(8, nameof(PropertyColumn<Item, decimal>.Title), "Price");
                builder.CloseComponent();
            })));
    }
}
