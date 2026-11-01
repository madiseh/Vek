<!-- Copyright (c) 2026 AMIN SABAGHI -->
<!-- Licensed under the MIT License. See the LICENSE file in the project root for more information. -->
<!-- Created: 2026-11-02 -->

# DataGrid shared operations

This implementation follows the [Core Architecture Rule](core-architecture.md). The shared public command surface is `VekDataGridHostExtensions`, split into partial source files by responsibility. Both UI adapters implement `IVekDataGridHost<TItem, TTemplate>` and supply one per-instance engine. Shared algorithms and interaction state remain in Core.

## Shared surface

| Concern | Core operations |
| --- | --- |
| Query | `ApplyFilterAsync`, `RemoveFilterAsync`, `SortAsync`, `ClearSortAsync` |
| Paging | `SetCurrentPageAsync`, `SetPageSizeAsync` |
| Columns | `SetColumnVisibilityAsync`, `ReorderColumnAsync`, `ResizeColumnAsync` |
| Selection | `CanSelectItem`, `IsItemSelected`, `SelectItemAsync`, `SelectRowAsync`, `ToggleSelectionAsync`, `AreAllVisibleItemsSelected`, `SelectAllAsync`, `ToggleAllVisibleItemsAsync`, `ReconcileSelectionAsync` |
| Row details | `IsRowExpanded`, `ExpandAllAsync`, `CollapseAllAsync`, `ToggleRowDetailsAsync`, `AreAllRowsExpanded`, `ToggleAllRowDetailsAsync` |
| Group expansion | `ToggleGroupAsync` |
| Editing | `CanEditRows`, `EditItemAsync`, `EditCellAsync`, `UpdateEditDraft`, `CommitEditAsync`, `CancelEditAsync` |

Import `Vek.Core.DataGrid` to call extensions on either adapter. Shared commands are not redefined as per-operation forwarding members in the UI classes. An unqualified call inside an adapter must use a receiver such as `this.SelectItemAsync(item)` for extension resolution. Native event handlers may translate arguments and call that shared surface.

## State and host integration

The engine owns query descriptors, column ordering, selected keys, and the edit session. Its `Interaction` state holds neutral interaction policies, selected-row projections, detail expansion, the active edit column, and access to available result scopes. Hosts synchronize native parameters and loaded data into that state. Result-scope providers are lazy so that reading a row predicate does not repeatedly filter and sort the complete source.

Scope providers describe the data available to an operation: the source, filtered results, a visible page/window, and a known remote count. They reuse engine algorithms for local data. Hosts continue to load remote data themselves; Core does not invoke providers or know renderers, controls, dispatchers, or native binding systems.

Successful mutations precede awaited host continuations. Follow-up failures propagate without rolling back or retrying the mutation. A rejected sort leaves state and callbacks unchanged. Rejected validation or persistence retains the active draft. Successful commit and cancellation clear the authoritative engine session. Drafts use a supplied clone operation or a shallow memberwise copy; applications needing deep isolation must configure `CloneItem`.

Filtering reconciles selection in Core under the configured keep-or-clear policy. Native selection enforces the same eligibility rules, and native edit completion/cancellation closes the shared session. Native columns that do not use the shared editable-template contract keep their own editing lifecycle.

Callbacks are divided by semantic concern: filtering, sorting, paging, columns, selection, details, grouping, and editing. The filtering continuation retains the existing `AfterCoreChangeAsync` contract. Other follow-up steps have default implementations so a plain host need not supply irrelevant UI behavior. UI adapters implement only their actual native work and use public implicit interface methods.

The adapters own native parameter/event declarations, lifecycle, subscriptions, templates, form validation contexts, dispatcher access, rendering, and native projections. Native column and paging properties remain native binding members. `ReloadAsync`, `RefreshServerDataAsync`, server loading, virtualization requests, row markup, CSS generation, focus, and container resizing remain adapter operations because their work belongs to the host or renderer. Core wrappers that merely call these methods would not add shared behavior.

## Verification

- Core-only tests exercise shared commands with neutral rows and template values, including page bounds, column identity, selection eligibility, expansion, rejected drafts, callback ordering, and failures.
- Blazor tests exercise the shared surface through component events, binding callbacks, row rendering, local/remote loading, editing, and virtualization.
- WPF tests exercise shared commands through native rows, columns, selection, details, sorting, and edit lifecycle integration.
- Run `dotnet test Vek.slnx` for the complete repository suite. Audit Core source and resolved references as well as tests when changing dependencies.

This command migration does not redefine native template types or rename existing presentation-only models. Any later contract consolidation must preserve strongly typed templates and follow the Core Architecture Rule.
