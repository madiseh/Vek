---
name: vek-showcase-authoring
description: Create or refactor full interactive Vek documentation showcases using Vek components and the project's page structure.
---

# Vek Showcase Authoring

Use this skill for complete interactive use cases in `src/Docs/Vek.Docs.Showcases`. A Showcase teaches and exercises Vek components; its rendered UI must be composed from Vek components.

## Page structure

For a route that belongs to one Showcase page, keep the page in `Pages/` and create exactly the page's three companion files with the same name:

```text
Pages/<ShowcaseName>.razor
Pages/<ShowcaseName>.razor.cs
Pages/<ShowcaseName>.razor.css
```

- Keep route markup, component state and behavior, and scoped styles in those three files respectively.
- Put state, derived values, event handlers, persistence, and helper methods in the `.razor.cs` partial class. Do not add an `@code` block to the page markup.
- Put page-scoped styles in `.razor.css`; prefer Vek design tokens and classes attached to Vek components.
- Blazor CSS isolation does not scope styles onto child Razor components. For a Vek-only page, define a stable `CssScope` for its `.razor.css` in `Vek.Docs.Showcases.csproj`, pass that same attribute to the root Vek component as an unmatched attribute, and use `.root-class ::deep .child-class` selectors for descendant Vek components. Do not rely on generated scope identifiers or add a raw HTML wrapper.
- Do not create a page-specific component under `Components/` just to host the page UI. Add a separate component only when it is genuinely reused by multiple Showcase pages or the user explicitly asks for that structure.
- Keep the route and navigation registration consistent with the existing Server and WASM hosts. Do not change Landing content unless the requested task requires it.

## Vek-only markup contract

Every rendered UI element in `.razor` must be a Vek component. Do not write native HTML elements such as `div`, `section`, `button`, `input`, `form`, `table`, or `a`; do not render HTML through `MarkupString`, hand-built render fragments, or JavaScript as a workaround. Razor control flow (`@if`, `@foreach`, expressions, and templates accepted by Vek components) is fine, but its rendered elements must still be Vek components.

Before choosing a component or parameter, inspect its public API and existing examples. Prefer the closest Vek component for layout, text, input, buttons, navigation, charts, and tabular records. For interactive data, use Vek's table/data-grid components and their supported templates, sorting, paging, and selection behavior instead of an HTML table or decorative imitation.

If a required behavior is not supported by the available Vek components, do not silently fall back to raw HTML or invent a nonfunctional control. Use a composition of supported Vek components where possible; otherwise state the specific Vek capability gap and keep the implementation honest.

## Verification gate

Completion requires passing the structural gate and the applicable runtime gate. A successful build alone does not pass this gate.

### Structural gate — required

- Confirm the `.razor`, `.razor.cs`, and `.razor.css` files exist together under `Pages/` with matching names.
- Confirm page state and handlers are in the partial code-behind, not in an `@code` block or inline JavaScript.
- Inspect the entire `.razor` file and confirm all rendered tags are Vek components; there are no native HTML tags, `MarkupString`, or raw HTML render fragments.
- Confirm the page does not delegate its main UI to a one-off component in `Components/`.
- Check that every named Vek component and parameter exists in the component source/docs, and that visible controls produce the represented behavior.
- Verify in the browser that the page's CSS scope attribute is present on the root Vek element and that representative rules from `.razor.css` affect the rendered layout.

If any check fails, fix the structure before building or reporting completion.

### Runtime gate — for implementation/refactor tasks

- Build the relevant Showcase host projects: `Vek.Docs.Server` and `Vek.Docs.WasmHost`. Run the normal asset pipeline when the Razor-isolated CSS or other frontend assets changed; do not pass `SkipBunCompile` for those changes.
- Open the exact Showcase route in a browser and exercise its primary actions, filters, sorting/paging, add/edit/delete flows, and in-page links as applicable. Check for failed route navigation, unresponsive controls, and browser/server errors.
- Keep verification data temporary and remove it before finishing.
- Report build verification and browser verification separately. Never imply that a build proves runtime behavior.

Do not add or run a test suite unless the user explicitly asks for tests or verification; the structural and runtime gates above are the Showcase-specific verification required by this skill.
