# VekDataGrid Design

## Intent

Build Vek's own reusable Blazor data grid, named `VekDataGrid`, so the component library can display different model types with client-side sorting and pagination.

## Goals

- Make the grid generic so callers can supply any row type.
- Define simple columns declaratively in Razor with a strongly typed `PropertyColumn` expression such as `item => item.Name`.
- Allow an optional column `Title`; when omitted, derive the title from the selected property name.
- Sort by one column at a time. Clicking its header toggles ascending and descending order; selecting another column replaces the current sort.
- Return to the first page when the active sort changes.
- Accept the complete item collection and perform sorting and pagination in the component.
- Expose a configurable page size with a default of 10, and provide previous/next controls plus a page indicator.
- Keep the initial visual style simple, use isolated CSS, and expose a `Class` parameter for caller styling.
- Add `docs/Components/DataGrid.md` as the component's usage documentation and demonstrate the grid with the existing Demo product data.
- Keep `Vek.Components` independent from Demo, demo models, and sample-data projects.

## Non-goals for the first version

- Server-side data loading or paging.
- Multi-column sorting, filtering, selection, editing, virtualization, or column reordering.
- Custom cell templates or non-property/template columns.
- A page-size selector.
- Adding test cases or running tests in this task.

## Constraints

- All projects target .NET 10 (`net10.0`).
- Follow the existing component organization: feature folder under `Vek.Components/Components/`, with Razor markup and isolated CSS together.
- Keep ports and contracts independent of adapters; the grid receives data through parameters and performs no data access.
- Keep documentation in English, consistent with the existing repository documentation.

## Approaches considered

### 1. Razor child column components (selected)

Declare `PropertyColumn` elements inside `VekDataGrid` and bind each to a typed property expression. This keeps grid usage readable in Razor, gives callers compile-time property checking, and leaves a natural extension point for future column component types while keeping the first version small.

### 2. C# column descriptor collection

Pass a list of column descriptor objects to the grid. This can make the rendering implementation direct, but moves table structure out of Razor and makes ordinary usage less declarative. It was not selected.

## Agreed behavior

`VekDataGrid<TItem>` is a custom, reusable component. It receives all rows through `Items`; sorting and pagination happen locally. Columns use `PropertyColumn` and a typed expression. Sorting is single-column, toggles ascending/descending, and resets pagination to page one when changed. `PageSize` defaults to 10 and is configurable; the pager has previous/next controls and a page indicator. Column titles default to the property name and can be overridden. Initial styling is simple and isolated, with a `Class` parameter.

## Implementation calls

- Use the existing Demo product table as the usage example, while keeping `VekDataGrid` generic and the component library independent from Demo types.
- Keep advanced features such as custom templates and server paging as future extensions rather than adding speculative APIs now.
