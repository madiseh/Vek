using Vek.WPF.Demo.Navigation;
using Vek.WPF.Demo.ViewModels;

namespace Vek.WPF.Demo.Tests;

public sealed class DemoPageCatalogTests
{
    /// <summary>
    /// Home's Get Started command opens Getting Started in the existing navigation state.
    /// </summary>
    [Fact]
    public void GetStartedNavigatesToGettingStarted()
    {
        var viewModel = new MainWindowViewModel();

        Assert.True(viewModel.GetStartedCommand.CanExecute(null));
        viewModel.GetStartedCommand.Execute(null);

        Assert.Equal("getting-started", viewModel.SelectedPage.Id);
    }

    /// <summary>
    /// Navigation exposes every current documentation topic once, in the shared documentation order.
    /// </summary>
    [Fact]
    public void PageCatalogContainsHomeAndEveryCurrentDocumentationTopic()
    {
        var expected = new (string Id, string Title, string Section)[]
        {
            ("home", "Home", "Overview"),
            ("getting-started", "Getting Started", "Overview"),
            ("vekdatagrid/basic-usage", "Basic Usage", "VekDataGrid"),
            ("vekdatagrid/sorting", "Sorting", "VekDataGrid"),
            ("vekdatagrid/filtering", "Filtering", "VekDataGrid"),
            ("vekdatagrid/pagination", "Pagination", "VekDataGrid"),
            ("vekdatagrid/selection", "Row Selection", "VekDataGrid"),
            ("vekdatagrid/editing", "Editing", "VekDataGrid"),
            ("vekdatagrid/grouping", "Grouping", "VekDataGrid"),
            ("vekdatagrid/aggregation", "Aggregation", "VekDataGrid"),
            ("vekdatagrid/hierarchy", "Hierarchy", "VekDataGrid"),
            ("vekdatagrid/server-data", "Server Data", "VekDataGrid"),
            ("vekdatagrid/virtualization", "Virtualization", "VekDataGrid"),
            ("vekdatagrid/column-management", "Column Management", "VekDataGrid"),
            ("vekdatagrid/column-layout", "Column Layout", "VekDataGrid"),
            ("vekdatagrid/presentation", "Presentation", "VekDataGrid"),
            ("vekdatagrid/context-actions", "Context Actions", "VekDataGrid"),
            ("vekdatagrid/observability", "Collection Observability", "VekDataGrid"),
            ("vekcodeview/basic-usage", "Basic Usage", "VekCodeView"),
            ("vekcodeview/multi-sample", "Multi-Sample", "VekCodeView")
        };

        Assert.Equal(expected, DemoPageCatalog.All.Select(page => (page.Id, page.Title, page.Section)));
        Assert.Equal(20, DemoPageCatalog.All.Select(page => page.Id).Distinct().Count());
        Assert.All(DemoPageCatalog.All, page => Assert.False(string.IsNullOrWhiteSpace(page.Description)));
    }

    /// <summary>
    /// The first visit opens Home while keeping every catalog page available for navigation.
    /// </summary>
    [Fact]
    public void HomeIsSelectedInitially()
    {
        var viewModel = new MainWindowViewModel();

        Assert.Equal("home", viewModel.SelectedPage.Id);
        Assert.Equal(DemoPageCatalog.All, viewModel.Pages);
    }

    /// <summary>
    /// Selecting a documentation entry updates the active page and notifies bound content once.
    /// </summary>
    [Fact]
    public void SelectingADocumentationPageChangesTheSelectedPage()
    {
        var viewModel = new MainWindowViewModel();
        var page = viewModel.Pages.Single(page => page.Id == "vekdatagrid/sorting");
        var notifications = new List<string?>();
        viewModel.PropertyChanged += (_, args) => notifications.Add(args.PropertyName);

        viewModel.SelectPage(page);
        viewModel.SelectedPage = page;

        Assert.Same(page, viewModel.SelectedPage);
        Assert.Equal(new[] { "SelectedPage" }, notifications);
    }

    /// <summary>
    /// DataGrid topics without a native behavior sample remain clearly marked in development.
    /// </summary>
    [Fact]
    public void UnimplementedDataGridTopicsAreMarkedInDevelopment()
    {
        var pages = DemoPageCatalog.All
            .Where(page => page.Section == "VekDataGrid" && page.Id != "vekdatagrid/basic-usage")
            .ToArray();

        Assert.Equal(15, pages.Length);
        Assert.All(pages, page => Assert.Equal(DemoPageImplementationStatus.InDevelopment, page.ImplementationStatus));
    }

    /// <summary>
    /// Basic Usage is the only DataGrid topic advertised as implemented while native features are added incrementally.
    /// </summary>
    [Fact]
    public void BasicUsageIsTheOnlyImplementedDataGridTopic()
    {
        var pages = DemoPageCatalog.All.Where(page => page.Section == "VekDataGrid").ToArray();

        Assert.Equal(DemoPageImplementationStatus.Implemented,
            pages.Single(page => page.Id == "vekdatagrid/basic-usage").ImplementationStatus);
        Assert.All(pages.Where(page => page.Id != "vekdatagrid/basic-usage"), page =>
            Assert.Equal(DemoPageImplementationStatus.InDevelopment, page.ImplementationStatus));
    }

    /// <summary>
    /// CodeView topics are marked implemented because the native read-only sample views are available.
    /// </summary>
    [Fact]
    public void CodeViewTopicsAreMarkedImplemented()
    {
        var pages = DemoPageCatalog.All.Where(page => page.Section == "VekCodeView").ToArray();

        Assert.Equal(2, pages.Length);
        Assert.All(pages, page => Assert.Equal(DemoPageImplementationStatus.Implemented, page.ImplementationStatus));
    }
}
