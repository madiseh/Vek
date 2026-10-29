using Bunit;
using Microsoft.AspNetCore.Components;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class GroupingTests : BunitContext
{
    /// <summary>Verifies rows are grouped by the selected property column.</summary>
    [Fact]
    public void RowsGroupBySelectedColumn()
    {
        var cut = RenderGroupGrid([new("Category")]);

        var groups = cut.FindAll("tr.group-header[data-group-level='0']").Select(row => row.TextContent).ToArray();

        Assert.Equal(2, groups.Length);
        Assert.Contains(groups, group => group.Contains("Furniture", StringComparison.Ordinal));
        Assert.Contains(groups, group => group.Contains("Stationery", StringComparison.Ordinal));
    }

    /// <summary>Verifies groups can be nested in the requested order and direction.</summary>
    [Fact]
    public void GroupsCanBeNestedAndOrdered()
    {
        var cut = RenderGroupGrid(
        [
            new("Category", VekDataGridGroupSortOrder.Descending),
            new("Name")
        ]);

        var topGroups = cut.FindAll("tr.group-header[data-group-level='0']").Select(row => row.TextContent).ToArray();
        Assert.Contains("Stationery", topGroups[0]);
        Assert.Contains("Furniture", topGroups[1]);
        Assert.Equal(3, cut.FindAll("tr.group-header[data-group-level='1']").Count);
    }

    /// <summary>Verifies a custom group template receives its column, value, level, and item count.</summary>
    [Fact]
    public void GroupTemplateReceivesDefinition()
    {
        VekDataGridGroupContext<TestProduct>? received = null;
        var cut = RenderGroupGrid([new("Category")], context => builder =>
        {
            received = context;
            builder.AddContent(0, $"Custom {context.Value} ({context.Count})");
        });

        Assert.Contains("Custom Furniture (2)", cut.Markup);
        Assert.NotNull(received);
        Assert.Equal("Category", received!.ColumnKey);
        Assert.Equal(0, received.Level);
    }

    /// <summary>Verifies group expansion controls reveal and hide their child rows.</summary>
    [Fact]
    public void GroupsCanExpandAndCollapse()
    {
        var cut = RenderGroupGrid([new("Category")]);
        Assert.Contains("Task Chair", cut.Find("tbody").TextContent);

        cut.Find("tr.group-header[data-group-level='0'] button").Click();

        Assert.DoesNotContain("Task Chair", cut.Find("tbody").TextContent);
        Assert.Equal("false", cut.Find("tr.group-header[data-group-level='0'] button").GetAttribute("aria-expanded"));
    }

    /// <summary>Verifies grouping is calculated over the visible page after paging and sorting.</summary>
    [Fact]
    public void GroupsAreScopedToTheCurrentPage()
    {
        var cut = RenderGroupGrid([new("Category")], pageSize: 1);
        Assert.Single(cut.FindAll("tr.group-header[data-group-level='0']"));

        cut.Find(".pager button[aria-label='Next page']").Click();

        Assert.Single(cut.FindAll("tr.group-header[data-group-level='0']"));
        Assert.Contains("Desk Lamp", cut.Find("tbody").TextContent);
    }

    private IRenderedComponent<VekDataGrid<TestProduct>> RenderGroupGrid(
        IReadOnlyList<VekDataGridGroupDefinition> groups,
        RenderFragment<VekDataGridGroupContext<TestProduct>>? groupTemplate = null,
        int pageSize = 10) => Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, DataGridTestHelpers.Products)
            .Add(grid => grid.PageSize, pageSize)
            .Add(grid => grid.GroupBy, groups)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
                builder.AddAttribute(1, nameof(PropertyColumn<TestProduct, string>.Property), (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Name));
                builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, string>.Key), "Name");
                builder.CloseComponent();
                builder.OpenComponent<PropertyColumn<TestProduct, string>>(3);
                builder.AddAttribute(4, nameof(PropertyColumn<TestProduct, string>.Property), (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Category));
                builder.AddAttribute(5, nameof(PropertyColumn<TestProduct, string>.Key), "Category");
                builder.AddAttribute(6, nameof(PropertyColumn<TestProduct, string>.GroupTemplate), groupTemplate);
                builder.CloseComponent();
            }));
}
