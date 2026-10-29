# VekDataGrid Feature Expansion Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use `superpowers:executing-plans` to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Expand `VekDataGrid<TItem>` into a complete reusable Blazor data grid, with one live Demo subpage per major feature family.

**Architecture:** Establish Vek-owned column, state, data-source, and event contracts in `Vek.Components`, then build feature families on those contracts. The Demo remains a consumer of the component library and uses its existing `DemoProduct` model and data provider. Each task is a testable feature slice that includes its Demo page and a separate commit.

**Tech Stack:** .NET 10, Razor Class Library, Blazor, bUnit, xUnit.

**Spec:** `docs/superpowers/specs/2026-10-29-vek-data-grid-design.md`

## Global Constraints

- All projects target `net10.0` unless an existing documented project-specific reason requires otherwise.
- `Vek.Components` has no reference to Demo or sample-data projects.
- Use Vek-owned public APIs and implementation; do not add any external grid dependency or external grid names, APIs, namespaces, markup, styles, or copied code.
- Use the existing `DemoProduct` model for every feature page. Keep the current live-grid and Razor/code-behind tab pattern.
- Every C# `[Fact]` or `[Theory]` has an XML `/// <summary>` directly above its attribute.
- For each behavior, add a focused failing test first, implement the behavior, rerun the focused test, then run the relevant test suite.
- Every feature task updates the component, its automated tests, and its Demo subpage before its own commit.
- Execute tasks in order without requesting per-task approval; commit each task independently after its checks pass.

## Review Focus

- Empty collections, empty result pages, invalid page sizes, and collection replacement must render safely and keep paging state valid. Cover in Tasks 1 and 4.
- Nullable values, non-string values, custom comparers, and culture-specific formatting must sort/filter/render consistently. Cover in Tasks 1–3 and 13.
- Filters, sort changes, page-size changes, and server reloads must reset or preserve page state predictably. Cover in Tasks 2–4 and 9.
- Selection and edit state must remain coherent when rows are filtered, sorted, replaced, canceled, or rejected by validation. Cover in Tasks 5–6.
- Async loading must honor cancellation and avoid stale responses overwriting newer grid state; virtual ranges must handle boundaries. Cover in Tasks 9–10.

---

## File Map

- `src/Components/Vek.Components/Components/DataGrid/VekDataGrid.razor(.cs|.css)` owns the table shell, state orchestration, and shared presentation.
- `IVekDataGridColumn.cs`, `PropertyColumn.razor(.cs)`, and new column components own registration and column-specific behavior. Add focused model files beside the component when they have a clear responsibility.
- `tests/Components/Vek.Components.Tests/DataGrid/` owns bUnit behavior tests; `DataGridTestHelpers.cs` owns reusable fixtures and render helpers.
- `src/Demo/Vek.Demo/Components/Pages/Docs/DataGrid/<Feature>/` owns one page, code-behind, and scoped style per feature family.
- `src/Demo/Vek.Demo/Components/Layout/DocumentationLayout.razor` owns the DataGrid feature navigation.
- `tests/Demo/Vek.Demo.Tests/DemoPageNavigationTests.cs` verifies the new documentation routes and samples.

## Shared Contracts

Task 1 defines these Vek-owned contracts, then later tasks extend them only when needed:

- `VekDataGridState<TItem>`: `Page`, `PageSize`, ordered `Sorts`, and active `Filters`.
- `VekDataGridSort`: stable column key, direction, and priority.
- `VekDataGridFilter`: stable column key, operator, value, and case-sensitivity setting.
- `VekDataGridData<TItem>`: returned `Items` and unpaged `TotalItems`.
- `VekDataGrid<TItem>.ServerData`: `Func<VekDataGridState<TItem>, CancellationToken, Task<VekDataGridData<TItem>>>`.
- Column definitions expose a stable key, title, cell template, optional value selector/comparer, and per-feature capability switches. Keep implementation-only registration contracts internal.

---

### Task 1: Grid foundation, basic usage, and column types

