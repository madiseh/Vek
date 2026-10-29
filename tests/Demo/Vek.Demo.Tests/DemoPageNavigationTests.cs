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
        Assert.DoesNotContain("GetProductsAsync", razorExample);
        Assert.Equal(
            "/docs/components/vekdatagrid/basic-usage",
            cut.Find(".docs-sidebar a.active").GetAttribute("href"));
        Assert.Equal(
            "/docs/components/vekdatagrid/basic-usage#live-example",
            cut.Find(".docs-toc a").GetAttribute("href"));
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
