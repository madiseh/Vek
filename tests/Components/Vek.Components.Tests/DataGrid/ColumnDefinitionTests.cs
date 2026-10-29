using Bunit;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class ColumnDefinitionTests : BunitContext
{
    /// <summary>
    /// Verifies that property columns format values and use their configured title.
    /// </summary>
    [Fact]
    public void PropertyColumnUsesTitleAndFormat()
    {
        var cut = DataGridTestHelpers.RenderPriceGrid(this);

        Assert.Equal("Price", cut.Find("thead th").TextContent.Trim());
        Assert.Equal("199", cut.Find("tbody td").TextContent.Trim());
    }

    /// <summary>
    /// Verifies that property columns allow custom cell rendering.
    /// </summary>
    [Fact]
    public void PropertyColumnRendersCustomCellTemplate()
    {
        var cut = DataGridTestHelpers.RenderCustomPriceGrid(this);

        Assert.Equal("Price", cut.Find("thead th").TextContent.Trim());
        Assert.Equal("$199.00", cut.Find("tbody td").TextContent.Trim());
    }

    /// <summary>
    /// Verifies that template columns render custom cell content for each item.
    /// </summary>
    [Fact]
    public void TemplateColumnRendersCustomCellContent()
    {
        var cut = DataGridTestHelpers.RenderTemplateGrid(this);

        Assert.Equal("Product", cut.Find("thead th").TextContent.Trim());
        Assert.Equal("Item: Task Chair", cut.Find("tbody td").TextContent.Trim());
    }

}
