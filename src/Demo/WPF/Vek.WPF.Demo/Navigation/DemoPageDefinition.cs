using Vek.WPF.Demo.Components.Pages;
using Vek.WPF.Demo.Components.Pages.Docs.WPF;
using Vek.WPF.Demo.Components.Pages.Docs.WPF.CodeView;
using AggregationPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Aggregation.Aggregation;
using BasicUsagePage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.BasicUsage.BasicUsage;
using ColumnLayoutPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.ColumnLayout.ColumnLayout;
using ColumnManagementPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.ColumnManagement.ColumnManagement;
using ContextActionsPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.ContextActions.ContextActions;
using EditingPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Editing.Editing;
using FilteringPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Filtering.Filtering;
using GroupingPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Grouping.Grouping;
using HierarchyPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Hierarchy.Hierarchy;
using ObservabilityPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Observability.Observability;
using PaginationPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Pagination.Pagination;
using PresentationPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Presentation.Presentation;
using SelectionPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Selection.Selection;
using ServerDataPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.ServerData.ServerData;
using SortingPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Sorting.Sorting;
using VirtualizationPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.DataGrid.Virtualization.Virtualization;
using CodeViewPage = Vek.WPF.Demo.Components.Pages.Docs.WPF.CodeView.CodeView;

namespace Vek.WPF.Demo.Navigation;

public enum DemoPageImplementationStatus
{
    InDevelopment,
    Implemented
}

public sealed record DemoPageDefinition(
    string Id,
    string Title,
    string Section,
    string Description,
    DemoPageImplementationStatus ImplementationStatus)
{
    public Type PageType => DemoPageTypes.For(Id);
}

internal static class DemoPageTypes
{
    private static readonly IReadOnlyDictionary<string, Type> Types = new Dictionary<string, Type>
    {
        ["home"] = typeof(Home),
        ["getting-started"] = typeof(GettingStarted),
        ["vekdatagrid/basic-usage"] = typeof(BasicUsagePage),
        ["vekdatagrid/sorting"] = typeof(SortingPage),
        ["vekdatagrid/filtering"] = typeof(FilteringPage),
        ["vekdatagrid/pagination"] = typeof(PaginationPage),
        ["vekdatagrid/selection"] = typeof(SelectionPage),
        ["vekdatagrid/editing"] = typeof(EditingPage),
        ["vekdatagrid/grouping"] = typeof(GroupingPage),
        ["vekdatagrid/aggregation"] = typeof(AggregationPage),
        ["vekdatagrid/hierarchy"] = typeof(HierarchyPage),
        ["vekdatagrid/server-data"] = typeof(ServerDataPage),
        ["vekdatagrid/virtualization"] = typeof(VirtualizationPage),
        ["vekdatagrid/column-management"] = typeof(ColumnManagementPage),
        ["vekdatagrid/column-layout"] = typeof(ColumnLayoutPage),
        ["vekdatagrid/presentation"] = typeof(PresentationPage),
        ["vekdatagrid/context-actions"] = typeof(ContextActionsPage),
        ["vekdatagrid/observability"] = typeof(ObservabilityPage),
        ["vekcodeview/basic-usage"] = typeof(CodeViewPage),
        ["vekcodeview/multi-sample"] = typeof(MultiSample),
    };

    public static Type For(string pageId) => Types.TryGetValue(pageId, out var pageType)
        ? pageType
        : throw new ArgumentOutOfRangeException(nameof(pageId), pageId, "No WPF page is registered for this ID.");
}
