# Vek Code View Multi Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an accessible `VekCodeViewMulti` that switches between titled code samples and document it under VekCodeView.

**Architecture:** Keep the single-sample `VekCodeView` unchanged. Add an immutable `VekCodeViewTab` model and a multi-sample Razor module that owns tab selection and renders its active sample through the existing `VekCodeView`. Keep sample content and navigation in `Vek.Demo`.

**Tech Stack:** .NET 10, Blazor Razor Class Library, isolated CSS, bUnit, xUnit.

**Spec:** `docs/superpowers/specs/2026-10-30-vek-code-view-multi-design.md`

## Global Constraints

- Keep reusable UI code in `Vek.Components` and Demo samples in `Vek.Demo`.
- Keep `Vek.Components` independent of Demo and sample-data projects.
- Reuse the current `VekCodeView` highlighter and copy behavior.
- Do not add third-party dependencies.
- Every C# `[Fact]` or `[Theory]` test has an XML `/// <summary>` immediately above its attribute.

## Review Focus

- Empty `Tabs` renders no tab panel and does not throw — test in Task 1.
- One sample remains a valid single tab and renders its title and code — test in Task 1.
- Switching tabs updates the selected state and active code panel — test in Task 1.
- Left/Right, Home, and End select the expected tab without moving past list bounds — test in Task 1.
- Tab titles are rendered as encoded text, and tab/panel ARIA references are unique across two instances — test in Task 1.

---

### Task 1: Define and test the multi-sample interface

**Files:**
- Create: `tests/Components/Vek.Components.Tests/CodeView/VekCodeViewMultiTests.cs`

**Interfaces:**
- Consumes: `VekCodeViewMulti` with a required `Tabs` collection of `VekCodeViewTab` values.
- Produces: bUnit behavior expectations for initial selection, clicking and keyboard navigation, empty and one-tab collections, encoded titles, and independent ARIA ids.

- [ ] **Step 1: Write the failing tests**

  Add `[Fact]` tests named `FirstTabIsSelectedAndRendersItsCode`, `ClickingTabShowsItsCodeAndUpdatesSelection`, `KeyboardNavigationSelectsTabsWithinBounds`, `EmptyTabsRenderNoPanel`, `SingleTabRendersNormally`, and `MultipleInstancesHaveUniqueTabAndPanelIds`. Add XML summaries immediately above every `[Fact]`. For title encoding, use a title containing `<` and assert the rendered button text is literal text.

- [ ] **Step 2: Run the focused tests to verify they fail**

  Run: `rtk dotnet test tests/Components/Vek.Components.Tests/Vek.Components.Tests.csproj --no-restore --filter FullyQualifiedName~VekCodeViewMultiTests -p:BaseOutputPath=C:/Users/madis/AppData/Local/Temp/vek-test-build/bin/`

  Expected: FAIL because `VekCodeViewMulti` and `VekCodeViewTab` do not exist yet.

- [ ] **Step 3: Add the tab model and multi-sample module**

  Create `src/Components/Vek.Components/Components/CodeView/VekCodeViewTab.cs` as `public sealed record VekCodeViewTab(string Title, VekCodeLanguage Language, string Code);`. Create `VekCodeViewMulti.razor`, `.razor.cs`, and `.razor.css` in the same folder. The module exposes `[Parameter, EditorRequired] public IReadOnlyList<VekCodeViewTab> Tabs { get; set; } = [];`, selects the first available tab by default, bounds keyboard selection, and renders only the active sample through `VekCodeView`. Give the tab list, buttons, and panel `tablist`/`tab`/`tabpanel` semantics, linked ids, `aria-selected`, roving `tabindex`, and click plus Left/Right/Home/End selection. Use a per-instance id prefix.

- [ ] **Step 4: Run the focused tests to verify they pass**

  Run the command from Step 2.

  Expected: all `VekCodeViewMultiTests` pass.

### Task 2: Add the multi-sample Demo page and navigation

**Files:**
- Create: `src/Demo/Vek.Demo/Components/Pages/Docs/Blazor/CodeView/MultiSample.razor`
- Create: `src/Demo/Vek.Demo/Components/Pages/Docs/Blazor/CodeView/MultiSample.razor.cs`
- Modify: `src/Demo/Vek.Demo/Components/Layout/DocumentationLayout.razor`
- Modify: `tests/Demo/Vek.Demo.Tests/DemoPageNavigationTests.cs`

**Interfaces:**
- Consumes: `VekCodeViewTab(string Title, string Code, VekCodeLanguage Language)` and `<VekCodeViewMulti Tabs="..." />` from Task 1.
- Produces: `/docs/components/vekcodeview/multi-sample`, demonstrating several titled code samples in selectable tabs, with a nested `Multi-Sample` link under the VekCodeView group.

- [ ] **Step 1: Write the failing Demo navigation/rendering test**

  Add `CodeViewMultiSampleRouteRendersSelectableTabs` to `DemoPageNavigationTests`. Verify the route heading, the nested link target, tab titles, initial code, and that clicking another tab shows its code. Put an XML summary immediately above `[Fact]`.

- [ ] **Step 2: Run the focused Demo test to verify it fails**

  Run: `rtk dotnet test tests/Demo/Vek.Demo.Tests/Vek.Demo.Tests.csproj --no-restore --filter FullyQualifiedName~CodeViewMultiSampleRouteRendersSelectableTabs -p:BaseOutputPath=C:/Users/madis/AppData/Local/Temp/vek-test-build/bin/`

  Expected: FAIL because the Multi-Sample route and navigation item do not exist.

- [ ] **Step 3: Implement the Demo page and navigation link**

  Create the page at `/docs/components/vekcodeview/multi-sample`, keep all samples in `Vek.Demo`, and append a `Multi-Sample` nested `NavLink` to the VekCodeView group in `DocumentationLayout.razor`.

- [ ] **Step 4: Run the focused Demo test to verify it passes**

  Run the command from Step 2.

  Expected: `CodeViewMultiSampleRouteRendersSelectableTabs` passes.

### Task 3: Verify the solution and run the Demo

**Files:**
- No additional file changes expected.

- [ ] **Step 1: Run the full test suite**

  Run: `rtk dotnet test --no-restore -p:BaseOutputPath=C:/Users/madis/AppData/Local/Temp/vek-test-build/bin/`

  Expected: all tests pass.

- [ ] **Step 2: Build the full solution**

  Run: `rtk dotnet build --no-restore -p:BaseOutputPath=C:/Users/madis/AppData/Local/Temp/vek-test-build/bin/`

  Expected: all seven projects build with zero errors.

- [ ] **Step 3: Run the Demo at an available local URL**

  Start `Vek.Demo` on an unused localhost port with `rtk dotnet run --no-build`, then open the Multi-Sample route and verify a tab click changes the displayed code.

- [ ] **Step 4: Check the final diff**

  Run: `rtk git diff --check; rtk git status --short`

  Expected: no whitespace errors; only the multi-sample component, tests, Demo page, navigation, and plan/spec documentation are changed.
