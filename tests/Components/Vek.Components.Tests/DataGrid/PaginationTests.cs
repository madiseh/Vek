using Bunit;
using Microsoft.AspNetCore.Components;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class PaginationTests : BunitContext
{
    /// <summary>Verifies the grid displays every supplied item when no page size is specified.</summary>
    [Fact]
    public void DefaultPageSizeDisplaysAllItems()
    {
        var items = Enumerable.Range(1, 12)
            .Select(index => new TestProduct($"Product {index}"))
            .ToArray();
        var cut = RenderPagingGrid(pageSize: null, items: items);

        Assert.Equal(items.Select(item => item.Name), DataGridTestHelpers.GetRowNames(cut));
        Assert.Empty(cut.FindAll(".pager"));
    }

    /// <summary>
    /// Verifies that the pager navigates forward and backward between result pages.
    /// </summary>
    [Fact]
    public void NextAndPreviousButtonsNavigateBetweenPages()
    {
        var cut = DataGridTestHelpers.RenderGrid(this, pageSize: 2);

        cut.WaitForAssertion(() =>
            Assert.Equal(
                new[] { "Task Chair", "Desk Lamp" },
                DataGridTestHelpers.GetRowNames(cut)));

        cut.Find(".pager button[aria-label='Next page']").Click();
        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                new[] { "Notebook Set" },
                DataGridTestHelpers.GetRowNames(cut));
            Assert.Contains("Page 2 of 2", cut.Markup);
        });

        cut.Find(".pager button[aria-label='Previous page']").Click();
        cut.WaitForAssertion(() =>
        {
            Assert.Equal(
                new[] { "Task Chair", "Desk Lamp" },
                DataGridTestHelpers.GetRowNames(cut));
            Assert.Contains("Page 1 of 2", cut.Markup);
        });
    }

    /// <summary>Verifies changing the rows-per-page selector updates the displayed page and total pages.</summary>
    [Fact]
    public void RowsPerPageCanChange()
    {
        var cut = RenderPagingGrid(pageSize: 2);

        cut.Find("select").Change("1");

        Assert.Equal(new[] { "Task Chair" }, DataGridTestHelpers.GetRowNames(cut));
        Assert.Contains("Page 1 of 3", cut.Markup);
    }

    /// <summary>Verifies custom pager content receives the matching state and item counts.</summary>
    [Fact]
    public void PagerContentReceivesCurrentState()
    {
        VekDataGridPagerContext<TestProduct>? pagerContext = null;
        var cut = RenderPagingGrid(pagerContent: context => builder =>
        {
            pagerContext = context;
            builder.AddContent(0, $"Page {context.State.Page}; {context.TotalItems} items");
        });

        Assert.Contains("Page 1; 3 items", cut.Markup);
        Assert.NotNull(pagerContext);
        Assert.Equal(3, pagerContext!.TotalItems);
    }

    /// <summary>Verifies the current page is clamped when an items refresh shortens the result set.</summary>
    [Fact]
    public void PageIsClampedWhenItemsShrink()
    {
        var reportedPage = 1;
        var cut = RenderPagingGrid(pageSize: 1, currentPageChanged: page => reportedPage = page);
        cut.Find(".pager button[aria-label='Next page']").Click();
        Assert.Contains("Page 2 of 3", cut.Markup);

        cut.Render(parameters => parameters.Add(grid => grid.Items, DataGridTestHelpers.Products.Take(1).ToArray()));

        Assert.Contains("Page 1 of 1", cut.Markup);
        Assert.Equal(new[] { "Task Chair" }, DataGridTestHelpers.GetRowNames(cut));
        Assert.Equal(1, reportedPage);
    }

    /// <summary>Verifies an empty collection safely displays one valid page with no rows.</summary>
    [Fact]
    public void EmptyItemsKeepOneValidPage()
    {
        var cut = RenderPagingGrid(items: []);

        Assert.Contains("Page 1 of 1", cut.Markup);
        Assert.Contains("No items to display.", cut.Markup);
        Assert.True(cut.Find(".pager button[aria-label='Previous page']").HasAttribute("disabled"));
        Assert.True(cut.Find(".pager button[aria-label='Next page']").HasAttribute("disabled"));
    }

    private IRenderedComponent<VekDataGrid<TestProduct>> RenderPagingGrid(
        int? pageSize = 2,
        TestProduct[]? items = null,
        RenderFragment<VekDataGridPagerContext<TestProduct>>? pagerContent = null,
        Action<int>? currentPageChanged = null) => Render<VekDataGrid<TestProduct>>(parameters =>
        {
            parameters.Add(grid => grid.Items, items ?? DataGridTestHelpers.Products);
            if (pageSize.HasValue)
            {
                parameters.Add(grid => grid.PageSize, pageSize.Value);
            }

            parameters
                .Add(grid => grid.PageSizeOptions, new[] { 1, 2, 3 })
                .Add(grid => grid.PagerContent, pagerContent)
                .Add(grid => grid.CurrentPageChanged, EventCallback.Factory.Create(this, currentPageChanged ?? (_ => { })))
                .AddChildContent(builder =>
            {
                builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
                builder.AddAttribute(1, nameof(PropertyColumn<TestProduct, string>.Property), (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Name));
                builder.CloseComponent();
            });
        });
}
