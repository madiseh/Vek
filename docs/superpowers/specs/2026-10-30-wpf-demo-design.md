# WPF Demo Design

## Intent

Add a native WPF counterpart to the current Blazor demo so the existing component topics can be explored from one desktop window.

## Goals

- Add a `Vek.WPF.Demo` WPF application targeting `net10.0-windows`.
- Open on a Home view, then navigate to documentation views in the same window.
- Include the current 20 screens: Home, Getting Started, 16 VekDataGrid topics, and two VekCodeView topics.
- Reuse `IDemoProductProvider`, `DemoProduct`, and the JSON-backed sample-data adapter.
- Match the existing Blazor demo's dark palette, page hierarchy, labels, and left navigation as closely as WPF allows.
- Use native WPF controls in samples. Keep all topics visible while each DataGrid feature is implemented incrementally; preserve each topic's explanation and clearly identify behaviors that are not implemented yet.
- Keep the two CodeView topics and show read-only native WPF code samples; a reusable WPF CodeView library is outside this change.

## Non-goals

- Porting all Blazor component behavior to WPF in this first step.
- Adding custom styling or behavior to `VekDataGrid` beyond what its current implementation provides.
- Creating a reusable `Vek.WPF.CodeView` package.
- Changing the Blazor demo or its routes and behavior.

## Constraints

- Keep the shared core and demo/data adapter boundaries independent of UI implementation.
- The demo is the composition root and may reference WPF components, demo ports, and sample-data adapters.
- Reusable WPF controls must be consumable as a library rather than an executable.
- Use the existing Blazor demo as the source for page names and visual treatment. The WPF UI should retain keyboard navigation and focus visibility.

## Approaches

1. **Separate WPF demo app with a data-driven page catalog (selected).** One window hosts Home and documentation content. A catalog provides the 20 screens and the left navigation; WPF controls render each sample. This preserves a single-window flow and keeps the initial documentation scaffold compact while features arrive incrementally.
2. **One WPF `Page` and view per Blazor route.** This mirrors route files directly but creates many mostly identical files before the WPF features exist.
3. **Embed the Blazor demo in a WebView.** This reuses pages but does not provide the requested native WPF samples or standalone desktop interaction.

## Decisions

- Use `src/Demo/WPF/Vek.WPF.Demo` for the WPF composition root.
- Convert `Vek.WPF.DataGrid` from its generated WPF app scaffold into a WPF class library so the demo can consume `VekDataGrid` as a component.
- Keep one main window. Home is the startup screen; Get Started opens the documentation shell in the same window.
- Preserve the current navigation grouping and page titles. Display the VekDataGrid and VekCodeView topics as native WPF documentation views.
- Use the existing simulated-data adapter for product examples.
- Show a basic live `VekDataGrid` sample with shared product data. Keep unfinished feature-specific topics navigable and clearly marked for incremental implementation.

## Remaining implementation calls

- Use a small view-model/page-catalog layer without adding an MVVM framework.
- Use a read-only `TextBox` for the initial CodeView sample surface; multi-sample content can use native WPF tabs.
- Keep the current Blazor project unchanged.
