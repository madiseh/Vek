# WPF Demo Design

## Intent

Add a native WPF counterpart to the current Blazor demo so the existing component topics can be explored from one desktop window.

## Goals

- Add a `Vek.WPF.Demo` WPF application targeting `net10.0-windows`.
- Open on a Home view, then navigate to documentation views in the same window.
- Include the current 20 screens: Home, Getting Started, 16 VekDataGrid topics, and two VekCodeView topics.
- Reuse `IDemoProductProvider`, `DemoProduct`, and the JSON-backed sample-data adapter.
- Match the existing Blazor demo's dark palette, page hierarchy, navigation tree, article section order, headings, explanatory copy, and local table of contents as closely as WPF allows.
- Use native WPF controls in samples. Keep all topics visible while each DataGrid feature is implemented incrementally; preserve each topic's explanation and clearly identify behaviors that are not implemented yet.
- Keep the two CodeView topics and show read-only native WPF code samples; a reusable WPF CodeView library is outside this change.
- In the documentation sidebar, show Getting Started first, then an initially collapsed Components group containing nested VekDataGrid and VekCodeView sections and their links in the same order as Blazor. Keep Home separate from the documentation navigation.
- Mirror each Blazor article's heading, introduction, section order, and “On this page” links. Place in-development notices in the corresponding live-example/code section without removing later explanation sections.
- Keep the documentation sidebar in a persistent WPF shell. Selecting a destination replaces only the right-side content with that destination's complete, independent WPF `UserControl`.
- Each WPF documentation page owns its article and local TOC layout in its own XAML file; do not render page content through one generic page renderer.
- Render Basic Usage's live example with the native WPF `VekDataGrid`, `ItemsSource`, and `DataGridTextColumn` definitions matching Name, Category, Price, and Product ID.
- VekCodeView Basic Usage has separate HTML, Java, C#, and SQL sections in that order; Multi-Sample has one “Choose a sample” section with selectable samples.
- Mirror Blazor's page file structure under `src/Demo/WPF/Vek.WPF.Demo/Components/Pages/Docs/WPF/`: every `.razor` page under `Components/Pages/Docs/Blazor/` has a same-named `.xaml` page at the matching relative path under `WPF/`. Put Home at `Components/Pages/Home.xaml` and the shared docs shell under `Components/Layout/`.

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

1. **Separate WPF demo app with a page component per Blazor route (selected).** One window hosts Home and a persistent documentation shell. Every WPF page has its own same-named XAML component under the mirrored `Components/Pages` tree and owns its article/TOC layout; the page catalog maps destinations to those components. Selecting a page loads that component into the right pane and leaves the sidebar in place.
2. **One generic WPF page for every destination.** A catalog chooses copy and samples but there are no route-specific page files. This is compact but does not meet the required Blazor-like folder and page naming structure.
3. **Embed the Blazor demo in a WebView.** This reuses pages but does not provide the requested native WPF samples or standalone desktop interaction.

## Decisions

- Use `src/Demo/WPF/Vek.WPF.Demo` for the WPF composition root.
- Mirror the Blazor `Components/Pages/Docs/Blazor` paths and filenames under the WPF demo's `Components/Pages/Docs/WPF`; keep Home at `Components/Pages/Home.xaml`.
- Convert `Vek.WPF.DataGrid` from its generated WPF app scaffold into a WPF class library so the demo can consume `VekDataGrid` as a component.
- Keep one main window. Home is the startup screen; Get Started opens the documentation shell in the same window.
- Preserve the current navigation grouping and page titles. Display the VekDataGrid and VekCodeView topics as native WPF documentation views.
- Use the existing simulated-data adapter for product examples.
- Show a basic live `VekDataGrid` sample with shared product data. Keep unfinished feature-specific topics navigable and clearly marked for incremental implementation.

## Remaining implementation calls

- Use a small view-model/page-catalog layer without adding an MVVM framework.
- Use a read-only `TextBox` for the initial CodeView sample surface; multi-sample content can use native WPF tabs.
- Keep the current Blazor project unchanged.