**Files:**
- Modify: `src/Components/Vek.Components/Components/DataGrid/VekDataGrid.razor(.cs|.css)`
- Modify: `src/Components/Vek.Components/Components/DataGrid/IVekDataGridColumn.cs`
- Modify: `src/Components/Vek.Components/Components/DataGrid/PropertyColumn.razor(.cs)`
- Create: `src/Components/Vek.Components/Components/DataGrid/TemplateColumn.razor(.cs)` and focused state/column model files
- Modify: `tests/Components/Vek.Components.Tests/DataGrid/DataGridTestHelpers.cs`
- Create: `tests/Components/Vek.Components.Tests/DataGrid/ColumnDefinitionTests.cs`
- Modify: `src/Demo/Vek.Demo/Components/Pages/Docs/DataGrid/BasicUsage/BasicUsage.razor(.cs|.css)`
- Modify: `src/Demo/Vek.Demo/Components/Layout/DocumentationLayout.razor` only if the existing Basic Usage route needs correcting
- Verify: `tests/Demo/Vek.Demo.Tests/DemoPageNavigationTests.cs`

**Interfaces:** Consumes the current `Items`, `PageSize`, `PropertyColumn.Property`, and `PropertyColumn.Title`. Produces `VekDataGridState<TItem>`, stable column metadata, property columns with format/cell templates, and template columns with cell content.

- [x] Add tests `PropertyColumnUsesTitleAndFormat`, `PropertyColumnRendersCustomCellTemplate`, and `TemplateColumnRendersCustomCellContent` to `ColumnDefinitionTests.cs`; assert rendered header and formatted/template cell content.
- [x] Run focused tests and confirm the new assertions fail first for the missing format and template-column behavior.
- [x] Implement the shared grid/column contracts and basic column features in the listed component files. Preserve the current default sort/paging behavior and use semantic table markup.
- [x] Rerun focused tests and `dotnet test tests/Components/Vek.Components.Tests`.
- [x] Update the Basic Usage page with both column types, a live example, and Razor/code-behind tabs; verify its route and sample through `dotnet test tests/Demo/Vek.Demo.Tests --filter FullyQualifiedName~DataGridBasicUsageRouteShowsLiveTableAndExample`.
- [x] Commit the complete slice as `feat: establish data grid foundation and columns`.

### Task 2: Sorting

**Files:** Modify grid and column files; `tests/Components/Vek.Components.Tests/DataGrid/SortingTests.cs`; create `src/Demo/Vek.Demo/Components/Pages/Docs/DataGrid/Sorting/Sorting.razor(.cs|.css)`; modify documentation navigation and `DemoPageNavigationTests.cs`.

**Interfaces:** Consumes stable column keys/value selectors. Produces sortable capability switches, `SortMode` (none/single/multiple), ordered sort state, direction/unsorted transitions, comparer and selector customization, and sort-change callbacks.

- [x] Add XML-documented tests `SortModeCanDisableSorting`, `MultipleSortUsesPriorityAndDirection`, `CustomSelectorAndComparerControlOrder`, and `SortChangeReportsOrderedState`.
- [x] Run the focused sorting tests and observe the expected failures.
- [x] Implement client-side sort state and comparison; preserve null/nullable behavior and expose changes to bound state/callbacks.
- [x] Run the focused sorting tests and full component test project.
- [x] Add a live sorting page using `DemoProduct`, code tabs, and a tested route/navigation link.
- [x] Commit as `feat: add data grid sorting`.

### Task 3: Filtering

**Files:** Modify grid, column, and filter model files; create `tests/Components/Vek.Components.Tests/DataGrid/FilteringTests.cs`; create `.../Docs/DataGrid/Filtering/Filtering.razor(.cs|.css)`; update navigation and route tests.

**Interfaces:** Consumes columns with filter selectors. Produces quick filtering, per-column filter definitions/operators, simple panel, per-column menu and inline-row modes, case sensitivity, custom filter templates, and filter state callbacks.

- [x] Add tests `QuickFilterMatchesAcrossRows`, `ColumnFilterModesApplyPredicates`, `StringCaseSensitivityIsConfigurable`, `FilterTemplateAppliesAndClearsDefinition`, and `ApplyingFilterResetsPage`.
- [x] Run focused filtering tests; confirm failures represent missing behavior.
- [x] Implement filter state, client evaluation, filter UI variants, custom filter surfaces, and clearing/apply behavior.
- [x] Rerun focused filtering tests and component suite.
- [x] Add the live Filtering page with examples for each mode and a tested route.
- [x] Commit as `feat: add data grid filtering`.

### Task 4: Paging

**Files:** Modify grid and pager files; create pager component/model files where useful; create `tests/Components/Vek.Components.Tests/DataGrid/PaginationTests.cs` updates and `.../Docs/DataGrid/Pagination/Pagination.razor(.cs|.css)`; update navigation and route tests.

