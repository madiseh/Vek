# Vek Code View Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a reusable read-only `VekCodeView` with built-in syntax highlighting for HTML, Java, C#, and SQL, plus copy support and Demo examples.

**Architecture:** Keep the component, language enum, and tokenization logic inside `Vek.Components`. Render token text through Razor encoding, use a small JS module only for browser clipboard access, and keep the four showcase samples and navigation in `Vek.Demo`.

**Tech Stack:** .NET 10, Blazor Razor Class Library, isolated CSS, JavaScript module for clipboard access.

**Spec:** `docs/superpowers/specs/2026-10-29-vek-code-view-design.md`

## Global Constraints

- Target `net10.0`.
- Keep `Vek.Components` independent of Demo and sample-data projects.
- Do not add MudBlazor or a third-party syntax-highlighting dependency.
- Support HTML, Java, C#, and SQL, with one explicit language per component instance.
- The component is read-only and exposes copy with accessible status feedback.
- Preserve unrelated existing working-tree changes.

## Review Focus

- Code containing HTML-looking text must be rendered as text, never interpreted as markup.
- Empty and multiline code must retain its contents and line breaks.
- Comments and quoted strings containing keyword-like text must not be colored as keywords.
- Clipboard API failure or an unsupported browser must produce readable failure feedback.
- A language value outside the supported enum values must fail predictably without breaking the page.

---

### Task 1: Add the reusable VekCodeView component

**Files:**
- Create: `src/Components/Vek.Components/Components/CodeView/VekCodeLanguage.cs`
- Create: `src/Components/Vek.Components/Components/CodeView/VekCodeToken.cs`
- Create: `src/Components/Vek.Components/Components/CodeView/VekCodeHighlighter.cs`
- Create: `src/Components/Vek.Components/Components/CodeView/VekCodeView.razor`
- Create: `src/Components/Vek.Components/Components/CodeView/VekCodeView.razor.cs`
- Create: `src/Components/Vek.Components/Components/CodeView/VekCodeView.razor.css`
- Create: `src/Components/Vek.Components/Components/CodeView/VekCodeView.razor.js`

**Interfaces:**
- `VekCodeLanguage` has `Html`, `Java`, `CSharp`, and `Sql` values.
- `VekCodeView` exposes required `string Code` and `VekCodeLanguage Language` parameters.
- Internal `VekCodeHighlighter.Highlight(string code, VekCodeLanguage language)` returns ordered text tokens with a syntax category.
- The Razor view escapes token text, labels the copy button, and announces copy success or failure through a polite live region.
- The JS module exports `copyTextToClipboard(string text)` and reports clipboard success as a boolean.

- [x] Implement language and token types, then add focused scanners for the four languages, giving comments and quoted strings precedence over keywords and leaving unknown text as plain code.
- [x] Add Razor markup and isolated styles for a responsive, high-contrast code panel with language label, scrollable preformatted text, and an always-visible keyboard-accessible copy button.
- [x] Add clipboard module loading and accessible success/failure feedback to the component code-behind; handle JS interop unavailability without throwing during normal rendering.

### Task 2: Add the Demo documentation page and navigation

**Files:**
- Create: `src/Demo/Vek.Demo/Components/Pages/Docs/CodeView/CodeView.razor`
- Create: `src/Demo/Vek.Demo/Components/Pages/Docs/CodeView/CodeView.razor.cs`
- Create: `src/Demo/Vek.Demo/Components/Pages/Docs/CodeView/CodeView.razor.css`
- Modify: `src/Demo/Vek.Demo/Components/Layout/DocumentationLayout.razor`
- Modify: `src/Demo/Vek.Demo/Components/_Imports.razor`

**Interfaces:**
- The page route is `/docs/components/vekcodeview`.
- The page displays four `VekCodeView` instances, one per language, with representative HTML, Java, C#, and SQL snippets.

- [x] Add the documentation page and representative snippets using component parameters, not markup strings that Razor interprets.
- [x] Add `VekCodeView` to shared Razor imports and a matching navigation link under Components.

### Task 3: Build the Demo host

**Files:**
- No additional file changes expected.

- [x] Run `rtk dotnet build src/Demo/Vek.Demo/Vek.Demo.csproj -p:BaseOutputPath=C:/Users/madis/AppData/Local/Temp/vek-code-view-build/`.
- [x] Fix build errors caused by this feature and rerun the build to confirm success.
