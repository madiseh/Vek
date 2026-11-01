# Vek DataGrid Core and UI Adapter Design

## Intent

Move framework-independent DataGrid behavior and state into `Vek.Core.DataGrid`. Keep `Vek.Blazor.DataGrid` and `Vek.WPF.DataGrid` as UI adapters that provide native interaction and rendering while using the same core. Record the same rule for future component families that intentionally support more than one UI framework.

## Current State

- `Vek.Core.DataGrid` targets `net10.0` and currently contains only its project file.
- `Vek.Blazor.DataGrid` contains the full DataGrid implementation. Its Razor component code-behind owns sorting, filtering, paging, selection, grouping, aggregates, editing, server loading, virtualization, and column management; `VekDataGridState.cs` also contains UI contexts and shared-looking state types.
- `Vek.WPF.DataGrid` targets `net10.0-windows`; its `VekDataGrid` currently derives from WPF `DataGrid` without shared behavior.
- The older `2026-10-29-vek-data-grid-design.md` defines the earlier Blazor-only boundary. Its feature inventory remains useful; its project-boundary decisions are superseded by this design.

## Goals

- Put shared DataGrid state, operations, and framework-neutral contracts in `Vek.Core.DataGrid`.
- Keep a separate state instance for each rendered grid.
- Let the core handle common operations over in-memory data and create neutral requests for server-backed data.
- Model sort, filter, grouping, paging, selection, and edit lifecycle without references to Blazor, WPF, ASP.NET Core, browser storage, or database/provider packages.
- Give common DataGrid contracts one canonical set of interface, property, method, and state names in Core; Blazor and WPF must use those same names for the same concepts.
- Keep server query execution, persistence, and CRUD side effects with the host/application through callbacks or ports.
- Keep UI interaction, rendering, templates, editors, styling, native accessibility, and platform-specific features in their respective adapter projects.
- Preserve the existing Blazor and WPF public APIs as far as practical; make compatibility changes explicit when a framework-specific type cannot be part of a neutral Core contract.
- Add a repository rule: multi-UI component families share a framework-neutral Core; single-UI component families do not need one.

## Non-goals

- Making Core render controls, read browser or WPF state, call APIs/databases, persist settings, or implement platform-specific gestures.
- Forcing every existing Blazor feature into Core. A behavior belongs in Core only when it has a useful platform-neutral meaning that an adapter can consume; UI-only capabilities stay in the UI project.
- Changing Demo models or sample-data boundaries.
- Replacing native WPF controls or introducing a third-party grid dependency.

## Constraints

- Core targets `net10.0` and references no Blazor, WPF, Demo, or sample-data project.
- Blazor and WPF projects reference Core; Core never references either adapter.
- WPF remains `net10.0-windows`; Blazor remains a Razor Class Library.
- Server request and state snapshot contracts use stable column keys and neutral values, not UI framework types or LINQ expression trees.
- Server data loading is asynchronous, carries a `CancellationToken`, and returns page items plus the total matching count.
- Hosts execute remote data queries and persist or mutate application data. Core owns request/state semantics and the edit workflow; adapters/hosts provide validation integration and commit callbacks where framework-specific behavior is needed.
- Persisted grid state is exported/imported through Core contracts; the UI or host chooses the storage mechanism.

## Approaches Considered

1. **Keep the current implementations separate.** This has no migration cost but leaves behavior duplicated and prevents consistent operation semantics across WPF and Blazor.
2. **Share only DTOs and query descriptors.** This enables common server contracts but leaves sorting, filtering, state transitions, selection, grouping, and edit behavior duplicated in the adapters.
3. **Build a framework-neutral Core engine and use UI adapters.** Core owns the shared state and behavior; each adapter translates native input into Core commands and renders Core results. This has the largest initial migration cost and directly meets the agreed goal.

**Selected:** Approach 3. The user confirmed this boundary during the design interview.

## Design Decisions

### Core ownership

Core owns the per-grid state model and shared transitions/operations, including sorting, filtering, paging, selection, grouping, and the UI-independent edit lifecycle. It exposes a serializable state snapshot that can be restored. Each DataGrid creates and owns its own Core state/controller instance so grids remain independent.

Column metadata in Core contains stable keys, value access, and capabilities required by shared operations. Display templates, CSS/classes, native editor controls, and other presentation metadata stay in the adapter.

### Server data and mutations

When a grid uses remote data, Core creates a platform-neutral request containing page, page size, ordered sort descriptors, and filter descriptors (column key, operator, value, and relevant comparison options). The adapter or application invokes its data provider asynchronously with a cancellation token and returns the requested items plus the total matching count. Core does not contact the provider itself.

Core manages edit state and commit/cancel transitions. It coordinates validation through a neutral seam and calls host-provided commit/delete operations for persistence. It has no database or API knowledge.

### UI adapters

Blazor and WPF own input gestures, component/control lifecycle, rendering, templates, platform editors, styling, and UI-specific accessibility or virtualization mechanics. They translate those inputs into Core commands and render the resulting state/data. Features that exist only in one UI remain in that adapter.

Both adapters consume the same Core interfaces and common property/operation names. Their local variable names for shared concepts follow the Core contract names (for example `Items`, `Page`, `PageSize`, `Sorts`, `Filters`, `Groups`, and `SelectedKeys`). Native event arguments and rendering objects can have platform-specific names because they do not represent shared Core state.

The existing Blazor component API and WPF control API remain the compatibility surface where feasible. Existing public types that embed UI framework types stay adapter-side; Core receives neutral counterparts and adapters map between the two when needed.

### Repository rule

For a reusable component family that intentionally has multiple UI adapters, put shared state, behavior, and contracts in `<Component>.Core`, with no references from Core to any UI adapter. Core is the single source of common interface and state names; adapters use those names and own interaction/presentation. A component family with one UI adapter may keep its behavior in that UI package without creating a Core project. Detailed project layout and dependency directions belong in `docs/1.governance/architecture.md`; `AGENTS.md` carries the concise rule and points to that guide.

## Vendor References

Official vendor documentation provides useful API precedents for server operation descriptors, provider callbacks, and state persistence. It does not establish that Syncfusion or DevExpress use one framework-neutral internal engine shared between their WPF and Blazor products. MudBlazor is Blazor-only and its public DataGrid implementation combines UI and behavior. These products are references for behavior and callback design, not proof of the selected internal architecture. See `docs/research/datagrid-vendor-architecture.md`.

## Left to the Builder

- Choose Core public type names and signatures, retaining adapter-facing names through mapping/facades where that preserves compatibility.
- Inventory the Blazor feature set and move only behaviors that have a genuine neutral semantic model. Keep Blazor-only and WPF-only presentation features in their adapters.
- Define a neutral validation seam that does not expose Blazor `EditContext` or WPF validation types from Core.
- Define supported serializable filter value types and versioning behavior for state snapshots.
- Refactor in small vertical slices so each adapter can consume the same Core behavior before the next behavior family moves.
- Update the architecture guide and `AGENTS.md` after this design is approved.
