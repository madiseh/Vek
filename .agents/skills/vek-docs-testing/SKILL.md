---
name: vek-docs-testing
description: Audit, test, and diagnose Vek documentation pages and feature subpages using browser and unit test seams. Discover component documentation pages, execute user-level feature interactions (such as APG keyboard navigation, selection, editing, and form interactions), identify unhandled exceptions, and implement fixes with verified regression tests.
---

# Vek Docs Testing & Feature Verification

Use this skill when tasked with testing a Vek component's documentation (e.g. "تست داکیومنت DataGrid" or "بررسی و رفع خطاهای صفحات داکس کامپوننت X") without requiring explicit page URLs from the user.

## Overview

Vek documentation is organized into main component overviews and specialized feature subpages (under `src/Docs/Vek.Docs/Pages/Components/<ComponentName>/` and registered in `MenuService.cs`). This skill provides an autonomous discovery and verification pipeline:

1. **Autonomous Route Discovery**: Locate all documentation subpages and live interactive examples for the targeted component.
2. **Interactive UI / Interaction Audit**: Test interactive features (keyboard navigation, cell range selection, inline/form edits, filtering, column chooser, context menus) using browser/Playwright tools or scoped bUnit runner seams.
3. **Unhandled Exception Detection**: Capture Blazor server-side or WebAssembly runtime errors (`An unhandled exception has occurred...`).
4. **Root Cause Analysis & Fix**: Trace exceptions to missing cascades, component state mismatches, unhandled edit commits, or rendering exceptions.
5. **Verification Seam**: Add regression unit tests in `Vek.UnitTests` or `Vek.UnitTests.Docs` and re-verify cleanly.

---

## Autonomous Route & Example Discovery

When asked to test a component (e.g. `DataGrid`):

1. **Inspect Navigation & Structure**:
   - Query `src/Docs/Vek.Docs/Services/Menu/MenuService.cs` for the component's registered routes (e.g., `datagrid`, `datagrid/selection`, `datagrid/keyboard-accessibility`, `datagrid/columns`, `datagrid/editing`, `datagrid/filtering`, etc.).
   - Inspect files under `src/Docs/Vek.Docs/Pages/Components/<ComponentName>/` and their examples in `Examples/`.
   - Check `src/Docs/Vek.Docs.Wasm/wwwroot/sitemap.xml` for published routes.

2. **Map Feature Matrix to Test**:
   - **Overview page** (`*Page.razor`): Canonical quickstart, Bento grid feature cards (`vek-bento-grid`).
   - **Core feature subpages**:
     - *Selection & Focus*: Row click, checkbox selection, focused row navigation.
     - *Keyboard & Accessibility (APG)*: Roving tabindex (`tabindex="0"` on active cell, `-1` on inactive), Arrow keys, Home/End, Ctrl+Home/Ctrl+End, PageUp/PageDown, Space (selection toggle), F2/Enter (edit activate/commit), Escape (cancel edit).
     - *Editing*: Inline row editing, cell editing, edit form dialog, validation, commit and cancel lifecycle.
     - *Columns & Data*: Column resizing, reordering, grouping, sorting, filtering.

---

## Testing Workflow & Interaction Matrix

### 1. Verification Seams
- **Interactive UI Testing (Playwright / Browser Subagent)**:
  - If a local Docs server is running (e.g. `http://localhost:5000` or configured port in `launchSettings.json`), navigate directly to each discovered route without waiting for the user to provide URLs.
  - Replay interactive sequences (click cells, trigger action buttons like `Left`, `Right`, `Edit (F2)`, `Commit (Enter)`, `Cancel (Esc)`, `Toggle Select (Space)`).
  - Verify that no red error banner (`#blazor-error-ui`) appears.
- **Component & Docs Unit Tests (bUnit Seam)**:
  - For rapid, deterministic reproduction of Blazor lifecycle exceptions, use bUnit tests in `tests/Components/Vek.UnitTests/Components/<ComponentName>/` and `tests/Docs/Vek.UnitTests.Docs/`.
  - Execute tests using the scoped runner:
    ```bash
    dotnet test .\tests\Components\Vek.UnitTests\bin\Debug\net10.0\Vek.UnitTests.dll -- --filter "FullyQualifiedName~<ComponentName>" --output Normal --no-ansi
    ```

### 2. Common Docs & Interaction Failure Modes
- **Blazor Error Banner (`An unhandled exception has occurred`)**:
  - Null reference in `@ref` or state when accessed before initial render or during dynamic callbacks.
  - Clone strategy failures during inline edit (`ICloneStrategy<T>` failing to copy properties or handling complex objects).
  - Type mismatch during edit binding (e.g. `Convert.ChangeType` failing on numeric or nullable types).
  - Incomplete parameter cascades (e.g., missing `VekDialogProvider` or `VekPopoverProvider` in Docs example wrappers).
  - Missing route or mismatched casing in `MenuService.cs` or `DocsExploreCard`.

### 3. Fixing and Verifying Issues
1. **Reproduce**: Capture the exact failing interaction or lifecycle hook.
2. **Regression Test**: Write or augment a test in `Vek.UnitTests` exercising the exact edge case (e.g. keyboard navigation activating inline edit, committing changes, or canceling).
3. **Minimal Fix**: Correct the defect in `src/Vek/` or `src/Docs/`.
4. **Validate**:
   - Re-run scoped tests to prove they turn green.
   - Run `dotnet format whitespace --folder --include <modified_files>`.
   - Verify zero unhandled exceptions on the docs page.
