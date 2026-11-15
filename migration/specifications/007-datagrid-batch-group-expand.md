# Migration Specification: 007 - DataGrid Batch Group Collapse and Expand Controls

## 1. Metadata
- **Feature ID**: 007
- **Feature Slug**: datagrid-batch-group-expand
- **Module**: DataGrid
- **Status**: Completed
- **Discovered At**: 2026-11-15 19:23:28Z
- **Dependencies**: []

---

## 2. Behavioral Scope & Requirements
- **Feature Purpose**: Let callers expand or collapse all grouped rows with one action, including nested groups and paged data.
- **Functional Requirements**:
  - `ExpandAllGroupsAsync()` expands every active grouping level.
  - `CollapseAllGroupsAsync()` collapses every active grouping level.
  - The operations are no-ops when grouping is inactive and preserve the grid's normal rendering and paging behavior.
  - Applications can connect accessible buttons to the methods; the grid does not impose toolbar UI.

## 3. Architectural Mapping
- The feature is implemented by `VekDataGrid<T>.ExpandAllGroupsAsync()` and `CollapseAllGroupsAsync()`.
- The shared recursive state update clears per-group overrides, updates each active grouping column through its registered state, then rebuilds grouped items.
- No additional parameter, component, or styling API is required.

## 4. Existing Behavior Coverage
- `DataGridGroupExpandAllCollapseAll` verifies expanding, paging through expanded groups, collapsing, and refresh behavior.
- `DataGridGroupCollapseAll` and server-data grouping tests cover additional data sources and initial expansion states.
- The DataGrid grouping examples expose the methods through caller-owned buttons, keeping accessible names and keyboard behavior in application markup.

## 5. Verification & Acceptance Criteria
- Build and test the DataGrid test project with zero warnings and all applicable tests passing.
- Run the repository formatting check after implementation or documentation changes.
- Keep commits, pull request text, source, and documentation free of third-party product identifiers.
