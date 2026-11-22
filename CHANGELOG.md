# Vek Changelog

All notable changes to Vek will be documented in this file.
For GitHub Releases and downloadable packages, see the [Releases page](https://github.com/madiseh/Vek/releases).

## [v1.3.3] - 2026-11-22

> Vek v1.3.3 adds seven component features: multi-level DatePicker calendar navigation, inline TreeView node renaming, modal stacking and focus isolation, richer Tabs headers and badges, vertical Slider orientation and touch scrubbing, customizable and clearable Rating items, and Card media aspect ratios. The release also includes TreeView live search, Tabs drag-and-drop reordering, and documentation improvements.

### Component Features
- **DatePicker:** Navigate between month, decade, and century views. ([PR #95](https://github.com/madiseh/Vek/pull/95))
- **TreeView:** Rename nodes inline ([PR #96](https://github.com/madiseh/Vek/pull/96)); filter labels with matching ancestors expanded ([PR #90](https://github.com/madiseh/Vek/pull/90)); load child nodes on demand ([PR #88](https://github.com/madiseh/Vek/pull/88)).
- **Dialog:** Stack modal dialogs and isolate focus in the active modal. ([PR #97](https://github.com/madiseh/Vek/pull/97))
- **Tabs:** Add badge and custom header templates ([PR #98](https://github.com/madiseh/Vek/pull/98)); reorder tabs by dragging and dropping ([PR #91](https://github.com/madiseh/Vek/pull/91)).
- **Slider:** Add vertical orientation, configurable height, inversion, and touch scrubbing. ([PR #99](https://github.com/madiseh/Vek/pull/99))
- **Rating:** Add item templates, hover and active colors, and a clearable value. ([PR #100](https://github.com/madiseh/Vek/pull/100))
- **Card:** Support aspect-ratio sizing for card media. ([PR #101](https://github.com/madiseh/Vek/pull/101))

### Documentation and Showcase
- Refine the interactive Expense Tracker showcase. ([PR #92](https://github.com/madiseh/Vek/pull/92))
- Improve DataGrid API sitemap coverage. ([PR #86](https://github.com/madiseh/Vek/pull/86))

### Contributors
- [@madiseh](https://github.com/madiseh) — [PR #86](https://github.com/madiseh/Vek/pull/86), [#88](https://github.com/madiseh/Vek/pull/88), [#90](https://github.com/madiseh/Vek/pull/90), [#91](https://github.com/madiseh/Vek/pull/91), [#92](https://github.com/madiseh/Vek/pull/92), [#93](https://github.com/madiseh/Vek/pull/93), [#94](https://github.com/madiseh/Vek/pull/94), [#95](https://github.com/madiseh/Vek/pull/95), [#96](https://github.com/madiseh/Vek/pull/96), [#97](https://github.com/madiseh/Vek/pull/97), [#98](https://github.com/madiseh/Vek/pull/98), [#99](https://github.com/madiseh/Vek/pull/99), [#100](https://github.com/madiseh/Vek/pull/100), [#101](https://github.com/madiseh/Vek/pull/101)

### Test Results
- Total: 8,658 — 8,656 passed, 0 failed, 2 skipped.
- `Vek.UnitTests`: 6,733 total — 6,731 passed, 0 failed, 2 skipped.
- `Vek.UnitTests.Analyzers`: 391 total — 391 passed, 0 failed, 0 skipped.
- `Vek.UnitTests.Docs`: 1,534 total — 1,534 passed, 0 failed, 0 skipped.
- Format check passed; build-only projects passed: `src/Docs/Vek.Docs.Server` and `src/Docs/Vek.Docs.WasmHost`.
- Source commit: `a6f7b8ca4acba874eea0b4a980d8e10965d5d4cc`; [CI run 45017258015](https://github.com/madiseh/Vek/actions/runs/45017258015).
- [Download JUnit reports](https://github.com/madiseh/Vek/releases/download/v1.3.3/test-results-v1.3.3.zip).

[Feature screenshots](https://github.com/madiseh/Vek/releases/download/v1.3.3/release-slideshow.gif): TreeView search (#90), Tabs reordering (#91), DatePicker navigation (#95), TreeView renaming (#96), Dialog stacking (#97), Tabs headers (#98), vertical Slider (#99), Rating templates (#100), and Card aspect ratio (#101).

**Full Changelog**: https://github.com/madiseh/Vek/compare/v1.3.2...v1.3.3

## [v1.3.2] - 2026-11-21

> Vek v1.3.2 delivers 13 new component features across DataGrid, TreeView, TimePicker, DatePicker, Tabs, Button, Dialog, Slider, TextField, and MaskedInput, alongside an interactive Expense Tracker showcase and documentation enhancements.

### ✨ Component Features
- **ButtonGroup**: Added single, multiple, and none selection modes. ([PR #68](https://github.com/madiseh/Vek/pull/68))
- **Dialog**: Added draggable modal dialogs and resizable dialog interactions. ([PR #69](https://github.com/madiseh/Vek/pull/69))
- **TextField**: Added custom editor action buttons and embedded adornment controls. ([PR #70](https://github.com/madiseh/Vek/pull/70))
- **Slider**: Added dual-thumb range selection with interval binding. ([PR #71](https://github.com/madiseh/Vek/pull/71))
- **Tabs**: Added lazy rendering mode with DOM caching. ([PR #72](https://github.com/madiseh/Vek/pull/72))
- **Button**: Added badge adornments, status dots, and counter badges. ([PR #73](https://github.com/madiseh/Vek/pull/73))
- **DataGrid**: Added visual filter builder dialog with nested condition logic. ([PR #77](https://github.com/madiseh/Vek/pull/77))
- **MaskedInput**: Added pattern rules, prompt characters, and literal preservation. ([PR #78](https://github.com/madiseh/Vek/pull/78))
- **Tabs**: Added vertical orientation and tab strip positioning. ([PR #79](https://github.com/madiseh/Vek/pull/79))
- **DatePicker**: Added embedded inline calendar display mode. ([PR #80](https://github.com/madiseh/Vek/pull/80))
- **TimePicker**: Added interactive radial clock face and selector. ([PR #81](https://github.com/madiseh/Vek/pull/81))
- **DataGrid**: Added context menu actions and right-click cell hooks. ([PR #83](https://github.com/madiseh/Vek/pull/83))
- **TreeView**: Added on-demand asynchronous child node loading. ([PR #88](https://github.com/madiseh/Vek/pull/88))

### 🌐 Documentation, Showcases & Developer Tools
- Added an interactive Expense Tracker showcase application at `/showcases/expense-tracker` and split documentation into Docs, Landing, and Showcase modules. ([PR #89](https://github.com/madiseh/Vek/pull/89))
- Replaced legacy brand glyph with the modern Vek brand icon. ([PR #75](https://github.com/madiseh/Vek/pull/75))
- Added universal SEO analysis skill. ([PR #76](https://github.com/madiseh/Vek/pull/76))
- Added consumer component adoption skill and migration workflows. ([PR #82](https://github.com/madiseh/Vek/pull/82))
- Updated NuGet package project URL and website links. ([PR #84](https://github.com/madiseh/Vek/pull/84))
- Improved technical SEO metadata and documentation sitemap coverage. ([PR #85](https://github.com/madiseh/Vek/pull/85), [PR #86](https://github.com/madiseh/Vek/pull/86))
- Added release contributors section automation in release skill guidelines. ([PR #87](https://github.com/madiseh/Vek/pull/87))

### 🖼️ Feature Screenshots
![Vek v1.3.2 component feature screenshots](https://github.com/madiseh/Vek/releases/download/v1.3.2/release-slideshow.gif)

1. ButtonGroup selection modes — [PR #68](https://github.com/madiseh/Vek/pull/68)
2. Dialog dragging and resizing — [PR #69](https://github.com/madiseh/Vek/pull/69)
3. TextField custom action buttons — [PR #70](https://github.com/madiseh/Vek/pull/70)
4. Slider dual-thumb range selection — [PR #71](https://github.com/madiseh/Vek/pull/71)
5. Tabs lazy rendering mode — [PR #72](https://github.com/madiseh/Vek/pull/72)
6. Button badge adornments — [PR #73](https://github.com/madiseh/Vek/pull/73)
7. DataGrid filter builder dialog — [PR #77](https://github.com/madiseh/Vek/pull/77)
8. MaskedInput pattern rules & literals — [PR #78](https://github.com/madiseh/Vek/pull/78)
9. Tabs vertical orientation & positioning — [PR #79](https://github.com/madiseh/Vek/pull/79)
10. DatePicker inline calendar mode — [PR #80](https://github.com/madiseh/Vek/pull/80)
11. TimePicker interactive radial clock face — [PR #81](https://github.com/madiseh/Vek/pull/81)
12. DataGrid context menu actions — [PR #83](https://github.com/madiseh/Vek/pull/83)
13. TreeView on-demand async child loading — [PR #88](https://github.com/madiseh/Vek/pull/88)

### 👥 Contributors
- @madiseh ([#68](https://github.com/madiseh/Vek/pull/68), [#69](https://github.com/madiseh/Vek/pull/69), [#70](https://github.com/madiseh/Vek/pull/70), [#71](https://github.com/madiseh/Vek/pull/71), [#72](https://github.com/madiseh/Vek/pull/72), [#73](https://github.com/madiseh/Vek/pull/73), [#75](https://github.com/madiseh/Vek/pull/75), [#76](https://github.com/madiseh/Vek/pull/76), [#77](https://github.com/madiseh/Vek/pull/77), [#78](https://github.com/madiseh/Vek/pull/78), [#79](https://github.com/madiseh/Vek/pull/79), [#80](https://github.com/madiseh/Vek/pull/80), [#81](https://github.com/madiseh/Vek/pull/81), [#82](https://github.com/madiseh/Vek/pull/82), [#83](https://github.com/madiseh/Vek/pull/83), [#84](https://github.com/madiseh/Vek/pull/84), [#85](https://github.com/madiseh/Vek/pull/85), [#86](https://github.com/madiseh/Vek/pull/86), [#87](https://github.com/madiseh/Vek/pull/87), [#88](https://github.com/madiseh/Vek/pull/88), [#89](https://github.com/madiseh/Vek/pull/89))

### ✅ Test Results
- Unit tests: 6,718 passed, 2 skipped, 0 failed (6,720 total).
- Documentation tests: 1,528 passed, 0 skipped, 0 failed.
- Analyzer tests: 391 passed, 0 skipped, 0 failed.
- Format check: passed.
- Build-only projects: 2 passed.
- [Download the JUnit test reports](https://github.com/madiseh/Vek/releases/download/v1.3.2/test-results-v1.3.2.zip) from [candidate CI run 44906144181](https://github.com/madiseh/Vek/actions/runs/44906144181), for commit `fe041384d3f83f84e2bcd5ec081decb519becef4`.

## [v1.3.1] - 2026-11-20

> Vek v1.3.1 reorganizes the repository project structure, centralizes NuGet dependency management, refreshes documentation branding and contributor guidelines, and removes obsolete integrations.

### 🏗️ Architecture & Project Structure
- Reorganized repository layout into an architecture-aligned root solution, `src/`, and `tests/` structure. ([PR #66](https://github.com/madiseh/Vek/pull/66))
- Centralized NuGet package version management via root `Directory.Packages.props`. ([PR #67](https://github.com/madiseh/Vek/pull/67))

### 🌐 Documentation & Branding
- Refreshed documentation branding, rewritten getting started and support guides, and updated contributor guidelines. ([PR #63](https://github.com/madiseh/Vek/pull/63))
- Removed Carbon Ads integration and ad-block detection. ([PR #65](https://github.com/madiseh/Vek/pull/65))

### ✅ Test Results
- Unit tests: 6,568 passed, 2 skipped, 0 failed (6,570 total).
- Documentation tests: 1,493 passed, 0 skipped, 0 failed.
- Analyzer tests: 391 passed, 0 skipped, 0 failed.
- Format check: passed.
- Build-only projects: 2 passed.
- [Download the JUnit test reports](https://github.com/madiseh/Vek/releases/download/v1.3.1/test-results-v1.3.1.zip) from [candidate CI run 44721941602](https://github.com/madiseh/Vek/actions/runs/44721941602), for commit `45d7e7912c5795955faa6a1e6c9b2bdd4e382a32`.

## [v1.3.0] - 2026-11-20

> Vek v1.3.0 introduces 17 new component features across input, selection, navigation, display, and layout, along with a modern Bento-style documentation overview redesign.

### ✨ Component Features
- Added split button and dropdown button components with action menus. ([PR #45](https://github.com/madiseh/Vek/pull/45))
- Added multi-selection with removable chip tags in Select. ([PR #46](https://github.com/madiseh/Vek/pull/46))
- Added click debounce and double-click protection to Button. ([PR #47](https://github.com/madiseh/Vek/pull/47))
- Added scale tick marks and step snapping to Slider. ([PR #48](https://github.com/madiseh/Vek/pull/48))
- Added configurable text value binding modes (OnInput, OnBlur, OnEnter, Delayed) to TextField. ([PR #49](https://github.com/madiseh/Vek/pull/49))
- Added password masking with visibility toggle button in TextField. ([PR #51](https://github.com/madiseh/Vek/pull/51))
- Added collapsible card body toggle to Card. ([PR #52](https://github.com/madiseh/Vek/pull/52))
- Added dual calendar date range selection and preset ranges to DatePicker. ([PR #53](https://github.com/madiseh/Vek/pull/53))
- Added half-star precision and hover preview to Rating. ([PR #54](https://github.com/madiseh/Vek/pull/54))
- Added bounded resizable panes and one-click collapse toggles to SplitPanel. ([PR #55](https://github.com/madiseh/Vek/pull/55))
- Added async action button execution and undo callbacks to Snackbar. ([PR #56](https://github.com/madiseh/Vek/pull/56))
- Added logical status dot indicators and capped counter positions (99+) to Badge. ([PR #57](https://github.com/madiseh/Vek/pull/57))
- Added card placeholders and synchronized shimmer animations to Skeleton. ([PR #58](https://github.com/madiseh/Vek/pull/58))
- Added page size selector and quick jump controls to Pagination. ([PR #59](https://github.com/madiseh/Vek/pull/59))
- Added closable tabs and closing lifecycle events to Tabs. ([PR #60](https://github.com/madiseh/Vek/pull/60))
- Added configurable minute/second intervals and 12/24-hour formats to TimePicker. ([PR #61](https://github.com/madiseh/Vek/pull/61))
- Added drag-and-drop node reordering and hierarchical reparenting to TreeView. ([PR #64](https://github.com/madiseh/Vek/pull/64))

### 🌐 Documentation & Showcase
- Redesigned the Overview component directory with a modern Bento showcase, categorized cards, and live search. ([PR #50](https://github.com/madiseh/Vek/pull/50))

### 🖼️ Feature Screenshots
![Vek v1.3.0 component feature screenshots](https://github.com/madiseh/Vek/releases/download/v1.3.0/release-slideshow.gif)

1. SplitButton & DropDownButton action menus — [PR #45](https://github.com/madiseh/Vek/pull/45)
2. Select multi-selection chip tags — [PR #46](https://github.com/madiseh/Vek/pull/46)
3. Button click debounce & double-click protection — [PR #47](https://github.com/madiseh/Vek/pull/47)
4. Slider scale ticks & step snapping — [PR #48](https://github.com/madiseh/Vek/pull/48)
5. TextField value binding modes — [PR #49](https://github.com/madiseh/Vek/pull/49)
6. TextField password visibility toggle — [PR #51](https://github.com/madiseh/Vek/pull/51)
7. Card collapsible body — [PR #52](https://github.com/madiseh/Vek/pull/52)
8. DatePicker dual range selection & presets — [PR #53](https://github.com/madiseh/Vek/pull/53)
9. Rating half-star precision & hover preview — [PR #54](https://github.com/madiseh/Vek/pull/54)
10. SplitPanel resizable panes & collapse — [PR #55](https://github.com/madiseh/Vek/pull/55)
11. Snackbar async action button & undo callback — [PR #56](https://github.com/madiseh/Vek/pull/56)
12. Badge status dots & capped counter positions — [PR #57](https://github.com/madiseh/Vek/pull/57)
13. Skeleton card placeholders & shimmer animation — [PR #58](https://github.com/madiseh/Vek/pull/58)
14. Pagination page size selector & quick jump — [PR #59](https://github.com/madiseh/Vek/pull/59)
15. Tabs closable tabs & closing lifecycle events — [PR #60](https://github.com/madiseh/Vek/pull/60)
16. TimePicker intervals & 12/24-hour formats — [PR #61](https://github.com/madiseh/Vek/pull/61)
17. TreeView drag-and-drop node reordering — [PR #64](https://github.com/madiseh/Vek/pull/64)

### ✅ Test Results
- Unit tests: 6,568 passed, 2 skipped, 0 failed (6,570 total).
- Documentation tests: 1,500 passed, 0 skipped, 0 failed.
- Analyzer tests: 391 passed, 0 skipped, 0 failed.
- Format check: passed.
- Build-only projects: 2 passed.
- [Download the JUnit test reports](https://github.com/madiseh/Vek/releases/download/v1.3.0/test-results-v1.3.0.zip) from [candidate CI run 44710092127](https://github.com/madiseh/Vek/actions/runs/44710092127), for commit `2be9e6b58d091977e112f8a590dc527a1beebc35`.

---

## [v1.2.1] - 2026-11-19

> Vek v1.2.1 centralizes release version resolution from build configuration and updates documentation version labels. The Vek component API is unchanged.

### 🌐 Documentation & Versioning
- Display the current package version dynamically from `Metadata.Version` in documentation navigation and hero badges. ([PR #44](https://github.com/madiseh/Vek/pull/44))
- Enforce package version alignment with `VersionPrefix` in build and release workflows. ([PR #44](https://github.com/madiseh/Vek/pull/44))

### ✅ Test Results
- Unit tests: 6,457 passed, 2 skipped, 0 failed (6,459 total).
- Documentation tests: 1,469 passed, 0 skipped, 0 failed.
- Analyzer tests: 391 passed, 0 skipped, 0 failed.
- [Download the JUnit test reports](https://github.com/madiseh/Vek/releases/download/v1.2.1/test-results-v1.2.1.zip) from [candidate CI run 44597121943](https://github.com/madiseh/Vek/actions/runs/44597121943), for commit `b34da4f4d7608529cd6b85ffcf61441116452828`.

---

## [v1.2.0] - 2026-11-19

> Vek v1.2.0 adds new input, navigation, selection, and layout options across core Blazor components.

### ✨ Component Features
- Added configurable TextField clear button display modes. ([PR #28](https://github.com/madiseh/Vek/pull/28))
- Added loading states that prevent button activation while work is in progress. ([PR #29](https://github.com/madiseh/Vek/pull/29))
- Added disabled-date predicates and custom day templates to DatePicker. ([PR #30](https://github.com/madiseh/Vek/pull/30))
- Added cascading tri-state checkbox selection to TreeView. ([PR #31](https://github.com/madiseh/Vek/pull/31))
- Added Autocomplete virtualization, match highlighting, and custom values. ([PR #32](https://github.com/madiseh/Vek/pull/32))
- Added accordion expansion modes and icon placement options. ([PR #33](https://github.com/madiseh/Vek/pull/33))
- Added NumericField spin buttons and opt-in mouse-wheel stepping. ([PR #34](https://github.com/madiseh/Vek/pull/34))
- Added configurable Tabs scrolling and navigation buttons. ([PR #35](https://github.com/madiseh/Vek/pull/35))
- Added selector or element-reference targets and keyboard/touch triggers for context menus. ([PR #37](https://github.com/madiseh/Vek/pull/37))
- Added adaptive ToolBar overflow menus for constrained layouts. ([PR #38](https://github.com/madiseh/Vek/pull/38))

### 🖼️ Feature Screenshots
![Vek v1.2.0 component feature screenshots](https://github.com/madiseh/Vek/releases/download/v1.2.0/release-slideshow.gif)

1. TextField clear button modes — [PR #28](https://github.com/madiseh/Vek/pull/28)
2. Button loading state — [PR #29](https://github.com/madiseh/Vek/pull/29)
3. DatePicker disabled dates and day templates — [PR #30](https://github.com/madiseh/Vek/pull/30)
4. TreeView tri-state selection — [PR #31](https://github.com/madiseh/Vek/pull/31)
5. Autocomplete virtualization and custom values — [PR #32](https://github.com/madiseh/Vek/pull/32)
6. ExpansionPanel accordion modes — [PR #33](https://github.com/madiseh/Vek/pull/33)
7. NumericField spin buttons and wheel stepping — [PR #34](https://github.com/madiseh/Vek/pull/34)
8. Tabs scrolling and navigation — [PR #35](https://github.com/madiseh/Vek/pull/35)
9. ContextMenu targets and triggers — [PR #37](https://github.com/madiseh/Vek/pull/37)
10. ToolBar adaptive overflow — [PR #38](https://github.com/madiseh/Vek/pull/38)

### ✅ Test Results
- Unit tests: 6,457 passed, 2 skipped, 0 failed (6,459 total).
- Documentation tests: 1,469 passed, 0 skipped, 0 failed.
- Analyzer tests: 391 passed, 0 skipped, 0 failed.
- [Download the JUnit test reports](https://github.com/madiseh/Vek/releases/download/v1.2.0/test-results-v1.2.0.zip) from [candidate CI run 44585554333](https://github.com/madiseh/Vek/actions/runs/44585554333), for commit `ff0d02ce77989d3d630a42ae4589abd4974ba508`.

---

## [v1.1.1] - 2026-11-19

> Vek v1.1.1 updates documentation-site analytics configuration and automates release-triggered deployment of vekui.dev. The Vek component API is unchanged.

### 🌐 Documentation Site Analytics
- Read the Google Analytics measurement ID from host configuration for the Server, WASM Host, and WASM apps. ([PR #36](https://github.com/madiseh/Vek/pull/36))

### 🚀 Release Website Deployment
- Deploy vekui.dev after NuGet publication, with an HTTPS health check and rollback attempt. ([PR #40](https://github.com/madiseh/Vek/pull/40))

---

## [v1.1.0] - 2026-11-19

> Vek v1.1.0 adds comprehensive keyboard navigation and accessibility support for DataGrid, plus configurable custom grouping intervals.

### ⌨️ DataGrid Keyboard Navigation & Accessibility
- Added WAI-ARIA grid keyboard navigation with roving focus, grid boundary navigation, paging, editing and selection shortcuts, and natural Tab exit.
- Added focused-cell state and accessible grid roles, active-descendant tracking, and selected-state reporting.
- Added an interactive documentation example. ([PR #22](https://github.com/madiseh/Vek/pull/22))

### 🗓️ Custom DataGrid Group Intervals
- Added date-based year, month, quarter, and alphabetic intervals, configurable numeric intervals, and custom group-key calculators.
- Rebuilds group definitions when interval parameters change and documents the feature with an interactive example. ([PR #23](https://github.com/madiseh/Vek/pull/23))

### ✅ Test Results
- Unit tests: 6,433 passed, 2 skipped, 0 failed (6,435 total).
- Documentation tests: 1,456 passed, 0 skipped, 0 failed.
- Analyzer tests: 391 passed, 0 skipped, 0 failed.
- Format check: passed.
- Build-only projects: 2 passed.
- [Download the JUnit test reports](https://github.com/madiseh/Vek/releases/download/v1.1.0/test-results-v1.1.0.zip) from [candidate CI run 44408158952](https://github.com/madiseh/Vek/actions/runs/44408158952), for commit `f13fd7e55fa8a7e270a339b0aa4cb2ba0c29e379`.

---

## [v1.0.5] - 2026-11-18

> Vek v1.0.5 introduces DataGrid cell range and multi-cell selection, focused row and active keyboard navigation, a major documentation overhaul with a modern bento hero and glassmorphism styling, component architecture reorganization into functional subfolders, and completion of the 100-feature migration catalog.

### 🔲 DataGrid Cell Range Selection & Multi-Cell Selection
- Added rectangular cell range selection (`DataGridCellRange`) with row/column boundary tracking (`StartRowIndex`, `EndRowIndex`, `StartColumnIndex`, `EndColumnIndex`).
- Added `DataGridSelectionMode.Cell` and `DataGridSelectionMode.CellRange` supporting single-cell, multi-cell, and continuous rectangular cell selection.
- Introduced `DataGridCellSelectionModel<T>` managing selected ranges and individual cell coordinates with shift-click range expansion.
- Added interactive documentation example (`DataGridCellRangeSelectionExample`) and comprehensive bUnit test coverage (`DataGridCellRangeTests`).
- Styled cell selection regions with subtle token-based selection fills and active range boundaries.

### 🎯 DataGrid Focused Row & Keyboard Navigation
- Added focused row selection with `FocusedRowEnabled="true"` and `FocusedRowIndex` two-way binding on `VekDataGrid<T>`.
- Added `FocusedRowChanged` event callback with `DataGridFocusedRowChangedEventArgs<T>`.
- Added programmatic focus management via `DataGridFocusManager<T>` (`FocusRowAsync`, `FocusNextRowAsync`, `FocusPreviousRowAsync`, `FocusFirstRowAsync`, `FocusLastRowAsync`).
- Built-in ARIA keyboard navigation for row focus (Arrow Up, Arrow Down, Home, End).
- Full NativeAOT / trimming compatibility with `[DynamicallyAccessedMembers]` annotations on focus types.
- Visual styling with elevation, token-based outline, and active background tint.
- Added interactive documentation example (`DataGridFocusedRowExample`) and comprehensive bUnit test suite (`DataGridFocusedRowTests`).

### 🎨 Documentation Site Modernization & Bento Hero
- Complete visual redesign of the Vek documentation portal featuring modern typography, refined palette, and glassmorphism appbar.
- Rebuilt landing page with an interactive bento grid hero highlighting core framework capabilities and live component showcases.
- Modernized section headers, cards, and theme presets.

### 🏗️ Component Architecture & Functional Subfolders
- Reorganized components into domain-driven subfolders across 12 component families for maintainability and scalability:
  - **Table**: `Cells`, `Columns`, `Models`
  - **Button**: `ButtonGroup`, `IconButton`, `MenuButton`, `ToggleIconButton`
  - **Input**: `Adornments`, `Base`, `Debounced`, `Field`, `Masked`, `Numeric`, `Range`, `Selection`, `Text`
  - **Tabs**: `Models`, `Parts`
  - **Snackbar**: `Configuration`, `Models`, `Parts`
  - **TreeView**: `Models`, `Parts`
  - **Card**: `Parts`
  - **Breadcrumbs**: `Models`, `Parts`
  - **Stepper**: `Models`
  - **NavMenu**: `Models`
  - **Dialog**: `Models`
  - **DropZone**: `Models`

### 📚 DataGrid Documentation & Examples
- Split legacy catch-all options into dedicated, focused interactive examples:
  - `DataGridColGroupExample`: Demonstrating column group widths, styling, and multi-column span.
  - `DataGridLoadingExample`: Demonstrating async reloads, `LoadingProgressColor`, and custom loading content templates.
  - `DataGridFixedHeaderFooterExample`: Demonstrating viewport scrolling with sticky fixed headers and aggregate fixed footers.
- Synchronized governance rules in `docs/governance/development-rules.md`.

### 🔄 Autonomous Feature Migration Pipeline
- Expanded upstream feature catalog to 100 comprehensive specifications across all core UI components.
- Upgraded automated quality gate verification to full `dotnet format` checks in CI.

---

## [v1.0.4] - 2026-11-17

> Vek v1.0.4 introduces sticky fixed and pinned columns with dynamic drag pinning, an advanced filter row with type-safe comparison operators, a column chooser visibility dialog, and full orchestration for the autonomous feature migration pipeline.

### 📌 DataGrid Fixed & Pinned Columns
- Added left and right sticky column pinning with `DataGridFixedStyle` (`None`, `Left`, `Right`) on `Column<T>`.
- Dynamic sticky offset calculation for multi-column left and right pinning without overlapping.
- Visual elevation drop shadows separating fixed boundary zones from scrollable content.
- Column options header context menu actions for dynamic pinning and unpinning.
- Dynamic drag pinning support when reordering columns across fixed boundary regions.
- Added interactive documentation example (`DataGridFixedColumnsExample`) and complete bUnit test suite.

### 🔍 DataGrid Advanced Filter Row
- Added dedicated filter row below column headers enabled via `FilterRow="true"` on `VekDataGrid<T>`.
- Rich set of comparison operators (`DataGridFilterOperator`) including Equals, NotEquals, Contains, StartsWith, EndsWith, GreaterThan, LessThan, Between, Empty, and NotEmpty.
- Dynamic LINQ filter expression generator with null-safe expression tree compilation.
- Type-aware filter cell inputs supporting text, numbers, dates, booleans, and enums.
- Added interactive documentation example (`DataGridAdvancedFilterRowExample`) and comprehensive test coverage.

### 🎛️ DataGrid Column Chooser Dialog
- Interactive column visibility management dialog with search filtering and toggle switches.
- Programmatic control via `ShowColumnChooserAsync()` on `VekDataGrid<T>`.
- Added `Hidable` property on columns to protect critical columns from being hidden.
- Added interactive documentation example (`DataGridColumnChooserExample`) and unit test suite.

### 🔄 Autonomous Feature Migration Pipeline
- Established specifications and contract verification for 30 upstream feature migrations.
- Implemented bidirectional backlog tracking and orchestration tooling.

---

## [v1.0.3] - 2026-11-15

> Vek v1.0.3 introduces content-based column width auto-calculation and double-click column auto-fitting for DataGrid, adds full target framework and CI packaging pipeline support for **.NET 11**, and enhances package asset verification.

### 📐 DataGrid Content-Based Column Auto-Fit
- Added `AutoFitColumnAsync(string propertyOrTitle)` and `AutoFitAllColumnsAsync()` to `VekDataGrid<T>` for programmatic content-driven column sizing.
- Added header resizer double-click interaction to automatically fit individual columns to the width of their widest cell content.
- Integrated client-side DOM and canvas measurement via TypeScript module (`vekDataGrid.ts`) with zero UI layout thrashing.
- Added interactive documentation (`DataGridAutoFitExample`) to the DataGrid columns documentation page.
- Comprehensive unit test coverage for programmatic auto-fit methods and client-side measurement interop.

### 🚀 .NET 11 Support & Multi-Targeting
- Added conditional target framework support for **.NET 11 (`net11.0`)** alongside `net8.0`, `net9.0`, and `net10.0`.
- Configured .NET 11 SDK in NuGet publishing workflow with automated validation verifying all multi-target assemblies (`net8.0`, `net9.0`, `net10.0`, `net11.0`) are bundled inside the published package.
- Updated documentation and governance references to reflect next-generation runtime support.

---

## [v1.0.2] - 2026-11-15

> Vek v1.0.2 introduces native DataGrid data export (CSV and Excel XLSX) with custom formatters, autonomous feature migration contracts, and multi-registry package distribution.

### 📊 DataGrid Data Export (CSV & Excel XLSX)
- Added `ExportDataAsync` method to `VekDataGrid<T>` supporting `ExportFormat.Csv` and `ExportFormat.Xlsx`.
- Configurable export options with `DataGridExportOptions`: custom delimiters, column filtering (`Exportable` property on columns), and custom per-column value formatters.
- Client-side streaming download integration via native browser interop without third-party heavy dependencies.
- Added comprehensive interactive documentation and unit test coverage.

### 🚀 Autonomous Feature Migration Pipeline
- Established autonomous discovery, backlog tracking, and contract specification generator for upstream MudBlazor feature migrations.
- Added verification pipelines and GitHub Actions workflows for continuous autonomous triage.

### 📦 Distribution & Packaging
- Enabled automated publishing to **GitHub Packages** alongside **NuGet.org**.
- Added manual workflow dispatch and automated asset attachment to GitHub Releases.

---

## [v1.0.1] - 2026-11-15

> Vek v1.0.1 introduces the official Vek Prism brand identity, packages high-res assets with .NET 8/10 support, and adds the new DataGrid live search panel.

### 🎨 New Vek Logo & Brand Identity
- Replaced old MudBlazor assets with the official **Vek Prism Logo** across all packages, docs, and icons.
- Updated NuGet package icon and metadata to highlight **.NET 8.0** and **.NET 10.0** multi-targeting.

### ⚡ DataGrid Search Panel
- Added built-in search panel with real-time match highlighting across columns.
- Added interactive documentation examples and comprehensive test coverage.

### 🛠️ Improvements & Refactoring
- Reorganized DataGrid architecture into clean domain subfolders (`Cells`, `Columns`, `Filtering`, `Grouping`, `Models`).
- Split DataGrid documentation into focused feature pages.

---

## [v1.0.0] - 2026-11-15

- Initial release and rebrand to Vek.