**Interfaces:** Consumes `VekDataGridState<TItem>`. Produces bindable current page/page size, total count, custom pager content, page-size changes, and page navigation.

- [x] Add tests `RowsPerPageCanChange`, `PagerContentReceivesCurrentState`, `PageIsClampedWhenItemsShrink`, and `EmptyItemsKeepOneValidPage`; retain existing next/previous test.
- [x] Run focused paging tests and confirm expected failures.
- [x] Implement client paging and pager state transitions; reset page when page size changes.
- [x] Run focused paging tests and component suite.
- [x] Add the live Pagination page and test navigation.
- [x] Commit as `feat: add data grid paging`.

### Task 5: Row selection

**Files:** Modify grid/column files; create `tests/Components/Vek.Components.Tests/DataGrid/SelectionTests.cs` and `.../Docs/DataGrid/Selection/Selection.razor(.cs|.css)`; update navigation and route tests.

**Interfaces:** Produces single/multiple selected item state, row-click/explicit-checkbox selection, select-column capability, disabled-row predicate, stable comparer, and selection callbacks.

- [x] Add tests `RowClickSelectsSingleItem`, `CheckboxSelectsMultipleItems`, `DisabledRowsCannotBeSelected`, `ComparerKeepsSelectionAfterItemsRefresh`, and `FilteredOutSelectionFollowsConfiguredPolicy`.
- [x] Run focused selection tests and confirm failures.
- [x] Implement selection semantics and accessible checkbox labels without coupling them to visual styling.
- [x] Run focused selection tests and component suite.
- [x] Add the live Selection page using `DemoProduct` and verify its route.
- [x] Commit as `feat: add data grid row selection`.

### Task 6: Editing and validation

**Files:** Modify grid/column files; create edit-state/context files; create `tests/Components/Vek.Components.Tests/DataGrid/EditingTests.cs` and `.../Docs/DataGrid/Editing/Editing.razor(.cs|.css)`; update navigation and route tests.

**Interfaces:** Produces read-only/editable state, cell/inline-row/form edit modes, edit templates, clone strategy, validation, commit/cancel actions, and before/after/cancel lifecycle callbacks.

- [ ] Add tests `CellEditCommitsValue`, `InlineEditAllowsOnlyOneRow`, `FormEditCanKeepOpenAfterRejectedCommit`, `CancelRestoresOriginalItem`, and `ValidationFailurePreventsCommit`.
- [ ] Run focused editing tests and confirm expected failures.
- [ ] Implement copied edit state, editable-column controls, validation, commit/cancel, and callbacks. Do not mutate the source item before a successful commit.
- [ ] Rerun focused editing tests and component suite.
- [ ] Add the live Editing page with a `DemoProduct` example and tested route.
- [ ] Commit as `feat: add data grid editing and validation`.

### Task 7: Grouping

**Files:** Modify grid/column files; create group definition/row files; create `tests/Components/Vek.Components.Tests/DataGrid/GroupingTests.cs` and `.../Docs/DataGrid/Grouping/Grouping.razor(.cs|.css)`; update navigation and route tests.

**Interfaces:** Produces multi-level grouping, per-column groupability/default order, group templates, expand/collapse state, and group callbacks.

- [ ] Add tests `RowsGroupBySelectedColumn`, `GroupsCanBeNestedAndOrdered`, `GroupTemplateReceivesDefinition`, and `GroupsCanExpandAndCollapse`.
- [ ] Run focused grouping tests and confirm expected failures.
- [ ] Implement grouping over the current filtered/sorted dataset; define and test its interaction with paging and virtualization.
- [ ] Run focused grouping tests and component suite.
- [ ] Add the live Grouping page and verify route/navigation.
- [ ] Commit as `feat: add data grid grouping`.

### Task 8: Aggregation

**Files:** Modify grid/column/group files; create aggregation definition files; create `tests/Components/Vek.Components.Tests/DataGrid/AggregationTests.cs` and `.../Docs/DataGrid/Aggregation/Aggregation.razor(.cs|.css)`; update navigation and route tests.

**Interfaces:** Produces average, count, custom, max, min, and sum aggregation definitions; aggregate templates; footer templates; group-level aggregation.

