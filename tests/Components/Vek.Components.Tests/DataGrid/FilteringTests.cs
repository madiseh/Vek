using Bunit;
using Microsoft.AspNetCore.Components;
using Vek.Components.Components.DataGrid;

namespace Vek.Components.Tests.DataGrid;

public sealed class FilteringTests : BunitContext
{
    /// <summary>
    /// Verifies that quick filtering removes rows that do not match its predicate.
    /// </summary>
    [Fact]
    public void QuickFilterMatchesAcrossRows()
    {
        var cut = DataGridTestHelpers.RenderQuickFilterGrid(
            this,
            product => product.Name.StartsWith("Notebook", StringComparison.Ordinal));

        Assert.Equal(new[] { "Notebook Set" }, DataGridTestHelpers.GetRowNames(cut));
    }

    /// <summary>Verifies column definitions apply comparisons to their selected values.</summary>
    [Fact]
    public async Task ColumnFilterModesApplyPredicates()
    {
        var cut = RenderFilterGrid();
        await cut.InvokeAsync(() => cut.Instance.ApplyFilterAsync("Name", VekDataGridFilterOperator.Contains, "Desk"));
        Assert.Equal(new[] { "Desk Lamp" }, DataGridTestHelpers.GetRowNames(cut));
        Assert.Equal("Desk", cut.Instance.CurrentState.Filters.Single().Value);
    }

    /// <summary>Verifies text filters honor the configured case sensitivity.</summary>
    [Fact]
    public async Task StringCaseSensitivityIsConfigurable()
    {
        var cut = RenderFilterGrid(caseSensitive: true);
        await cut.InvokeAsync(() => cut.Instance.ApplyFilterAsync("Name", VekDataGridFilterOperator.Contains, "desk"));
        Assert.Equal(new[] { "No items to display." }, DataGridTestHelpers.GetRowNames(cut));
        await cut.InvokeAsync(() => cut.Instance.ApplyFilterAsync("Name", VekDataGridFilterOperator.Contains, "Desk"));
        Assert.Equal(new[] { "Desk Lamp" }, DataGridTestHelpers.GetRowNames(cut));
    }

    /// <summary>Verifies custom filter templates can apply and clear their column definition.</summary>
    [Fact]
    public void FilterTemplateAppliesAndClearsDefinition()
    {
        VekDataGridFilterContext? filterContext = null;
        var cut = RenderFilterGrid(filterTemplate: context => builder =>
        {
            filterContext = context;
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "onclick", EventCallback.Factory.Create(this, () => context.Apply("Notebook")));
            builder.AddContent(2, "Apply custom filter");
            builder.CloseElement();
        });

        cut.Find("button").Click();
        cut.WaitForAssertion(() => Assert.Equal(new[] { "Notebook Set" }, DataGridTestHelpers.GetRowNames(cut)));
        Assert.NotNull(filterContext);
        filterContext!.Clear();
        cut.WaitForAssertion(() => Assert.Equal(3, DataGridTestHelpers.GetRowNames(cut).Length));
    }

    /// <summary>Verifies applying a filter resets pagination to the first matching page.</summary>
    [Fact]
    public async Task ApplyingFilterResetsPage()
    {
        var cut = RenderFilterGrid(pageSize: 1);
        cut.Find(".pager button:last-child").Click();
        Assert.Contains("Page 2 of 3", cut.Markup);

        await cut.InvokeAsync(() => cut.Instance.ApplyFilterAsync("Name", VekDataGridFilterOperator.Contains, "Notebook"));

        Assert.Contains("Page 1 of 1", cut.Markup);
        Assert.Equal(new[] { "Notebook Set" }, DataGridTestHelpers.GetRowNames(cut));
    }

    /// <summary>Verifies each configured filter mode renders its corresponding filter surface.</summary>
    [Fact]
    public void FilterModesRenderTheirConfiguredControls()
    {
        var panel = RenderFilterGrid();
        Assert.Single(panel.FindAll(".filter-panel input"));

        var menu = RenderFilterGrid(filterMode: VekDataGridFilterMode.ColumnMenu);
        Assert.Single(menu.FindAll("thead details"));

        var row = RenderFilterGrid(filterMode: VekDataGridFilterMode.ColumnRow);
        Assert.Single(row.FindAll("thead .filter-row input"));
    }

    private IRenderedComponent<VekDataGrid<TestProduct>> RenderFilterGrid(
        int pageSize = 3,
        bool caseSensitive = false,
        VekDataGridFilterMode filterMode = VekDataGridFilterMode.Simple,
        RenderFragment<VekDataGridFilterContext>? filterTemplate = null) => Render<VekDataGrid<TestProduct>>(parameters => parameters
            .Add(grid => grid.Items, DataGridTestHelpers.Products)
            .Add(grid => grid.PageSize, pageSize)
            .Add(grid => grid.FilterCaseSensitive, caseSensitive)
            .Add(grid => grid.FilterMode, filterMode)
            .AddChildContent(builder =>
            {
                builder.OpenComponent<PropertyColumn<TestProduct, string>>(0);
                builder.AddAttribute(1, nameof(PropertyColumn<TestProduct, string>.Property), (System.Linq.Expressions.Expression<Func<TestProduct, string>>)(product => product.Name));
                builder.AddAttribute(2, nameof(PropertyColumn<TestProduct, string>.Key), "Name");
                if (filterTemplate is not null)
                    builder.AddAttribute(3, nameof(PropertyColumn<TestProduct, string>.FilterTemplate), filterTemplate);
                builder.CloseComponent();
            }));
}
