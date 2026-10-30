using Bunit;
using Microsoft.AspNetCore.Components;
using Vek.Blazor.DataGrid.Components.DataGrid;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class ContextActionsTests : BunitContext
{
    /// <summary>Verifies row and cell action templates receive the item, stable column key, and visible row index.</summary>
    [Fact]
    public void ContextActionReceivesRowAndCell()
    {
        var rows = new List<VekDataGridRowContext<TestProduct>>();
        var cells = new List<VekDataGridCellContext<TestProduct>>();
        var cut = Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, DataGridTestHelpers.Products)
            .Add(grid => grid.SelectionMode, VekDataGridSelectionMode.Single)
            .Add(grid => grid.RowActionsTemplate, (RenderFragment<VekDataGridRowContext<TestProduct>>)(context => builder =>
            {
                rows.Add(context);
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "class", "row-action");
                builder.AddAttribute(2, "onclick", EventCallback.Factory.Create(this, context.SelectAsync));
                builder.AddContent(3, $"Open {context.Item.Name}");
                builder.CloseElement();
            }))
            .Add(grid => grid.CellActionsTemplate, (RenderFragment<VekDataGridCellContext<TestProduct>>)(context => builder =>
            {
                cells.Add(context);
                builder.OpenElement(0, "button");
                builder.AddAttribute(1, "class", "cell-action");
                builder.AddContent(2, $"Inspect {context.ColumnKey}");
                builder.CloseElement();
            }))
            .AddChildContent(NameColumn()));

        Assert.Contains("Open Task Chair", cut.Find("button.row-action").TextContent);
        Assert.Contains("Inspect Name", cut.Find("button.cell-action").TextContent);
        Assert.Contains(rows, context => context.Item.Name == "Task Chair" && context.Index == 0);
        Assert.Contains(cells, context => context.Item.Name == "Task Chair" && context.ColumnKey == "Name" && context.RowIndex == 0);
        cut.Find("button.row-action").Click();
        Assert.Equal("Task Chair", cut.Instance.SelectedItem?.Name);
    }

    /// <summary>Verifies public commands clear state, update selection and details, and commit an edit.</summary>
    [Fact]
    public async Task PublicCommandsUpdateGridState()
    {
        var committed = new List<TestProduct>();
        var cut = Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, DataGridTestHelpers.Products)
            .Add(grid => grid.PageSize, 10)
            .Add(grid => grid.SelectionMode, VekDataGridSelectionMode.Multiple)
            .Add(grid => grid.EditMode, VekDataGridEditMode.Row)
            .Add(grid => grid.ItemCommitted, EventCallback.Factory.Create<TestProduct>(this, committed.Add))
            .AddChildContent(Columns()));

        cut.Find("button[aria-label='Sort by Name']").Click();
        await cut.Instance.ApplyFilterAsync("Name", VekDataGridFilterOperator.Contains, "Desk");
        Assert.Single(cut.FindAll("tbody tr"));

        await cut.Instance.ClearFiltersAsync();
        await cut.Instance.ClearSortAsync();
        Assert.Empty(cut.Instance.CurrentState.Filters);
        Assert.Empty(cut.Instance.CurrentState.Sorts);
        Assert.Contains(DataGridTestHelpers.Products[0].Name, cut.Find("tbody").TextContent);

        await cut.Instance.SelectAllAsync(true);
        Assert.Equal(3, cut.Instance.SelectedItems.Count);
        await cut.Instance.ExpandAllAsync();
        Assert.Equal(3, cut.FindAll("tr.detail-row").Count);
        await cut.Instance.CollapseAllAsync();
        Assert.Empty(cut.FindAll("tr.detail-row"));

        await cut.Instance.EditItemAsync(DataGridTestHelpers.Products[0]);
        Assert.Contains("Editing", cut.Find("tbody").TextContent);
        Assert.True(await cut.Instance.CommitEditAsync());
        Assert.Single(committed);
        await cut.Instance.EditItemAsync(DataGridTestHelpers.Products[1]);
        await cut.Instance.CancelEditAsync();
        Assert.DoesNotContain("Editing Task Chair", cut.Find("tbody").TextContent);

        var requests = 0;
        var serverGrid = Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.ServerData, (_, _) =>
            {
                requests++;
                return Task.FromResult(new VekDataGridData<TestProduct>(DataGridTestHelpers.Products, 3));
            })
            .AddChildContent(NameColumn()));
        await serverGrid.Instance.ReloadAsync();
        Assert.Equal(2, requests);
    }

    private static RenderFragment NameColumn() => builder =>
    {
        builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
        builder.AddAttribute(1, nameof(PropertyColumn<TestProduct, string>.Property),
            (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Name));
        builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, string>.Title), "Name");
        builder.AddAttribute(3, nameof(PropertyColumn<TestProduct, string>.Key), "Name");
        builder.CloseComponent();
    };

    private static RenderFragment Columns() => builder =>
    {
        builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
        builder.AddAttribute(1, nameof(PropertyColumn<TestProduct, string>.Property),
            (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Name));
        builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, string>.Title), "Name");
        builder.AddAttribute(3, nameof(PropertyColumn<TestProduct, string>.Key), "Name");
        builder.AddAttribute(4, nameof(PropertyColumn<TestProduct, string>.Editable), true);
        builder.AddAttribute(5, nameof(PropertyColumn<TestProduct, string>.EditTemplate),
            (RenderFragment<VekDataGridEditContext<TestProduct>>)(context => builder => builder.AddContent(0, $"Editing {context.Item.Name}")));
        builder.CloseComponent();
        builder.OpenComponent<HierarchyColumn<TestProduct>>(6);
        builder.AddAttribute(7, nameof(HierarchyColumn<TestProduct>.DetailTemplate),
            (RenderFragment<VekDataGridRowDetailContext<TestProduct>>)(context => builder => builder.AddContent(0, $"Details: {context.Item.Name}")));
        builder.CloseComponent();
    };
}