- [ ] Add tests `BuiltInAggregatesProduceExpectedValues`, `CustomAggregateUsesProvidedSelector`, `FooterTemplateReceivesCurrentItems`, and `GroupedRowsShowTheirOwnAggregates`.
- [ ] Run focused aggregation tests and confirm failures.
- [ ] Implement aggregate computation over defined grid/group scopes and custom rendering.
- [ ] Run focused aggregation tests and component suite.
- [ ] Add the live Aggregation page and verify route/navigation.
- [ ] Commit as `feat: add data grid aggregation`.

### Task 9: Hierarchical row details

**Files:** Modify grid files; create `HierarchyColumn` and row-detail context files; create `tests/Components/Vek.Components.Tests/DataGrid/HierarchyTests.cs` and `.../Docs/DataGrid/Hierarchy/Hierarchy.razor(.cs|.css)`; update navigation and route tests.

**Interfaces:** Produces expandable row details, custom detail renderer/template, initial expansion predicate, single-expanded-row behavior, and header expand/collapse controls.

- [ ] Add tests `RowDetailExpandsAndCollapses`, `InitialExpansionPredicateIsApplied`, `SingleExpansionClosesPreviousRow`, and `HeaderToggleControlsAllRows`.
- [ ] Run focused hierarchy tests and confirm failures.
- [ ] Implement hierarchy state using the configured comparer and preserve expanded state correctly across data refresh.
- [ ] Run focused hierarchy tests and component suite.
- [ ] Add the live Hierarchy page and verify route/navigation.
- [ ] Commit as `feat: add hierarchical row details`.

### Task 10: Server data

**Files:** Modify grid/state/data result files; create `tests/Components/Vek.Components.Tests/DataGrid/ServerDataTests.cs` and `.../Docs/DataGrid/ServerData/ServerData.razor(.cs|.css)`; update navigation and route tests.

**Interfaces:** Uses the Task 1 `VekDataGridState<TItem>` and `VekDataGridData<TItem>` contracts. Produces mutually exclusive local/server sources, async load with cancellation, total count, loading/empty/error presentation, refresh, and server sort/filter/page state.

- [ ] Add tests `ServerDataReceivesCurrentState`, `ServerDataReturnsPageAndTotalCount`, `NewRequestCancelsPreviousRequest`, `StaleResponseCannotReplaceNewerData`, and `ServerFailureCanRenderErrorContent`.
- [ ] Run focused server-data tests and confirm failures.
- [ ] Implement the async source contract and state transitions. Reject invalid source combinations with a clear exception.
- [ ] Run focused server-data tests and component suite.
- [ ] Add a Demo callback that applies the state to the existing `DemoProduct` sample provider; add the live Server Data page and route test.
- [ ] Commit as `feat: add server-backed data grid loading`.

### Task 11: Virtualization

**Files:** Modify grid/data-source files; create `tests/Components/Vek.Components.Tests/DataGrid/VirtualizationTests.cs` and `.../Docs/DataGrid/Virtualization/Virtualization.razor(.cs|.css)`; update navigation and route tests.

**Interfaces:** Produces bounded client rendering and range-based async server loading using `VekDataGridState<TItem>` plus start/count/cancellation, with explicit row size and constrained-height requirements.

- [ ] Add tests `ClientVirtualizationRendersVisibleRange`, `ServerVirtualizationRequestsRequestedRange`, `VirtualRangeClampsAtEnd`, and `VirtualizationRejectsUnsupportedGroupingCombination`.
- [ ] Run focused virtualization tests and confirm failures.
- [ ] Implement client and server virtualization; document and enforce feature combinations that cannot coexist.
- [ ] Run focused virtualization tests and component suite.
- [ ] Add the live Virtualization page with a larger generated `DemoProduct` collection and tested route.
- [ ] Commit as `feat: add data grid virtualization`.

### Task 12: Column management

**Files:** Modify grid/column files; create column-panel support files; create `tests/Components/Vek.Components.Tests/DataGrid/ColumnManagementTests.cs` and `.../Docs/DataGrid/ColumnManagement/ColumnManagement.razor(.cs|.css)`; update navigation and route tests.

**Interfaces:** Produces hide/show, visibility binding, column options, searchable/reorderable column panel, drag/drop reorder modes, and drop feedback classes.

