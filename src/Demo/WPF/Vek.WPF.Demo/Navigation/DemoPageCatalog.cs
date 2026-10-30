namespace Vek.WPF.Demo.Navigation;

public static class DemoPageCatalog
{
    // IDs are stable navigation keys; titles and order follow the Blazor documentation.
    public static IReadOnlyList<DemoPageDefinition> All { get; } = Array.AsReadOnly<DemoPageDefinition>(
    [
        new("home", "Home", "Overview",
            "Explore native WPF component examples and documentation.", DemoPageImplementationStatus.Implemented),
        new("getting-started", "Getting Started", "Overview",
            "Create a .NET 10 WPF app, add Vek.WPF.DataGrid, and bind the native grid to your product collection.", DemoPageImplementationStatus.Implemented),
        new("vekdatagrid/basic-usage", "Basic Usage", "VekDataGrid",
            "Display the first five shared products in a sortable native WPF data grid.", DemoPageImplementationStatus.Implemented),
        new("vekdatagrid/sorting", "Sorting", "VekDataGrid",
            "Sort by one column or combine columns to create a prioritized order.", DemoPageImplementationStatus.InDevelopment),
        new("vekdatagrid/filtering", "Filtering", "VekDataGrid",
            "Combine quick predicates with per-column filters, choose a filter surface, and control string matching.", DemoPageImplementationStatus.InDevelopment),
        new("vekdatagrid/pagination", "Pagination", "VekDataGrid",
            "Control the current page and page size, or replace the built-in pager with a custom surface.", DemoPageImplementationStatus.InDevelopment),
        new("vekdatagrid/selection", "Row Selection", "VekDataGrid",
            "Bind one or many selected products, select from the row or checkbox, and control which rows can be selected.", DemoPageImplementationStatus.InDevelopment),
        new("vekdatagrid/editing", "Editing", "VekDataGrid",
            "Edit a cell or row through an isolated draft, validate it, then commit or cancel the change.", DemoPageImplementationStatus.InDevelopment),
        new("vekdatagrid/grouping", "Grouping", "VekDataGrid",
            "Organize products by one or more columns, customize group headers, and collapse groups as needed.", DemoPageImplementationStatus.InDevelopment),
        new("vekdatagrid/aggregation", "Aggregation", "VekDataGrid",
            "Summarize the filtered products and each group with built-in or custom aggregate calculations.", DemoPageImplementationStatus.InDevelopment),
        new("vekdatagrid/hierarchy", "Hierarchy", "VekDataGrid",
            "Expand a product row to reveal related information without leaving the grid.", DemoPageImplementationStatus.InDevelopment),
        new("vekdatagrid/server-data", "Server Data", "VekDataGrid",
            "Load only the requested page and let the data source apply the grid's current sort and filter state.", DemoPageImplementationStatus.InDevelopment),
        new("vekdatagrid/virtualization", "Virtualization", "VekDataGrid",
            "Render a small viewport-sized window from a large local collection or request ranges from a remote data source.", DemoPageImplementationStatus.InDevelopment),
        new("vekdatagrid/column-management", "Column Management", "VekDataGrid",
            "Let readers choose which columns they need and rearrange their order with pointer or keyboard controls.", DemoPageImplementationStatus.InDevelopment),
        new("vekdatagrid/column-layout", "Column Layout", "VekDataGrid",
            "Arrange related columns, set useful widths, and tune the grid for dense or narrow layouts.", DemoPageImplementationStatus.InDevelopment),
        new("vekdatagrid/presentation", "Presentation", "VekDataGrid",
            "Shape the grid around your product language with templates, culture-aware values, and accessible native controls.", DemoPageImplementationStatus.InDevelopment),
        new("vekdatagrid/context-actions", "Context Actions", "VekDataGrid",
            "Add row and cell actions with access to the item's stable column and current result index.", DemoPageImplementationStatus.InDevelopment),
        new("vekdatagrid/observability", "Collection Observability", "VekDataGrid",
            "Keep the visible rows in sync when an observable collection changes.", DemoPageImplementationStatus.InDevelopment),
        new("vekcodeview/basic-usage", "Basic Usage", "VekCodeView",
            "Display titled read-only code samples in a native WPF text viewer.", DemoPageImplementationStatus.Implemented),
        new("vekcodeview/multi-sample", "Multi-Sample", "VekCodeView",
            "Switch between titled code samples in one viewer.", DemoPageImplementationStatus.Implemented)
    ]);

    public static IReadOnlyList<DemoPageDefinition> Documentation { get; } =
        Array.AsReadOnly(All.Where(page => page.Id != "home").ToArray());
}
