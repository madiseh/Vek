using Microsoft.Extensions.DependencyInjection;

namespace Vek.Demo.Tests;

public sealed class DemoPageNavigationTests : BunitContext
{
    public DemoPageNavigationTests()
    {
        Services.AddSingleton<IDemoProductProvider>(new TestProductProvider([]));
    }

    /// <summary>
    /// Verifies that the landing page links to the getting started guide and the Vek repository.
    /// </summary>
    [Fact]
    public void LandingPageLinksToDocumentationAndGithub()
    {
        var cut = Render<Routes>();

        Assert.Equal("/docs/getting-started", cut.Find("a.get-started-link").GetAttribute("href"));
        Assert.Equal(
            "https://github.com/madiseh/Vek",
            cut.Find("a.github-link").GetAttribute("href"));
    }

    /// <summary>
    /// Verifies that the getting started documentation route renders its own page heading.
    /// </summary>
    [Fact]
    public void GettingStartedRouteShowsItsPageHeading()
    {
        Services.GetRequiredService<NavigationManager>().NavigateTo("/docs/getting-started");

        var cut = Render<Routes>();

        Assert.Equal("Getting Started", cut.Find("main h1").TextContent.Trim());
    }

    /// <summary>
    /// Verifies that the Components navigation group starts collapsed until the user opens it.
    /// </summary>
    [Fact]
    public void ComponentsNavigationStartsCollapsed()
    {
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/docs/components/vekdatagrid/basic-usage");

        var cut = Render<Routes>();

        Assert.False(cut.Find(".docs-section").HasAttribute("open"));
    }

    /// <summary>
    /// Verifies that the VekDataGrid guide renders sample data, markup, and working page links.
    /// </summary>
    [Fact]
    public void DataGridBasicUsageRouteShowsLiveTableAndExample()
    {
        Services.AddSingleton<IDemoProductProvider>(new TestProductProvider(
        [
            new DemoProduct(1, "Desk Lamp", "Lighting", 49.99m),
            new DemoProduct(2, "Task Chair", "Furniture", 199.00m)
        ]));
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/docs/components/vekdatagrid/basic-usage");

        var cut = Render<Routes>();

        Assert.Equal("VekDataGrid: Basic Usage", cut.Find("main h1").TextContent.Trim());
        Assert.Contains("Desk Lamp", cut.Find("tbody").TextContent);
        var razorExample = cut.Find(".code-sample pre code").TextContent;
        Assert.Contains("<VekDataGrid", razorExample);
        Assert.Contains("<TemplateColumn", razorExample);
        Assert.DoesNotContain("GetProductsAsync", razorExample);
        Assert.Equal(
            "/docs/components/vekdatagrid/basic-usage",
            cut.Find(".docs-sidebar a.active").GetAttribute("href"));
        Assert.Equal(
            "/docs/components/vekdatagrid/basic-usage#live-example",
            cut.Find(".docs-toc a").GetAttribute("href"));
    }

    /// <summary>
    /// Verifies that the sorting guide shows a live multi-column grid and its sample markup.
    /// </summary>
    [Fact]
    public void DataGridSortingRouteShowsLiveGridAndExample()
    {
        Services.AddSingleton<IDemoProductProvider>(new TestProductProvider(
        [
            new DemoProduct(1, "Desk Lamp", "Furniture", 49.99m),
            new DemoProduct(2, "Task Chair", "Furniture", 199.00m)
        ]));
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/docs/components/vekdatagrid/sorting");

        var cut = Render<Routes>();

        Assert.Equal("VekDataGrid: Sorting", cut.Find("main h1").TextContent.Trim());
        Assert.Contains("Desk Lamp", cut.Find("tbody").TextContent);
        Assert.Contains("SortMode=\"VekDataGridSortMode.Multiple\"", cut.Find(".code-sample pre code").TextContent);
        Assert.Equal(
            "/docs/components/vekdatagrid/sorting",
            cut.Find(".docs-sidebar a.active").GetAttribute("href"));
    }

    /// <summary>
    /// Verifies the filtering guide shows a live grid, filter modes, and the Vek API example.
    /// </summary>
    [Fact]
    public void DataGridFilteringRouteShowsLiveGridAndExample()
    {
        Services.AddSingleton<IDemoProductProvider>(new TestProductProvider(
        [
            new DemoProduct(1, "Desk Lamp", "Furniture", 49.99m),
            new DemoProduct(2, "Task Chair", "Furniture", 199.00m)
        ]));
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/docs/components/vekdatagrid/filtering");

        var cut = Render<Routes>();

        Assert.Equal("VekDataGrid: Filtering", cut.Find("main h1").TextContent.Trim());
        Assert.Contains("Desk Lamp", cut.Find("tbody").TextContent);
        Assert.Contains("FilterMode=\"VekDataGridFilterMode.ColumnRow\"", cut.Find(".code-sample pre code").TextContent);
        Assert.Equal(
            "/docs/components/vekdatagrid/filtering",
            cut.Find(".docs-sidebar a.active").GetAttribute("href"));
    }

