# Architecture

Vek contains reusable .NET 10 Blazor component packages, a reusable WPF DataGrid library, and separate Blazor and WPF demo hosts. Both demos follow the same hexagonal data boundary: they consume a demo-facing port, and the JSON-backed sample-data project implements that port.

## Solution Projects

| Project | Responsibility |
| --- | --- |
| `Vek.Core.DataGrid` | Platform-neutral DataGrid contracts, per-instance state, and shared data operations used by UI adapters. |
| `Vek.Blazor.DataGrid` | NuGet-ready Razor Class Library for the reusable DataGrid feature family. |
| `Vek.Blazor.CodeView` | NuGet-ready Razor Class Library for reusable code-viewer components. |
| `Vek.WPF.DataGrid` | Reusable WPF class library for the native DataGrid control. |
| `Vek.Demo.Abstractions` | Demo-facing models and ports, including `DemoProduct` and `IDemoProductProvider`. |
| `Vek.SimulatedData` | JSON sample data and `JsonDemoProductProvider`, the adapter that implements the demo data port. |
| `Vek.Demo` | Blazor Web App and composition root. It registers the JSON adapter and demonstrates the data path. |
| `Vek.WPF.Demo` | Native WPF application and composition root. It consumes the WPF DataGrid, demo ports, and JSON adapter. |
| `Vek.Components.Tests` | xUnit and bUnit test project for component behavior. |
| `Vek.WPF.Demo.Tests` | Windows-targeted xUnit test project for WPF demo integration and UI behavior. |

## Dependency Direction

```mermaid
flowchart LR
    Demo[Vek.Demo<br/>composition root]
    DataGrid[Vek.Blazor.DataGrid<br/>Razor Class Library]
    CoreGrid[Vek.Core.DataGrid<br/>shared contracts and behavior]
    CodeView[Vek.Blazor.CodeView<br/>Razor Class Library]
    WpfDemo[Vek.WPF.Demo<br/>WPF composition root]
    WpfDataGrid[Vek.WPF.DataGrid<br/>WPF class library]
    Ports[Vek.Demo.Abstractions<br/>models and ports]
    SimData[Vek.SimulatedData<br/>JSON adapter]
    Tests[Vek.Components.Tests<br/>xUnit + bUnit]
    WpfTests[Vek.WPF.Demo.Tests<br/>WPF xUnit tests]

    Demo --> DataGrid
    DataGrid --> CoreGrid
    Demo --> CodeView
    Demo --> Ports
    Demo --> SimData
    WpfDemo --> WpfDataGrid
    WpfDataGrid --> CoreGrid
    WpfDemo --> Ports
    WpfDemo --> SimData
    SimData --> Ports
    Tests --> DataGrid
    Tests --> CodeView
    WpfTests --> WpfDemo
```

The Blazor packages and WPF DataGrid library have no references to either demo host, demo models, or the data provider. Each demo composes the shared provider and passes data into reusable components; components do not read JSON or call a database/API. The Blazor demo uses component parameters and callbacks, while the WPF demo binds the same demo-facing models to native WPF controls.

## Sample Data Flow

1. `Vek.Demo` registers `IDemoProductProvider` with `JsonDemoProductProvider`.
2. `JsonDemoProductProvider` reads `products.json` from `Vek.SimulatedData`.
3. The provider returns `DemoProduct` values defined by `Vek.Demo.Abstractions`.
4. The Demo page passes those values to components as it showcases them.

This flow keeps the sample-data adapter replaceable while leaving the component library independent of how a demo obtains data.

## Component Organization

Organize each independently packaged Blazor component category under `src/Components/Blazor/<PackageProject>/`. Every package project must contain a `Components/` directory; `Directory.Build.targets` enforces this rule during builds. Keep each component's Razor markup and isolated CSS together:

```text
src/Components/Blazor/Vek.Blazor.DataGrid/
├── Components/
│   └── DataGrid/
├── _Imports.razor
└── Vek.Blazor.DataGrid.csproj

src/Components/Blazor/Vek.Blazor.CodeView/
├── Components/
│   └── CodeView/
├── _Imports.razor
└── Vek.Blazor.CodeView.csproj
```

Keep simple components in a `.razor` file. Add a `.razor.cs` code-behind when the component's logic benefits from being separated from its markup. Keep package-wide styles under that package's `wwwroot/` directory.

## Cross-platform component architecture

When one component category is implemented for multiple UI platforms, its Core project owns platform-neutral contracts, state, and shared behavior. Each UI project owns only its platform interaction and presentation: native events, rendering, templates, editors, styling, and platform-specific capabilities.

All adapters use the same Core interfaces, state names, operation names, and local names for shared concepts. For DataGrid, canonical names include `Items`, `Page`, `PageSize`, `Sorts`, `Filters`, `Groups`, and `SelectedKeys`. Core defines one generic `IVekDataGridColumn<TItem, TTemplate>` contract. Its `TTemplate` parameter is supplied by each adapter (`RenderFragment` in Blazor and `DataTemplate` in WPF), while `VekDataGridTemplate<TContext, TTemplate>` preserves the exact context and native template types for cell, filter, group, edit, and detail slots. An adapter maps UI event arguments into these contracts, calls Core, and renders the resulting state. Shared algorithms such as filtering, sorting, paging, grouping, aggregation, selection transitions, and edit transitions belong in Core. Features declared by the shared contract must be supported by every adapter.

For remote data, Core creates a neutral request descriptor containing page and query state. The host calls its data provider with that descriptor and a cancellation token, then supplies the returned items and total count to Core. Core does not depend on a provider, HTTP client, database, or UI framework.

Keep template implementations, visual state, editor components, and concrete native UI types out of Core. Generic template type parameters and platform-neutral template contexts may appear in Core contracts without referencing UI frameworks. A component that has only one UI implementation does not need a Core project. Use `Vek.Core.DataGrid` and its Blazor and WPF adapters as the example when adding another cross-platform component category.

## Target Framework and Platforms

- Cross-platform projects target `net10.0` through the root `Directory.Build.props`; WPF applications, controls, and tests target `net10.0-windows` and use WPF.
- Solution platform choices are `AnyCPU`, `x86`, and `x64`. The default target is `AnyCPU`; explicitly selected `x86` and `x64` builds set matching platform targets.
- Package each reusable component category independently. Demo, ports, sample-data, and test projects remain non-packable.
