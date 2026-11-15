# Migration Specification: 001 - DataGrid Search Panel & Live Highlighting

## 1. Metadata
- **Feature ID**: 001
- **Feature Slug**: datagrid-search-panel
- **Module**: DataGrid
- **Status**: Completed
- **Pull Request**: https://github.com/madiseh/Vek/pull/5

---

## 2. Behavioral Scope & Requirements
- **ShowSearchBox**: Boolean flag toggling the search input in the DataGrid toolbar header.
- **SearchText**: Two-way bindable string property for active search query.
- **SearchTextParseMode**:
  - `GroupWordsByAnd` (default): Whitespace-separated words; each word must match at least one searchable column.
  - `GroupWordsByOr`: Whitespace-separated words; any word matching any searchable column matches the row.
  - `ExactMatch`: Exact literal phrase match across searchable columns.
  - Quoted phrases (`"first last"`) treated as single tokens.
- **SearchBoxInputDelay**: Debounce interval in milliseconds (default: 300ms).
- **SearchBoxPlaceholder**: Customizable placeholder text for search input.
- **SearchBoxTemplate**: RenderFragment slot providing `DataGridSearchBoxContext<T>` for custom search inputs.
- **AllowSearch**: Column-level toggle (`Column<T>`) controlling search participation (default: true).
- **HighlightSearchResults**: Toggleable live cell text highlighting wrapping matched tokens in `<mark class="vek-table-cell-highlight">`.

---

## 3. Architectural Mapping (Vek Standards)
- **Target Location**:
  - `src/Vek/Components/DataGrid/Filtering/DataGridSearchEngine.cs`
  - `src/Vek/Components/DataGrid/Filtering/DataGridSearchTextParseMode.cs`
  - `src/Vek/Components/DataGrid/Models/DataGridSearchBoxContext.cs`
  - `src/Vek/Components/DataGrid/Columns/Column.razor.cs`
  - `src/Vek/Components/DataGrid/VekDataGrid.razor` & `.cs`
  - `src/Vek/Styles/components/_datagrid.scss`
- **State Management**: All parameters registered in constructor via `CreateRegisterScope()` and managed by `ParameterState<T>`.
- **Performance**: Zero child-component instantiation per cell during highlighting; uses `Splitter.GetFragments` and compiled non-backtracking regex.

---

## 4. Verification & Acceptance Evidence
- **Build**: Succeeded with 0 warnings and 0 errors (`TreatWarningsAsErrors=true`).
- **Tests**: 69 dedicated unit & bUnit tests passing in `DataGridSearchEngineTests.cs` and `DataGridSearchTests.cs`.
- **Formatting**: Validated with `dotnet format whitespace`.
- **Anonymity**: Zero vendor brand traces in code, commits, or documentation.
