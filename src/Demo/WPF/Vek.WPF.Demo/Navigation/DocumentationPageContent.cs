namespace Vek.WPF.Demo.Navigation;

public enum DocumentationContentRole
{
    Explanation,
    NativeGrid,
    NativeGridCode,
    InDevelopment,
    CodeSample,
    MultiSample
}

public sealed record DocumentationSection(
    string Id, string Title, IReadOnlyList<string> Paragraphs, DocumentationContentRole Role, string? TocTitle = null);

public sealed record DocumentationArticle(
    string Title, string Introduction, IReadOnlyList<DocumentationSection> Sections);

/// <summary>
/// Mirrors the Blazor documentation articles. API prose describes the Blazor reference;
/// the renderer identifies native WPF support separately from these shared explanations.
/// </summary>
public static class DocumentationPageContent
{
    public static DocumentationArticle For(DemoPageDefinition page) => Articles[page.Id];

    private static readonly IReadOnlyDictionary<string, DocumentationArticle> Articles =
        new Dictionary<string, DocumentationArticle>
    {
        ["vekcodeview/basic-usage"] = new("VekCodeView: Basic Usage",
            "Display read-only code with syntax highlighting and a copy action.",
        [
            new("html-heading", "HTML",
            [
            ], DocumentationContentRole.CodeSample),
            new("java-heading", "Java",
            [
            ], DocumentationContentRole.CodeSample),
            new("csharp-heading", "C#",
            [
            ], DocumentationContentRole.CodeSample),
            new("sql-heading", "SQL",
            [
            ], DocumentationContentRole.CodeSample),
        ]),
        ["vekcodeview/multi-sample"] = new("VekCodeView: Multi-Sample",
            "Switch between titled code samples in one viewer.",
        [
            new("multi-sample-heading", "Choose a sample",
            [
            ], DocumentationContentRole.MultiSample),
        ]),
        ["vekdatagrid/aggregation"] = new("VekDataGrid: Aggregation",
            "Summarize the filtered products and each group with built-in or custom aggregate calculations.",
        [
            new("live-example", "Live example",
            [
                "Expand a category to see its price total. The footer summarizes all filtered products.",
            ], DocumentationContentRole.InDevelopment),
            new("code", "Code",
            [
            ], DocumentationContentRole.InDevelopment),
            new("aggregate-types", "Aggregate types and scopes",
            [
                "Choose Sum, Average, Count, Min, Max, or provide a custom aggregate. The default selector comes from the column; a definition can override it. Numeric operations skip null values, and Count counts non-null selected values.",
                "Choose footer, group, or both scopes. Aggregate and footer templates receive their items and computed values, while a group template can render the aggregates for its own rows.",
            ], DocumentationContentRole.Explanation, "Aggregate types"),
        ]),
        ["vekdatagrid/basic-usage"] = new("VekDataGrid: Basic Usage",
            "Display every supplied item in a sortable table, and opt into pagination with PageSize.",
        [
            new("live-example", "Live example",
            [
                "Click a column heading to sort the five sample products.",
            ], DocumentationContentRole.NativeGrid),
            new("code", "Code",
            [
                "Keep the component markup in Sample.razor and its data loading logic in Sample.razor.cs.",
            ], DocumentationContentRole.NativeGridCode),
            new("sorting-and-pagination", "Sorting and pagination",
            [
                "Each property column can be sorted by selecting its heading. When PageSize is omitted, all supplied items are displayed. Set it to opt into pagination.",
                "Column filters are opt-in: Filterable defaults to false. Set it to true on columns that need filtering and choose a filter layout on the grid. See Filtering for the available options.",
            ], DocumentationContentRole.Explanation),
        ]),
        ["vekdatagrid/column-layout"] = new("VekDataGrid: Column Layout",
            "Arrange related columns, set useful widths, and tune the table for dense or narrow layouts.",
        [
            new("live-example", "Live example",
            [
                "Resize columns from their heading controls. The header stays visible inside its scroll area, and the grid keeps wide content inside its own horizontal scroller.",
            ], DocumentationContentRole.InDevelopment),
            new("code", "Code",
            [
            ], DocumentationContentRole.InDevelopment),
            new("sizing", "Sizing and grouped headings",
            [
                "Give each column a stable Key before adding its key to ColumnGroups. Set Width, MinWidth, and MaxWidth to define its sizing range.",
                "ColumnResizeMode.Column exposes column controls; Container exposes the grid width control. Fixed headers scroll within the grid, while responsive mode contains wide columns in a horizontal scroller.",
            ], DocumentationContentRole.Explanation),
            new("appearance", "Density and row styling",
            [
                "Use Density, Bordered, and Striped to set common table treatments. Row, cell, header, and footer callbacks can add data-dependent classes or inline styles.",
            ], DocumentationContentRole.Explanation),
        ]),
        ["vekdatagrid/column-management"] = new("VekDataGrid: Column Management",
            "Let readers choose which columns they need and rearrange their order with pointer or keyboard controls.",
        [
            new("live-example", "Live example",
            [
                "Open the column panel to search, hide, show, or move columns. Drag the headers to change their order, or use the panel arrows for keyboard-accessible reordering.",
            ], DocumentationContentRole.InDevelopment),
            new("code", "Code",
            [
            ], DocumentationContentRole.InDevelopment),
            new("column-options", "Visibility and reordering",
            [
                "Set ShowColumnPanel to expose the searchable options panel. A column can bind its Visible state with @bind-Visible, and ColumnReorderMode selects panel controls, header drag and drop, both, or neither.",
                "The panel includes up/down buttons for keyboard reordering. A ColumnReordered callback receives the new key order, while headers and panel rows expose drop feedback classes during dragging.",
            ], DocumentationContentRole.Explanation),
        ]),
        ["vekdatagrid/context-actions"] = new("VekDataGrid: Context Actions",
            "Add row and cell actions with access to the item's stable column and current result index.",
        [
            new("live-example", "Live example",
            [
                "Use the row action to inspect a product. Cell actions can be limited to a column and receive an edit command in their context.",
            ], DocumentationContentRole.InDevelopment),
            new("code", "Code",
            [
            ], DocumentationContentRole.InDevelopment),
            new("actions", "Action contexts",
            [
                "RowActionsTemplate adds an action cell and provides the row item, index, selection state, and commands to select, toggle details, or edit.",
                "CellActionsTemplate adds actions inside each cell with the item, stable column key, title, row index, and an edit command. Actions stop row-click propagation.",
            ], DocumentationContentRole.Explanation),
            new("commands", "Programmatic commands",
            [
                "The grid exposes commands for clearing filters and sorting, reloading data, selecting rows, expanding or collapsing details, and starting or finishing edits.",
            ], DocumentationContentRole.Explanation),
        ]),
        ["vekdatagrid/editing"] = new("VekDataGrid: Editing and Validation",
            "Edit a cell or row through an isolated draft, validate it, then commit or cancel the change.",
        [
            new("live-example", "Live example",
            [
                "Start editing a product name. An empty name is rejected and leaves the draft open.",
            ], DocumentationContentRole.InDevelopment),
            new("code", "Code",
            [
            ], DocumentationContentRole.InDevelopment),
            new("edit-modes", "Edit modes and validation",
            [
                "Choose Cell, Row, or Form. Each editable property column supplies an EditTemplate that receives the draft and its EditContext. Use CloneItem when the default shallow copy is not sufficient.",
                "The grid validates the edit context before calling CommitEdit. Return false from that callback to keep the draft open. A successful commit raises ItemCommitted; cancel discards the draft and raises EditCancelled.",
            ], DocumentationContentRole.Explanation, "Edit modes"),
        ]),
        ["vekdatagrid/filtering"] = new("VekDataGrid: Filtering",
            "Combine quick predicates with per-column filters, choose a filter surface, and control string matching.",
        [
            new("live-example", "Live example",
            [
                "Choose a filter layout, then enter a value to narrow the products.",
            ], DocumentationContentRole.InDevelopment),
            new("code", "Code",
            [
                "Columns can select their comparison operator, and the grid applies filters before sorting and paging.",
            ], DocumentationContentRole.InDevelopment),
            new("filter-modes", "Filter modes and operators",
            [
                "Simple places controls above the grid, ColumnMenu groups each control in a menu, and ColumnRow places controls below the headings. Use FilterBy for a custom selector, FilterTemplate for a custom editor, and FilterCaseSensitive to set string matching behavior.",
                "Available operators include equality, text matching, numeric comparisons, and null or empty checks. Filter changes reset the page and are exposed through CurrentState and FilterChanged.",
            ], DocumentationContentRole.Explanation, "Filter modes"),
            new("filter-options", "Filter properties",
            [
                "Filtering is opt-in per column. Set Filterable=\"true\" on the columns users can filter, then choose where their controls appear with FilterMode.",
                "Filterable bool",
                "Enables filtering for this column and includes it in the chosen filter surface.",
                "Default: false",
                "FilterBy Func<TItem, object?>?",
                "Selects the value compared by filters. If omitted, filtering uses the value selected by Property.",
                "Default: null",
                "FilterOperator VekDataGridFilterOperator",
                "Sets the comparison used by the built-in editor, such as Contains or GreaterThanOrEqual.",
                "Default: Contains",
                "FilterTemplate RenderFragment<VekDataGridFilterContext>?",
                "Replaces the built-in editor with custom controls that can apply or clear this column's filter.",
                "Default: built-in editor",
                "FilterMode VekDataGridFilterMode",
                "Chooses the filter surface: Simple, ColumnMenu, or ColumnRow.",
                "Default: Simple",
                "FilterCaseSensitive bool",
                "Controls whether string comparisons distinguish uppercase and lowercase letters.",
                "Default: false",
                "QuickFilter Func<TItem, bool>?",
                "Applies an item-level predicate before the per-column filters.",
                "Default: null",
                "FilterChanged EventCallback<IReadOnlyList<VekDataGridFilter>>",
                "Notifies the parent when column filters are applied or cleared.",
            ], DocumentationContentRole.Explanation),
        ]),
        ["vekdatagrid/grouping"] = new("VekDataGrid: Grouping",
            "Organize products by one or more columns, customize group headers, and collapse groups as needed.",
        [
            new("live-example", "Live example",
            [
                "Groups are built from the rows on the current page. A group can appear on more than one page when its rows span pages.",
            ], DocumentationContentRole.InDevelopment),
            new("code", "Code",
            [
            ], DocumentationContentRole.InDevelopment),
            new("group-state", "Group state",
            [
                "Provide an ordered list of VekDataGridGroupDefinition values. Columns can use custom group selectors and group templates. Group headers start expanded; their controls expose aria-expanded, and GroupChanged reports expand/collapse interactions.",
                "Grouping runs after filtering, sorting, and paging. This keeps paging based on data rows, while a group header can repeat when that group crosses a page boundary.",
            ], DocumentationContentRole.Explanation),
        ]),
        ["vekdatagrid/hierarchy"] = new("VekDataGrid: Hierarchical Rows",
            "Expand a product row to reveal related information without leaving the table.",
        [
            new("live-example", "Live example",
            [
                "Use the row controls for individual details or the column header to toggle the current results.",
            ], DocumentationContentRole.InDevelopment),
            new("code", "Code",
            [
            ], DocumentationContentRole.InDevelopment),
            new("hierarchy-options", "Hierarchy options",
            [
                "Provide a HierarchyColumn with a detail template. Use InitiallyExpanded to open matching rows on first load and SingleExpandedRow to keep at most one detail visible.",
                "The header toggle expands or collapses the current result set. RowDetailsComparer preserves expanded state across item refreshes, and RowDetailsExpandedChanged reports the current expanded items.",
            ], DocumentationContentRole.Explanation),
        ]),
        ["vekdatagrid/observability"] = new("VekDataGrid: Collection Observability",
            "Keep the visible rows in sync when an observable collection changes.",
        [
            new("live-example", "Live example",
            [
                "Add a product to the collection. The grid subscribes to collection changes and refreshes its rows and selection state.",
            ], DocumentationContentRole.InDevelopment),
            new("code", "Code",
            [
            ], DocumentationContentRole.InDevelopment),
            new("updates", "Collection changes",
            [
                "When Items implements INotifyCollectionChanged, the grid subscribes while that collection is assigned and refreshes after inserts, removals, moves, or replacements.",
                "When the Items source is replaced or the grid is disposed, it removes the old event subscription. Ordinary enumerable sources continue to render when their owning component rerenders.",
            ], DocumentationContentRole.Explanation),
        ]),
        ["vekdatagrid/pagination"] = new("VekDataGrid: Pagination",
            "Control the current page and page size, or replace the built-in pager with a custom surface.",
        [
            new("live-example", "Live example",
            [
                "Change the rows per page or use the custom pager below the table.",
            ], DocumentationContentRole.InDevelopment),
            new("code", "Code",
            [
            ], DocumentationContentRole.InDevelopment),
            new("paging-state", "Paging state",
            [
                "Bind CurrentPage and PageSize to keep paging state in the parent. Set PageSizeOptions to customize the built-in selector. A custom PagerContent receives current state, total item and page counts, and async actions to change the page or page size.",
                "When filters or sorting change, the grid returns to the first page. If the item collection shrinks, the current page is clamped to the remaining pages.",
            ], DocumentationContentRole.Explanation),
        ]),
        ["vekdatagrid/presentation"] = new("VekDataGrid: Presentation",
            "Shape the grid around your product language with templates, culture-aware values, and accessible table semantics.",
        [
            new("live-example", "Live example",
            [
                "The toolbar and extra heading row are supplied by the page. Use the Edit action on a price cell to see the grid culture in its edit context.",
            ], DocumentationContentRole.InDevelopment),
            new("code", "Code",
            [
            ], DocumentationContentRole.InDevelopment),
            new("templates", "Templates",
            [
                "Use ToolbarTemplate for content above the table and HeaderTemplate for extra semantic rows in the table heading. Column cells, row content, group headings, and footer content can also use templates.",
                "For a RowTemplate, render valid table cells for each item. It replaces the grid's default cell rendering for those rows while preserving the row's selection, click, and style behavior.",
            ], DocumentationContentRole.Explanation),
            new("culture-accessibility", "Culture and accessibility",
            [
                "Set Culture to control numeric and aggregate formatting. Edit templates receive that value through VekDataGridEditContext.Culture.",
                "Set AriaLabel to name the table for assistive technology. Unmatched attributes such as data-testid are applied directly to the native table. Built-in actions use labeled native controls that remain in the keyboard tab order.",
            ], DocumentationContentRole.Explanation),
        ]),
        ["vekdatagrid/selection"] = new("VekDataGrid: Row Selection",
            "Bind one or many selected products, select from the row or checkbox, and control which rows can be selected.",
        [
            new("live-example", "Live example",
            [
                "Select products with the row checkboxes or click a row. The header checkbox selects the current page.",
            ], DocumentationContentRole.InDevelopment),
            new("code", "Code",
            [
            ], DocumentationContentRole.InDevelopment),
            new("selection-options", "Selection options",
            [
                "Choose None, Single, or Multiple. Bind SelectedItem or SelectedItems, add a SelectionColumn for checkbox selection, and provide an IsRowSelectable predicate for disabled rows.",
                "SelectionComparer matches selected rows when data refreshes. FilteredSelectionMode chooses whether filtered rows remain selected or are cleared.",
            ], DocumentationContentRole.Explanation),
        ]),
        ["vekdatagrid/server-data"] = new("VekDataGrid: Server Data",
            "Load only the requested page and let the data source apply the grid's current sort and filter state.",
        [
            new("live-example", "Live example",
            [
                "Sorting, filtering, and paging send a new cancellable request to the sample provider.",
            ], DocumentationContentRole.InDevelopment),
            new("code", "Code",
            [
            ], DocumentationContentRole.InDevelopment),
            new("server-contract", "Server request contract",
            [
                "The source receives the requested page, page size, sort definitions, filters, and a cancellation token. Return the page rows together with the total number of matching rows. A new request cancels the previous one; failures can be presented with ErrorContent, and RefreshServerDataAsync reloads the current request.",
            ], DocumentationContentRole.Explanation),
        ]),
        ["vekdatagrid/sorting"] = new("VekDataGrid: Sorting",
            "Sort by one column or combine columns to create a prioritized order.",
        [
            new("live-example", "Live example",
            [
                "Click anywhere in a heading to cycle through ascending, descending, and unsorted. Hold Ctrl while selecting another heading to add a secondary sort.",
            ], DocumentationContentRole.InDevelopment),
            new("code", "Code",
            [
                "Enable multi-column sorting with SortMode=\"Multiple\".",
            ], DocumentationContentRole.InDevelopment),
            new("sort-modes", "Sort modes",
            [
                "Choose None, Single, or Multiple. Multiple sorts keep their priority order; click a heading with Alt to remove that sort. Set AllowUnsorted to false to keep a column sorted after it is selected. A custom selector or comparer can define specialized ordering.",
            ], DocumentationContentRole.Explanation),
        ]),
        ["vekdatagrid/virtualization"] = new("VekDataGrid: Virtualization",
            "Render a small viewport-sized window from a large local collection or request ranges from a remote data source.",
        [
            new("client-virtualization", "Client data",
            [
                "The grid has 500 product records. Sorting and filtering apply before the virtualizer renders the visible window.",
            ], DocumentationContentRole.InDevelopment),
            new("server-virtualization", "Server data",
            [
                "The source receives a start index and requested row count with the current sort/filter state. It can cancel work when the visible range changes.",
            ], DocumentationContentRole.InDevelopment),
            new("code", "Code",
            [
            ], DocumentationContentRole.InDevelopment),
            new("virtualization-options", "Sizing and supported features",
            [
                "VirtualItemSize should match the rendered row height. VirtualOverscanCount adds rows around the viewport, and VirtualMaxItemCount bounds each render. Virtualization uses the document scroll container and hides the pager.",
                "Grouping and hierarchical row details are rejected because they introduce variable-height rows. For server ranges, return the matching total and no more rows than the requested count; the grid also trims responses that extend beyond the reported end.",
            ], DocumentationContentRole.Explanation, "Sizing"),
        ]),
        ["getting-started"] = new("Getting Started",
            "This guide is being prepared.",
        [
        ]),
    };
}