    /// <summary>
    /// Verifies the pagination guide displays a live grid and custom pager sample.
    /// </summary>
    [Fact]
    public void DataGridPaginationRouteShowsLiveGridAndExample()
    {
        Services.AddSingleton<IDemoProductProvider>(new TestProductProvider(
        [
            new DemoProduct(1, "Desk Lamp", "Furniture", 49.99m),
            new DemoProduct(2, "Task Chair", "Furniture", 199.00m)
        ]));
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/docs/components/vekdatagrid/pagination");

        var cut = Render<Routes>();

        Assert.Equal("VekDataGrid: Pagination", cut.Find("main h1").TextContent.Trim());
        Assert.Contains("Desk Lamp", cut.Find("tbody").TextContent);
        Assert.Contains("<PagerContent Context=\"pager\">", cut.Find(".code-sample pre code").TextContent);
        Assert.Equal(
            "/docs/components/vekdatagrid/pagination",
            cut.Find(".docs-sidebar a.active").GetAttribute("href"));
    }

    /// <summary>
    /// Verifies the selection guide renders a live selection column and its binding example.
    /// </summary>
    [Fact]
    public void DataGridSelectionRouteShowsLiveGridAndExample()
    {
        Services.AddSingleton<IDemoProductProvider>(new TestProductProvider(
        [
            new DemoProduct(1, "Desk Lamp", "Furniture", 49.99m),
            new DemoProduct(2, "Task Chair", "Furniture", 199.00m)
        ]));
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/docs/components/vekdatagrid/selection");

        var cut = Render<Routes>();

        Assert.Equal("VekDataGrid: Row Selection", cut.Find("main h1").TextContent.Trim());
        Assert.Contains("Desk Lamp", cut.Find("tbody").TextContent);
        Assert.Contains("SelectionColumn", cut.Find(".code-sample pre code").TextContent);
        Assert.Equal(
            "/docs/components/vekdatagrid/selection",
            cut.Find(".docs-sidebar a.active").GetAttribute("href"));
    }

    /// <summary>
    /// Verifies the editing guide renders draft controls and a validation-aware commit example.
    /// </summary>
    [Fact]
    public void DataGridEditingRouteShowsLiveGridAndExample()
    {
        Services.AddSingleton<IDemoProductProvider>(new TestProductProvider(
        [
            new DemoProduct(1, "Desk Lamp", "Furniture", 49.99m),
            new DemoProduct(2, "Task Chair", "Furniture", 199.00m)
        ]));
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/docs/components/vekdatagrid/editing");

        var cut = Render<Routes>();

        Assert.Equal("VekDataGrid: Editing and Validation", cut.Find("main h1").TextContent.Trim());
        Assert.Contains("Desk Lamp", cut.Find("tbody").TextContent);
        Assert.Contains("EditTemplate", cut.Find(".code-sample pre code").TextContent);
        Assert.Equal(
            "/docs/components/vekdatagrid/editing",
            cut.Find(".docs-sidebar a.active").GetAttribute("href"));
    }

    /// <summary>
    /// Verifies the grouping guide shows nested group headers and its grouping definitions.
    /// </summary>
    [Fact]
    public void DataGridGroupingRouteShowsLiveGridAndExample()
    {
        Services.AddSingleton<IDemoProductProvider>(new TestProductProvider(
        [
            new DemoProduct(1, "Desk Lamp", "Furniture", 49.99m),
            new DemoProduct(2, "Task Chair", "Furniture", 199.00m),
            new DemoProduct(3, "Notebook Set", "Stationery", 12.90m)
        ]));
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/docs/components/vekdatagrid/grouping");

        var cut = Render<Routes>();

        Assert.Equal("VekDataGrid: Grouping", cut.Find("main h1").TextContent.Trim());
        Assert.Contains("Furniture", cut.Find("tbody").TextContent);
        Assert.Contains("<GroupTemplate Context=\"group\">", cut.Find(".code-sample pre code").TextContent);
        Assert.Equal(
            "/docs/components/vekdatagrid/grouping",
            cut.Find(".docs-sidebar a.active").GetAttribute("href"));
    }