- [ ] Add tests `ColumnVisibilityCanBeToggled`, `ColumnPanelFiltersColumnsBySearch`, `PanelReorderingUsesInsertPosition`, `HeaderReorderingUsesConfiguredMode`, and `HiddenColumnsAreExcludedFromRendering`.
- [ ] Run focused column-management tests and confirm failures.
- [ ] Implement accessible column controls and drag/drop interactions; provide keyboard alternatives for reorder and visibility.
- [ ] Run focused tests and component suite.
- [ ] Add the live Column Management page and verify its route.
- [ ] Commit as `feat: add data grid column management`.

### Task 13: Column sizing, styling, and responsive layout

**Files:** Modify grid/column Razor and CSS; create `tests/Components/Vek.Components.Tests/DataGrid/ColumnLayoutTests.cs` and `.../Docs/DataGrid/ColumnLayout/ColumnLayout.razor(.cs|.css)`; update navigation and route tests.

**Interfaces:** Produces column groups/initial sizing, column/container resize modes, breakpoint behavior, density, borders, striping, fixed header, and per-row/cell/header/footer class/style functions.

- [ ] Add tests `ColumnGroupSpansConfiguredColumns`, `ColumnResizeUpdatesConfiguredWidth`, `RowStyleFunctionReceivesItemAndIndex`, `FixedHeaderKeepsHeaderVisible`, and `NarrowGridAvoidsPageOverflow`.
- [ ] Run focused layout tests and confirm failures.
- [ ] Implement sizing/resize/layout behavior with visible keyboard focus and pointer/touch affordances; preserve sensible horizontal overflow for genuinely wide data.
- [ ] Run focused tests and component suite.
- [ ] Add the live Column Layout page and verify navigation.
- [ ] Commit as `feat: add data grid column layout controls`.

### Task 14: Templates, presentation, culture, and accessibility

**Files:** Modify grid/column files; create `tests/Components/Vek.Components.Tests/DataGrid/PresentationTests.cs` and `.../Docs/DataGrid/Presentation/Presentation.razor(.cs|.css)`; update navigation and route tests.

**Interfaces:** Produces toolbar/header/cell/footer/row/group templates, custom no-record/loading content, culture-aware formats, table attributes, accessible names, labels, and keyboard-operable controls.

- [ ] Add tests `ToolbarAndHeaderTemplatesRender`, `LoadingAndEmptyTemplatesRenderForTheirStates`, `CultureFormatsValuesAndEditors`, `TableHasAccessibleNameAndSortState`, and `AllGridActionsAreKeyboardReachable`.
- [ ] Run focused presentation tests and confirm failures.
- [ ] Implement templating and accessible semantics. Keep labels, focus, and state feedback visible and preserve clear overflow behavior on narrow widths.
- [ ] Run focused tests and component suite.
- [ ] Add the live Presentation page and verify route/navigation.
- [ ] Commit as `feat: add data grid presentation and accessibility`.

### Task 15: Context actions and collection observability

**Files:** Modify grid/row/cell files; create `tests/Components/Vek.Components.Tests/DataGrid/ContextActionsTests.cs` and `ObservabilityTests.cs`; create `.../Docs/DataGrid/ContextActions/ContextActions.razor(.cs|.css)` and `.../Docs/DataGrid/Observability/Observability.razor(.cs|.css)`; update navigation and route tests.

**Interfaces:** Produces row/cell context action templates/events, public state commands for clear filters/sort/reload/expand/collapse/edit/select, and automatic refresh for `INotifyCollectionChanged` sources.

- [ ] Add XML-documented tests `ContextActionReceivesRowAndCell`, `PublicCommandsUpdateGridState`, and `ObservableCollectionChangesRefreshRows`.
- [ ] Run focused tests and confirm expected failures.
- [ ] Implement context action hooks, documented public commands, and safe event subscription/unsubscription for observable collections.
- [ ] Run focused tests, all component tests, all Demo tests, and build the Demo project.
- [ ] Add both live pages and verify both routes/navigation entries.
- [ ] Review the full diff, confirm no excluded dependency or naming traces were introduced, then commit as `feat: add data grid context actions and observability`.

## Final Verification

- [ ] Run `dotnet test tests/Components/Vek.Components.Tests`.
- [ ] Run `dotnet test tests/Demo/Vek.Demo.Tests`.
- [ ] Run `dotnet build src/Demo/Vek.Demo/Vek.Demo.csproj`.
- [ ] Search the changed project files and commit messages for prohibited external grid names/dependencies.
- [ ] Review accessibility, responsive overflow, empty/loading/error states, feature interaction rules, and the full commit sequence.
