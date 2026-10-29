# VekDataGrid Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a reusable generic `VekDataGrid<TItem>` with property columns, client-side single-column sorting, pagination, isolated styling, and component documentation.

**Architecture:** `VekDataGrid<TItem>` owns rendering and sort/page state. Child `PropertyColumn<TItem, TValue>` components register an internal `IVekDataGridColumn<TItem>` definition with the grid; the interface supplies a typed value renderer and row comparison behavior. The Demo passes its product list into the library component and retains ownership of data retrieval.

**Tech Stack:** .NET 10, Blazor Razor Class Library, C#, Razor, isolated CSS.

**Spec:** `docs/superpowers/specs/2026-10-29-vek-data-grid-design.md`

## Global Constraints

- All projects target `net10.0`.
- `Vek.Components` remains independent from Demo, demo models, and data providers.
- Components are organized under `src/Components/Vek.Components/Components/<Feature>/`, with markup and isolated CSS together.
- Documentation is written in English under `docs/Components/`.
- Do not add test cases or run tests in this task.

## Review Focus

- **Empty item set:** render the header and an empty body without a divide-by-zero page count.
- **Page boundary:** previous/next controls stay within the available page range, including a single-page result.
- **Items shrink while on a later page:** clamp the current page to the last available page before rendering.
- **Sort changes:** first click sorts ascending; clicking the active column reverses direction; clicking a different column starts ascending and returns to page one.
- **Comparable property values:** default typed comparison handles the simple scalar property types used by the Demo; document that the initial `PropertyColumn` is intended for comparable values.

---

## File Structure

```text
src/Components/Vek.Components/Components/DataGrid/
  IVekDataGridColumn.cs
  VekDataGrid.razor
  VekDataGrid.razor.cs
  VekDataGrid.razor.css
  PropertyColumn.razor
  PropertyColumn.razor.cs
docs/Components/DataGrid.md
src/Demo/Vek.Demo/Components/Pages/Home.razor
src/Adapters/Vek.SimulatedData/products.json
```

No public base class is introduced in the first version. The internal `IVekDataGridColumn<TItem>` is the seam for future column types, without committing the package to a public inheritance API before additional column kinds exist.

### Task 1: Add the column contract and property column component

**Files:**
- Create: `src/Components/Vek.Components/Components/DataGrid/IVekDataGridColumn.cs`
- Create: `src/Components/Vek.Components/Components/DataGrid/PropertyColumn.razor`
- Create: `src/Components/Vek.Components/Components/DataGrid/PropertyColumn.razor.cs`

**Interfaces:**
- `internal interface IVekDataGridColumn<TItem>` exposes `string Title`, `RenderFragment<TItem> CellTemplate`, and `Comparison<TItem> CompareItems`.
- `PropertyColumn<TItem, TValue>` is a child Razor component with `[Parameter, EditorRequired] Expression<Func<TItem, TValue>> Property`, optional `[Parameter] string? Title`, and implements `IVekDataGridColumn<TItem>`.
- `PropertyColumn` compiles its `Property` expression when it changes, derives a default title from the final property member name, and uses `Comparer<TValue>.Default` for row comparison.
- The column receives its parent through a cascading `VekDataGrid<TItem>` and registers/unregisters its column definition with that grid. It renders no independent UI.

- [x] **Step 1: Define the internal column contract**

Create `IVekDataGridColumn<TItem>` in the DataGrid namespace with only the title, cell rendering, and row comparison members needed by the first version.

- [x] **Step 2: Implement typed property access and registration**

Implement `PropertyColumn<TItem, TValue>` with the exact parameters above. Compile the property expression once per parameter change; infer `Title` from a simple member expression when omitted; register the component with its cascading parent and unregister it when disposed.

### Task 2: Implement generic rendering, sorting, pagination, and CSS

**Files:**
- Create: `src/Components/Vek.Components/Components/DataGrid/VekDataGrid.razor`
- Create: `src/Components/Vek.Components/Components/DataGrid/VekDataGrid.razor.cs`
- Create: `src/Components/Vek.Components/Components/DataGrid/VekDataGrid.razor.css`

**Interfaces:**
- `VekDataGrid<TItem>` exposes `[Parameter, EditorRequired] IEnumerable<TItem> Items`, `[Parameter] int PageSize = 10`, `[Parameter] string? Class`, and `[Parameter] RenderFragment? ChildContent`.
- It cascades itself to child columns and exposes internal column registration methods.
- It renders columns registered through `IVekDataGridColumn<TItem>`, sorts the complete item sequence by the selected column, then applies pagination.
- Sorting is single-column and defaults to ascending on a new column; sorting the active column toggles direction and resets the page to one.
- Pagination provides a previous button, next button, and current/total page indicator. The page number is clamped when the available page count changes.

- [x] **Step 1: Render the generic table from registered columns**

Render one header per registered column and invoke each column's cell renderer per item. Render a valid empty body when no items exist. Cascade the grid instance to `ChildContent` so `PropertyColumn` can register.

- [x] **Step 2: Add single-column client-side sorting**

Sort all supplied items before slicing the current page. Use ascending order for the first click or a new column, descending on the next click of the active column, and reset to page one whenever the sort changes.

- [x] **Step 3: Add bounded pagination**

Default `PageSize` to 10. Calculate the available page count safely for empty results, clamp the current page when `Items` changes, and implement previous/next controls plus a page indicator without a size selector.

- [x] **Step 4: Add isolated minimal styling**

Style the table and pager in `VekDataGrid.razor.css`. Apply the optional `Class` parameter to the outer grid element so callers can add their own classes.

### Task 3: Demonstrate and document the component

**Files:**
- Modify: `src/Demo/Vek.Demo/Components/Pages/Home.razor`
- Modify: `src/Demo/Vek.Demo/Components/_Imports.razor` only if the component namespace is not already in scope
- Modify: `src/Adapters/Vek.SimulatedData/products.json`
- Create: `docs/Components/DataGrid.md`

- [x] **Step 1: Replace the sample product HTML table with `VekDataGrid`**

Pass the existing product list to `VekDataGrid<DemoProduct>` and declare Name, Category, and Price columns using `PropertyColumn` expressions. Keep product loading in Demo.

Expand the deterministic JSON sample to 500 products and use a Demo page size of 10 so the pager shows 50 pages; the component's default remains 10.

- [x] **Step 2: Document the initial API and its limits**

Add an English usage guide with the generic grid and property-column syntax, `Items`, `PageSize`, optional `Title`, `Class`, sorting behavior, pagination behavior, and the first-version scope limits.

### Task 4: Build-check the solution

- [x] Run `dotnet build Vek.slnx -c Release`.
- [x] Confirm the component library and Demo compile together. Do not run `dotnet test`.

Build result: `dotnet build Vek.slnx -c Release` completed with 0 errors and 0 warnings. No tests were added or run, as scoped in the approved design.