    /// <summary>
    /// Verifies the aggregation guide displays group summaries and its aggregate configuration.
    /// </summary>
    [Fact]
    public void DataGridAggregationRouteShowsLiveGridAndExample()
    {
        Services.AddSingleton<IDemoProductProvider>(new TestProductProvider(
        [
            new DemoProduct(1, "Desk Lamp", "Furniture", 49.99m),
            new DemoProduct(2, "Task Chair", "Furniture", 199.00m),
            new DemoProduct(3, "Notebook Set", "Stationery", 12.90m)
        ]));
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/docs/components/vekdatagrid/aggregation");

        var cut = Render<Routes>();

        Assert.Equal("VekDataGrid: Aggregation", cut.Find("main h1").TextContent.Trim());
        Assert.Contains("Furniture", cut.Find("tbody").TextContent);
        Assert.Contains("Aggregates=\"_aggregates\"", cut.Find(".code-sample pre code").TextContent);
        Assert.Contains("Sum:", cut.Find("tfoot").TextContent);
    }

    /// <summary>
    /// Verifies the hierarchy guide renders expandable product details and its detail template.
    /// </summary>
    [Fact]
    public void DataGridHierarchyRouteShowsLiveGridAndExample()
    {
        Services.AddSingleton<IDemoProductProvider>(new TestProductProvider(
        [
            new DemoProduct(1, "Desk Lamp", "Furniture", 49.99m),
            new DemoProduct(2, "Task Chair", "Furniture", 199.00m)
        ]));
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/docs/components/vekdatagrid/hierarchy");

        var cut = Render<Routes>();

        Assert.Equal("VekDataGrid: Hierarchical Rows", cut.Find("main h1").TextContent.Trim());
        Assert.Contains("Desk Lamp", cut.Find("tbody").TextContent);
        Assert.Contains("<HierarchyColumn", cut.Find(".code-sample pre code").TextContent);
        Assert.Contains("aria-label=\"Toggle all row details\"", cut.Markup);
    }

    /// <summary>
    /// Verifies the server data guide loads a page from the provider and exposes its source example.
    /// </summary>
    [Fact]
    public void DataGridServerDataRouteShowsLiveGridAndExample()
    {
        Services.AddSingleton<IDemoProductProvider>(new TestProductProvider(
        [
            new DemoProduct(1, "Desk Lamp", "Furniture", 49.99m),
            new DemoProduct(2, "Task Chair", "Furniture", 199.00m)
        ]));
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/docs/components/vekdatagrid/server-data");

        var cut = Render<Routes>();

        Assert.Equal("VekDataGrid: Server Data", cut.Find("main h1").TextContent.Trim());
        Assert.Contains("Desk Lamp", cut.Find("tbody").TextContent);
        Assert.Contains("ServerData=\"LoadServerPageAsync\"", cut.Find(".code-sample pre code").TextContent);
    }

    /// <summary>
    /// Verifies the virtualization guide shows its large sample and both client and server range examples.
    /// </summary>
    [Fact]
    public void DataGridVirtualizationRouteShowsLargeSampleAndExamples()
    {
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/docs/components/vekdatagrid/virtualization");

        var cut = Render<Routes>();

        Assert.Equal("VekDataGrid: Virtualization", cut.Find("main h1").TextContent.Trim());
        Assert.Contains("VirtualizeRows=\"true\"", cut.Find(".code-sample pre code").TextContent);
        Assert.Contains("ServerVirtualData=\"LoadVirtualRangeAsync\"", cut.Find(".code-sample pre code").TextContent);
    }

    /// <summary>
    /// Verifies that the code sample switches from Razor markup to its C# code-behind.
    /// </summary>
    [Fact]
    public void CodeSampleSwitchesBetweenRazorAndCodeBehind()
    {
        Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/docs/components/vekdatagrid/basic-usage");

        var cut = Render<Routes>();

        Assert.Equal("Sample.razor", cut.Find("button[data-sample='razor']").TextContent.Trim());
        Assert.Equal("Sample.razor.cs", cut.Find("button[data-sample='code-behind']").TextContent.Trim());

        cut.Find("button[data-sample='code-behind']").Click();

        var codeBehindExample = cut.Find(".code-sample pre code").TextContent;
        Assert.Contains("IDemoProductProvider", codeBehindExample);
        Assert.Contains("GetProductsAsync", codeBehindExample);
        Assert.DoesNotContain("<VekDataGrid", codeBehindExample);
    }

    private sealed class TestProductProvider(IReadOnlyList<DemoProduct> products) : IDemoProductProvider
    {
        public Task<IReadOnlyList<DemoProduct>> GetProductsAsync(
            CancellationToken cancellationToken = default) => Task.FromResult(products);
    }
}
