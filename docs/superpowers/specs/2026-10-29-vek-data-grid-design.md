# VekDataGrid Feature Expansion Design

## Intent

Expand `VekDataGrid<TItem>` into a reusable, feature-rich Blazor data grid in `Vek.Components`. The goal is to make the component useful for small in-memory collections and large or remotely loaded datasets while keeping the API and implementation native to this project. The Demo documentation should let users explore each major feature family through a live example and its source.

## Goals

- Provide the complete documented data-grid capability set identified during research: column definitions and templates; single and multiple sorting; filtering modes and quick filtering; paging; row selection; cell, row-inline, and form editing; validation and edit lifecycle callbacks; multi-level grouping; aggregation; hierarchical row details; server-side data operations; item and server virtualization; column visibility, options, sizing, resizing, and reordering; customizable rendering and empty/loading states; context actions; culture-aware formatting; and accessible, responsive behavior.
- Keep all reusable behavior and contracts in `Vek.Components`; keep Demo models, sample data, and routes in their existing projects and dependency directions.
- Give each major feature family its own Demo subpage. Each page uses the existing `DemoProduct` sample model and follows the current documentation-page pattern: explanation, interactive grid, and Razor/code-behind sample tabs.
- Add automated tests for each observable feature before production implementation and keep the focused behavior and its Demo page together in a separate commit.
- Keep the component independent of external grid libraries and their APIs, namespaces, packages, markup, and copied implementation.

## Non-goals

- Building a .NET MAUI host or MAUI-specific implementation/testing in this work.
- Adding non-grid controls from a broader component library.
- Changing the existing Demo product-data ports or the hexagonal project boundaries except where a demonstrated grid feature requires a narrowly scoped change; any such need should be surfaced before widening the work.
- Reproducing another library's branding, source code, or API names. The feature list is a behavioral reference; public names and implementation belong to Vek.

## Constraints

- Target framework remains .NET 10.
- `Vek.Components` remains independent of Demo and sample-data projects.
- Keep the existing Razor Class Library, Razor import, C# global using, and isolated-style conventions.
- Every feature commit includes the reusable component change, its automated behavior tests, and its corresponding Demo subpage. Use test-first implementation and run the relevant tests before each commit.
- Work through the approved task list serially without asking for approval after each feature. Commit each completed task independently. If a task exposes a blocking failure or a scope decision that cannot be made safely from this design, resolve it or ask about that specific blocker before proceeding.
- No project file, source file, test, page, documentation, task-list entry, or commit title/body may contain the excluded library's name or depend on its package/API.

## Approaches considered

1. **Extend the current implementation incrementally.** This is a smaller initial change because the component already sorts and pages local items. However, the current implementation has no shared grid state, filter definitions, server-data contract, editing lifecycle, or column-management model, so broad features would grow on top of a narrow base and could require repeated structural rewrites.
2. **Establish a Vek-native grid foundation, then add feature families in slices.** Define the internal column, state, data-source, and event contracts needed by the full feature set, preserving the existing basic behavior; then implement capabilities in dependency order. This has more upfront design work but gives the broad feature set one coherent base and supports independent, reviewable commits.

**Selected:** Approach 2. The user chose a Vek-native foundation followed by feature-family implementation.

## Feature families and Demo pages

Page names below are provisional route/folder names for the implementation plan; the builder may adjust naming to match repository conventions without changing scope.

1. **Basic usage and columns** — baseline grid setup, property and template columns, formatting, per-column capabilities, and basic customization. Existing `BasicUsage` page is the starting point.
2. **Sorting** — disabled/single/multiple sorting, sort state, custom sort selectors/comparers, and clearing sort.
3. **Filtering** — quick and per-column filtering, supported filter modes, case sensitivity, filter templates, and filter lifecycle.
4. **Paging** — current page, page size, pager content, totals, and paging state changes.
5. **Selection** — single/multiple selection, row-click and checkbox interaction, disabled rows, comparer behavior, and selection callbacks.
6. **Editing and validation** — cell, inline-row, and form editing; custom edit templates; commit/cancel; validation; and edit lifecycle callbacks.
7. **Grouping and aggregation** — nested grouping, group templates and expansion state, group summaries, built-in and custom aggregates, and footer templates.
8. **Hierarchical row details** — expandable child content, custom detail rendering, initial expansion, and expand/collapse controls.
9. **Server data** — asynchronous data loading with sort, filter, and paging state; cancellation, loading/error/empty feedback, refresh, and total counts.
10. **Virtualization** — client-side virtualization and server-backed virtual loading with bounded rendering and scroll behavior.
11. **Column management** — visibility, options, column panel, column ordering, and drag/drop feedback.
12. **Column layout and responsive behavior** — column groups and sizing, interactive resizing, density/borders/striping, fixed headers, and narrow-screen behavior.
13. **Templates and presentation** — toolbar/header/cell/footer/row templates and styles, custom filter content, empty/loading states, culture/formatting, and keyboard/screen-reader accessibility.
14. **Context actions and observability** — cell/row context actions, collection-change observation, and public commands/state access needed to control the grid.

The task plan should split a family further only when needed to keep one commit focused and testable. Any split must keep each commit independently buildable and must not create a page for every minor parameter.

## Workflow and acceptance

- Start from the current grid and its existing tests/demo conventions.
- Before each production change, add a focused automated test that describes an observable behavior and run it to confirm the expected failure.
- Implement the smallest slice, rerun the focused test, then run the relevant test suite and Demo build/checks selected in the task plan.
- Update the feature's Demo subpage with a live example and both code views in the same slice.
- Review the diff for scope, accessibility, responsive layout, and accidental external-library traces. Commit only after the slice's checks pass.
- Keep a task list in the repository with ordered tasks, dependencies, acceptance checks, and a concise intended commit subject for each task. Execute the approved list end-to-end without per-task pauses.

## Left to the builder

- Choose Vek-owned names and shapes for public types, parameters, callbacks, and state objects.
- Determine the smallest shared foundation that supports the listed behavior without adding unnecessary abstractions.
- Choose representative `DemoProduct` fields/data variations that make each example meaningful while avoiding project-boundary changes unless technically required.
- Identify capability conflicts and document compatibility rules in the component API and tests.
- Preserve the existing behavior of `VekDataGrid` unless a documented compatibility change is required by the new design.
