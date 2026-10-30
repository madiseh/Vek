# DataGrid vendor architecture references

Research date: 2026-10-30

This note compares public documentation and source code for reusable behavior/state, column metadata, and server data callbacks. It distinguishes documented API shape from inferences about internal implementation.

## Findings

### Syncfusion

- Blazor `SfGrid` supports persisted component state for columns, filters, sorting, grouping, paging, and search, with browser `localStorage` persistence keyed by component ID. This is explicitly UI component state persistence, not evidence of a separate cross-platform state engine. [State Persistence in Blazor](https://blazor.syncfusion.com/documentation/common/state-persistance)
- Its Blazor `DataAdaptor` is an explicit extension point between `SfDataManager` and an external source. The callback receives `DataManagerRequest` with sort/filter/search/paging descriptors; the adaptor performs or delegates data operations and returns result/count. [Custom Binding in Blazor DataManager](https://blazor.syncfusion.com/documentation/data/custom-binding)
- Blazor columns combine behavior metadata (`Field`, sorting/filtering settings) with display configuration (`HeaderText`, `Width`, formatting). [Blazor Excel-like Filter](https://blazor.syncfusion.com/documentation/datagrid/excel-like-filter)
- WPF `SfDataGrid` exposes platform-specific `GridColumn` APIs such as `MappingName`, `AllowSorting`, and `AllowFiltering`; its collection view maintains grouping, filtering, sorting, and editing operations. [WPF Columns](https://help.syncfusion.com/wpf/datagrid/columns), [WPF Data Binding](https://help.syncfusion.com/wpf/datagrid/data-binding)
- Inference: Syncfusion documents a useful separation for server-data integration in Blazor, and a separate native WPF grid API. The public docs do not establish that Blazor and WPF use one shared .NET DataGrid engine or state model internally.

### DevExpress

- Blazor `DxGrid` exposes a grid component API with data shaping and grid-specific columns/templates. The saved `GridPersistentLayout` includes state such as page index/size, filter criteria, sort/group indexes, column visibility/order/width. [Blazor Grid](https://docs.devexpress.com/Blazor/403143/components/grid), [Save and Restore Layout](https://docs.devexpress.com/Blazor/406096/components/grid/save-restore-layout)
- For large/server datasets, DevExpress documents server-mode and virtual-source approaches in which data shaping is delegated to the underlying query provider or virtual source. [Blazor large datasets](https://docs.devexpress.com/Blazor/403162/components/data-grid/migrate-from-data-grid-to-grid), [WPF Server Data and Large Data Sources](https://docs.devexpress.com/WPF/401037/controls-and-libraries/data-grid/bind-to-data/server-data-and-large-data-sources)
- WPF `GridControl` similarly offers WPF-specific binding modes and columns; in Server Mode the WPF grid loads batches and performs shaping server-side. [WPF Server Mode](https://docs.devexpress.com/WPF/6279/controls-and-libraries/data-grid/bind-to-data/server-mode-and-instant-feedback-mode)
- Inference: DevExpress has analogous concepts on Blazor and WPF, and documents data access as a delegated boundary for large datasets. Public docs show platform-specific APIs and do not prove one shared cross-platform state/behavior engine.

### MudBlazor

- `MudDataGrid<T>.ServerData` accepts `Func<GridState<T>, CancellationToken, Task<GridData<T>>>`; the state contains current sorting, filtering, and pagination parameters, and the caller returns a page plus total count. [MudDataGrid API](https://mudblazor.com/api/MudDataGrid)
- The official docs say the grid calls this async function on pager navigation and sorting changes, and that ServerData cannot be combined with `Items` or `QuickFilter`. [MudDataGrid documentation source](https://github.com/MudBlazor/MudBlazor/blob/dev/src/MudBlazor.Docs/Pages/Components/DataGrid/DataGridPage.razor)
- The public implementation places state, behavior, rendering, and server callback handling in the Blazor `MudDataGrid` component/code-behind. [MudDataGrid source](https://github.com/MudBlazor/MudBlazor/blob/dev/src/MudBlazor/Components/DataGrid/MudDataGrid.razor.cs)
- Inference: MudBlazor demonstrates a useful server-data callback contract but is not a Blazor/WPF cross-platform comparison; it only supplies Blazor UI components.

## Relevance to Vek

These vendors support the proposed *public boundary* in different ways: column interaction produces grid state/request descriptors, and a caller-provided source executes server queries and returns rows plus a total. They are less clear as precedent for extracting a framework-independent engine shared by two UIs: the public documentation does not demonstrate that architecture. For Vek, treat the vendors as API and behavior references, not as evidence that a shared Core package is how they implement their internals.

A practical principle that follows from the docs (inference): keep core query/state descriptors platform-neutral and serializable; keep templates, editors, keyboard/mouse gestures, styling, and browser/WPF state persistence in the UI adapters. Let an application callback execute remote queries and return data plus total count.
