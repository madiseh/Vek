using Bunit;
using Microsoft.AspNetCore.Components;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class HierarchyTests : BunitContext
{
    /// <summary>Verifies row details expand and collapse from the row control.</summary>
    [Fact]
    public void RowDetailExpandsAndCollapses()
    {
        var cut = RenderHierarchyGrid();

        cut.Find("tbody tr:first-child button[aria-label='Expand details for Task Chair']").Click();
        Assert.Contains("Details for Task Chair", cut.Find("tbody").TextContent);

        cut.Find("tbody tr:first-child button[aria-label='Collapse details for Task Chair']").Click();
        Assert.DoesNotContain("Details for Task Chair", cut.Find("tbody").TextContent);
    }

    /// <summary>Verifies rows matching the initial expansion predicate start expanded.</summary>
    [Fact]
    public void InitialExpansionPredicateIsApplied()
    {
        var cut = RenderHierarchyGrid(initiallyExpanded: product => product.Category == "Furniture");

        Assert.Contains("Details for Task Chair", cut.Find("tbody").TextContent);
        Assert.Contains("Details for Desk Lamp", cut.Find("tbody").TextContent);
        Assert.DoesNotContain("Details for Notebook Set", cut.Find("tbody").TextContent);
    }

    /// <summary>Verifies expanding a row closes the previously expanded row in single mode.</summary>
    [Fact]
    public void SingleExpansionClosesPreviousRow()
    {
        var cut = RenderHierarchyGrid(singleExpanded: true);

        cut.Find("button[aria-label='Expand details for Task Chair']").Click();
        cut.Find("button[aria-label='Expand details for Desk Lamp']").Click();

        Assert.DoesNotContain("Details for Task Chair", cut.Find("tbody").TextContent);
        Assert.Contains("Details for Desk Lamp", cut.Find("tbody").TextContent);
    }

    /// <summary>Verifies the hierarchy header control expands and collapses every current row.</summary>
    [Fact]
    public void HeaderToggleControlsAllRows()
    {
        var cut = RenderHierarchyGrid();

        cut.Find("thead button[aria-label='Toggle all row details']").Click();
        Assert.Equal(3, cut.FindAll("tr.detail-row").Count);

        cut.Find("thead button[aria-label='Toggle all row details']").Click();
        Assert.Empty(cut.FindAll("tr.detail-row"));
    }

    /// <summary>Verifies expanded details stay attached to matching rows after an item refresh.</summary>
    [Fact]
    public void ExpandedStateSurvivesItemsRefresh()
    {
        var products = DataGridTestHelpers.Products.ToArray();
        var comparer = EqualityComparer<TestProduct>.Create((left, right) => left?.Name == right?.Name, product => product.Name.GetHashCode());
        var cut = RenderHierarchyGrid(items: products, comparer: comparer);
        cut.Find("button[aria-label='Expand details for Desk Lamp']").Click();

        products = products.Select(product => product with { }).ToArray();
        cut.Render(parameters => parameters.Add(grid => grid.Items, products));

        Assert.Contains("Details for Desk Lamp", cut.Find("tbody").TextContent);
    }

    private IRenderedComponent<VekDataGrid<TestProduct>> RenderHierarchyGrid(
        Func<TestProduct, bool>? initiallyExpanded = null,
        bool singleExpanded = false,
        TestProduct[]? items = null,
        IEqualityComparer<TestProduct>? comparer = null) => Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, items ?? DataGridTestHelpers.Products)
            .Add(grid => grid.PageSize, 10)
            .Add(grid => grid.InitiallyExpanded, initiallyExpanded)
            .Add(grid => grid.SingleExpandedRow, singleExpanded)
            .Add(grid => grid.RowDetailsComparer, comparer)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<HierarchyColumn<TestProduct>>(0);
                builder.AddAttribute(1, nameof(HierarchyColumn<TestProduct>.DetailTemplate),
                    (RenderFragment<VekDataGridRowDetailContext<TestProduct>>)(context => content =>
                        content.AddContent(0, $"Details for {context.Item.Name}")));
                builder.CloseComponent();
                builder.OpenComponent<PropertyColumn<TestProduct, string>>(2);
                builder.AddAttribute(3, nameof(PropertyColumn<TestProduct, string>.Property), (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Name));
                builder.CloseComponent();
            }));
}
