using Vek.WPF.Demo.Navigation;
using Vek.WPF.Demo.ViewModels;

namespace Vek.WPF.Demo.Tests;

public sealed class DemoPageCatalogTests
{
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
    /// DataGrid topics do not claim implemented native demonstrations before those samples exist.
    /// </summary>
    [Fact]
    public void UnimplementedDataGridTopicsAreMarkedInDevelopment()
    {
        var pages = DemoPageCatalog.All.Where(page => page.Section == "VekDataGrid").ToArray();

        Assert.Equal(16, pages.Length);
        Assert.All(pages, page => Assert.Equal(DemoPageImplementationStatus.InDevelopment, page.ImplementationStatus));
    }
}
