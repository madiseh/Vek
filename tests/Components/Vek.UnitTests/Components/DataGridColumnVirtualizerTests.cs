#nullable enable
using AwesomeAssertions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Vek.UnitTests.Components;

[TestFixture]
public sealed class DataGridColumnVirtualizerTests : BunitTest
{
    /// <summary>
    /// Keeps the initial viewport and spacer widths aligned as horizontal scrolling moves the rendered column window.
    /// </summary>
    [Test]
    public async Task ScrollEvent_UpdatesRenderedColumnWindowAndSpacerWidths()
    {
        var listenerFactory = new TestScrollListenerFactory(new ScrollEventArgs { ClientWidth = 250 });
        Context.Services.AddSingleton<IScrollListenerFactory>(listenerFactory);

        var grid = RenderGrid(virtualizeColumns: true, columnOverscanCount: 0, columnCount: 8);

        await grid.WaitForAssertionAsync(() =>
        {
            grid.FindAll("thead th:not(.vek-datagrid-column-virtualizer-spacer)")
                .Select(header => header.TextContent.Trim())
                .ToArray()
                .Should().Equal("Field 01", "Field 02", "Field 03");
            grid.FindAll("tbody tr:first-child td.vek-datagrid-column-virtualizer-spacer")
                .Select(cell => cell.GetAttribute("style"))
                .Should().Contain(style => style!.Contains("500px", StringComparison.Ordinal));
        });

        listenerFactory.CreatedSelector.Should().Be($"#{grid.Find(".vek-table-container").GetAttribute("id")}");
        listenerFactory.CreatedReportRateMs.Should().Be(16);

        listenerFactory.Listener.RaiseScroll(new ScrollEventArgs { ClientWidth = 250, ScrollLeft = 300 });

        await grid.WaitForAssertionAsync(() =>
        {
            grid.FindAll("thead th:not(.vek-datagrid-column-virtualizer-spacer)")
                .Select(header => header.TextContent.Trim())
                .ToArray()
                .Should().Equal("Field 04", "Field 05", "Field 06");
            grid.FindAll("tbody tr:first-child td.vek-datagrid-column-virtualizer-spacer")
                .Select(cell => cell.GetAttribute("style"))
                .Should().Contain(style => style!.Contains("300px", StringComparison.Ordinal));
        });
    }

    /// <summary>
    /// Leaves all columns rendered when the horizontal viewport can contain the whole grid.
    /// </summary>
    [Test]
    public async Task ScrollEvent_RendersEveryColumnWhenAllColumnsFit()
    {
        var listenerFactory = new TestScrollListenerFactory(new ScrollEventArgs { ClientWidth = 1000 });
        Context.Services.AddSingleton<IScrollListenerFactory>(listenerFactory);

        var grid = RenderGrid(virtualizeColumns: true, columnOverscanCount: 0, columnCount: 8);

        await grid.WaitForAssertionAsync(() =>
        {
            grid.FindAll("thead th:not(.vek-datagrid-column-virtualizer-spacer)").Count.Should().Be(8);
            grid.FindAll(".vek-datagrid-column-virtualizer-spacer").Should().BeEmpty();
        });
    }

    /// <summary>
    /// Renders no column cells or spacers when every registered column is hidden.
    /// </summary>
    [Test]
    public async Task ScrollEvent_RendersNoColumnsWhenEveryColumnIsHidden()
    {
        var listenerFactory = new TestScrollListenerFactory(new ScrollEventArgs { ClientWidth = 250 });
        Context.Services.AddSingleton<IScrollListenerFactory>(listenerFactory);

        var grid = Context.Render<VekDataGrid<ColumnVirtualizerItem>>(parameters => parameters
            .Add(component => component.Items, [new ColumnVirtualizerItem("row")])
            .Add(component => component.VirtualizeColumns, true)
            .Add(component => component.ColumnOverscanCount, 0)
            .Add(component => component.Columns, RenderColumns(1, hidden: true)));

        await grid.WaitForAssertionAsync(() =>
        {
            grid.FindAll("thead th:not(.vek-datagrid-column-virtualizer-spacer)").Should().BeEmpty();
            grid.FindAll(".vek-datagrid-column-virtualizer-spacer").Should().BeEmpty();
        });
    }

