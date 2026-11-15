# Vek Changelog

All notable changes to Vek will be documented in this file.
For GitHub Releases and downloadable packages, see the [Releases page](https://github.com/madiseh/Vek/releases).

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
