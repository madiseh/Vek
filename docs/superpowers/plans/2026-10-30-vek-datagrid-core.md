# Vek DataGrid Core Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Move platform-neutral DataGrid state and behavior into `Vek.Core.DataGrid`, connect the Blazor and WPF adapters, and document the rule for future multi-UI component families.

**Architecture:** A `net10.0` Core project owns per-grid state, common local operations, state snapshots, and neutral server/edit contracts. Blazor and WPF reference Core and translate platform input/output at their boundaries. Host callbacks execute remote data and persistence operations.

**Tech Stack:** .NET 10, C#, Razor Class Library, WPF, xUnit/bUnit projects present in solution.

**Spec:** `docs/superpowers/specs/2026-10-30-vek-datagrid-core-design.md`

## Global Constraints

- Core targets `net10.0` and has no references to Blazor, WPF, Demo, or sample-data projects.
- WPF remains `net10.0-windows`; Blazor remains a Razor Class Library.
- Dependencies point from adapters to Core.
- Common interface, property, method, and state names have one canonical definition in Core and match across Blazor and WPF adapters.
- Preserve the current Blazor component parameters/callbacks and WPF control API where feasible.
- Keep Razor rendering, templates, styles, WPF controls, and platform-specific behavior in their adapters.
- Remote query and mutation execution stay with the host; server load callbacks accept a `CancellationToken`.
- Do not create commits during this implementation unless requested.

## Review Focus

- No Blazor-specific types such as `EditContext`, `EventCallback`, or `RenderFragment` leak into Core.
- Filter, sort, grouping, paging, and selection behavior remains coherent after commands and state restoration.
- Server load results do not overwrite newer state after cancellation or overlapping requests.
- Grid instances do not share mutable state.
- Adapter project references do not introduce a reverse dependency from Core.

---

### Task 1: Implement the Core contracts and per-grid engine

**Files:**
- Create Core domain files under `src/Components/Core/Vek.Core.DataGrid/`.
- Modify `src/Components/Core/Vek.Core.DataGrid/Vek.Core.DataGrid.csproj` only if packaging metadata or global usings are needed.
- Add a project-level `GlobalUsings.cs` if common namespaces warrant it.

**Interfaces:**
- Provide stable-key column metadata with a value accessor and common-operation capabilities.
- Provide immutable sort/filter/group/page descriptors, an importable/exportable neutral state snapshot, a server request/result contract, and a per-grid controller/engine.
- The controller exposes state transitions and derived rows/groups/aggregates for local data; it has no framework lifecycle or I/O dependencies.
- Keep edit validation and persistence behind neutral validator/commit seams.
- Extend `tests/Components/Vek.Components.Tests/Vek.Components.Tests.csproj` to reference Core and place engine specifications under `tests/Components/Vek.Components.Tests/DataGridCore/`.

- [x] Add XML-documented tests for stable sort/filter/page composition, independent state per controller, state export/import, group/aggregate calculation, and edit commit/cancel transitions; run each focused test to see the expected failure.
- [x] Implement contracts and the controller for sort, filter, page, selection, grouping, aggregation, edit lifecycle, and state restore/export.
- [x] Define one generic `IVekDataGridColumn<TItem, TTemplate>` contract in Core and use it directly in both adapters; preserve each platform's template type through typed `VekDataGridTemplate<TContext, TTemplate>` slots.
- [x] Build `Vek.Core.DataGrid` through the Blazor and WPF adapter builds with no UI framework dependency in Core.
- [x] Rerun focused Core tests and the full Vek.Components.Tests suite after the nested grouping, filter, and sort interaction changes.

### Task 2: Move Blazor behavior behind the Core engine

**Files:**
- Modify `src/Components/Blazor/Vek.Blazor.DataGrid/Components/DataGrid/VekDataGrid.razor.cs`.
- Modify `src/Components/Blazor/Vek.Blazor.DataGrid/Components/DataGrid/VekDataGridState.cs` and column components as needed.
- Modify `src/Components/Blazor/Vek.Blazor.DataGrid/Vek.Blazor.DataGrid.csproj` and its import files to reference/use Core.

**Interfaces:**
- Keep existing Razor parameters, callbacks, markup, and UI context types as the compatibility facade.
- The component creates one Core controller per component instance, maps parameters/commands into Core, and renders Core results.
- The adapter uses the same common state and operation names as Core; it does not keep parallel sorting/filtering/paging/grouping/editing calculations.
- RenderFragments, EventCallbacks, EditContext, CSS, and browser-facing behavior stay in the Blazor assembly.

- [x] Existing DataGrid tests exercise observable sorting, filtering, paging, selection, grouping, and editing behavior against the Core-backed component.
- [x] Replace component-owned sorting, filtering, paging, grouping, aggregate, selection state transition, and edit session calculations with Core calls.
- [x] Keep server load invocation in the component/host boundary; derive its neutral state from Core and preserve cancellation.
- [x] Run all Blazor DataGrid tests, the full component test suite, and build the Demo project after the latest changes.

### Task 3: Connect the WPF adapter to Core

**Files:**
- Modify `src/Components/WPF/Vek.WPF.DataGrid/Components/VekDataGrid.cs`.
- Modify `src/Components/WPF/Vek.WPF.DataGrid/Vek.WPF.DataGrid.csproj`.
- Update WPF DataGrid demo bindings only where required to exercise the adapter boundary.

**Interfaces:**
- Preserve `VekDataGrid` as a WPF `DataGrid`-based public control.
- Create a per-control Core controller and map WPF sorting, filtering, paging, selection, grouping, and edit lifecycle interactions to Core where those features are exposed by this control.
- Reuse Core's common interface and operation names; WPF-specific event arguments stay at the control boundary.
- WPF templates, native editors, commands/gestures, styling, and visual virtualization remain native.

- [x] Add XML-documented WPF adapter coverage for per-grid engine isolation and native sort mapping.
- [x] Reference Core from WPF and route native sort state through the engine.
- [x] Keep remote data and persistence callbacks owned by the WPF host.
- [x] Route native sorting and selection through Core, register the shared generic column contract as a native WPF template column, and verify cell/edit template selection in WPF.

### Task 4: Record the multi-UI component rule

**Files:**
- Modify `docs/1.governance/architecture.md`.
- Modify `AGENTS.md`.
- Update `docs/Components/DataGrid.md` to describe the Core and adapter responsibilities.
- Update the older DataGrid feature design and plan references only where they state that all reusable behavior must remain in Blazor.

- [x] Document the dependency direction, project locations, and single-UI exception in the architecture guide.
- [x] Add a concise mandatory rule and architecture-guide link in `AGENTS.md`.
- [x] Build the full solution with `dotnet build Vek.slnx`.
- [x] Run the complete solution test suite with `dotnet test Vek.slnx`.

## Execution Notes

- Execute this plan in the current clean checkout, in order, with one implementation pass per task.
- Existing automated test projects are present and must follow the repository's TDD and XML-summary conventions.
- Do not commit changes during execution.