    /// <summary>
    /// Keeps pinned columns in the rendered row while the scroll window advances across unpinned columns.
    /// </summary>
    [Test]
    public async Task ScrollEvent_KeepsPinnedColumnsRenderedOutsideTheWindow()
    {
        var listenerFactory = new TestScrollListenerFactory(new ScrollEventArgs { ClientWidth = 200 });
        Context.Services.AddSingleton<IScrollListenerFactory>(listenerFactory);

        var grid = Context.Render<VekDataGrid<ColumnVirtualizerItem>>(parameters => parameters
            .Add(component => component.Items, [new ColumnVirtualizerItem("row")])
            .Add(component => component.VirtualizeColumns, true)
            .Add(component => component.ColumnOverscanCount, 0)
            .Add(component => component.Columns, RenderColumns(8, pinFirstColumn: true)));

        await grid.WaitForAssertionAsync(() =>
            grid.FindAll("thead th:not(.vek-datagrid-column-virtualizer-spacer)")
                .Select(header => header.TextContent.Trim())
                .ToArray()
                .Should().Equal("Field 01", "Field 02"));

        listenerFactory.Listener.RaiseScroll(new ScrollEventArgs { ClientWidth = 200, ScrollLeft = 400 });

        await grid.WaitForAssertionAsync(() =>
            grid.FindAll("thead th:not(.vek-datagrid-column-virtualizer-spacer)")
                .Select(header => header.TextContent.Trim())
                .ToArray()
                .Should().Equal("Field 01", "Field 05", "Field 06"));
    }

    private IRenderedComponent<VekDataGrid<ColumnVirtualizerItem>> RenderGrid(bool virtualizeColumns, int columnOverscanCount, int columnCount)
    {
        return Context.Render<VekDataGrid<ColumnVirtualizerItem>>(parameters => parameters
            .Add(component => component.Items, [new ColumnVirtualizerItem("row")])
            .Add(component => component.VirtualizeColumns, virtualizeColumns)
            .Add(component => component.ColumnOverscanCount, columnOverscanCount)
            .Add(component => component.Columns, RenderColumns(columnCount)));
    }

    private static RenderFragment RenderColumns(int columnCount, bool hidden = false, bool pinFirstColumn = false) => builder =>
    {
        for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
        {
            var index = columnIndex;
            var title = $"Field {index + 1:D2}";
            builder.OpenComponent<TemplateColumn<ColumnVirtualizerItem>>(0);
            builder.AddAttribute(1, "Title", title);
            builder.AddAttribute(2, "Width", "100px");
            builder.AddAttribute(3, "Hidden", hidden);
            if (pinFirstColumn && columnIndex == 0)
            {
                builder.AddAttribute(4, "FixedStyle", DataGridFixedStyle.Left);
            }

            builder.AddAttribute(5, "CellTemplate", (RenderFragment<CellContext<ColumnVirtualizerItem>>)(context => cellBuilder =>
                cellBuilder.AddContent(0, $"{title}: {context.Item.Name}")));
            builder.CloseComponent();
        }
    };

    private sealed record ColumnVirtualizerItem(string Name);

    private sealed class TestScrollListenerFactory(ScrollEventArgs currentScrollData) : IScrollListenerFactory
    {
        public TestScrollListener Listener { get; } = new(currentScrollData);

        public string? CreatedSelector { get; private set; }

        public int? CreatedReportRateMs { get; private set; }

        public IScrollListener Create(string? selector)
        {
            CreatedSelector = selector;
            return Listener;
        }

        public IScrollListener Create(string? selector, int reportRateMs)
        {
            CreatedSelector = selector;
            CreatedReportRateMs = reportRateMs;
            return Listener;
        }
    }

    private sealed class TestScrollListener(ScrollEventArgs currentScrollData) : IScrollListener
    {
        private ScrollEventArgs _currentScrollData = currentScrollData;

        public string? Selector { get; set; }

        public int ReportRateMs { get; set; }

        public event EventHandler<ScrollEventArgs>? OnScroll;

        public ValueTask<ScrollEventArgs> GetCurrentScrollDataAsync() => ValueTask.FromResult(_currentScrollData);

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public void RaiseScroll(ScrollEventArgs args)
        {
            _currentScrollData = args;
            OnScroll?.Invoke(this, args);
        }
    }
}
