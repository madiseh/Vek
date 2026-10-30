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
        ["home"] = typeof(Components.Pages.Home),
        ["getting-started"] = typeof(Components.Pages.Docs.WPF.GettingStarted),
        ["vekdatagrid/basic-usage"] = typeof(Components.Pages.Docs.WPF.DataGrid.BasicUsage.BasicUsage),
        ["vekdatagrid/sorting"] = typeof(Components.Pages.Docs.WPF.DataGrid.Sorting.Sorting),
        ["vekdatagrid/filtering"] = typeof(Components.Pages.Docs.WPF.DataGrid.Filtering.Filtering),
        ["vekdatagrid/pagination"] = typeof(Components.Pages.Docs.WPF.DataGrid.Pagination.Pagination),
        ["vekdatagrid/selection"] = typeof(Components.Pages.Docs.WPF.DataGrid.Selection.Selection),
        ["vekdatagrid/editing"] = typeof(Components.Pages.Docs.WPF.DataGrid.Editing.Editing),
        ["vekdatagrid/grouping"] = typeof(Components.Pages.Docs.WPF.DataGrid.Grouping.Grouping),
        ["vekdatagrid/aggregation"] = typeof(Components.Pages.Docs.WPF.DataGrid.Aggregation.Aggregation),
        ["vekdatagrid/hierarchy"] = typeof(Components.Pages.Docs.WPF.DataGrid.Hierarchy.Hierarchy),
        ["vekdatagrid/server-data"] = typeof(Components.Pages.Docs.WPF.DataGrid.ServerData.ServerData),
        ["vekdatagrid/virtualization"] = typeof(Components.Pages.Docs.WPF.DataGrid.Virtualization.Virtualization),
        ["vekdatagrid/column-management"] = typeof(Components.Pages.Docs.WPF.DataGrid.ColumnManagement.ColumnManagement),
        ["vekdatagrid/column-layout"] = typeof(Components.Pages.Docs.WPF.DataGrid.ColumnLayout.ColumnLayout),
        ["vekdatagrid/presentation"] = typeof(Components.Pages.Docs.WPF.DataGrid.Presentation.Presentation),
        ["vekdatagrid/context-actions"] = typeof(Components.Pages.Docs.WPF.DataGrid.ContextActions.ContextActions),
        ["vekdatagrid/observability"] = typeof(Components.Pages.Docs.WPF.DataGrid.Observability.Observability),
        ["vekcodeview/basic-usage"] = typeof(Components.Pages.Docs.WPF.CodeView.CodeView),
        ["vekcodeview/multi-sample"] = typeof(Components.Pages.Docs.WPF.CodeView.MultiSample),
    };

    public static Type For(string pageId) => Types.TryGetValue(pageId, out var pageType)
        ? pageType
        : throw new ArgumentOutOfRangeException(nameof(pageId), pageId, "No WPF page is registered for this ID.");
}
