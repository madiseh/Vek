---
name: vek-docs-usecase-planning
description: Plan task-centered Vek component documentation by deriving subpages, section order, and focused examples from the component's actual APIs and behavior. Use before restructuring component docs; not for full Showcase applications.
---

# Vek Component Docs Use-Case Planning

Create an evidence-based information architecture for a Vek component's documentation. The result should make the docs read like Vek's own teaching path: task-focused page titles, a deliberate progression, and small examples that demonstrate Vek capabilities.

This skill plans documentation. Do not edit component docs, examples, navigation, or Showcase projects unless the user also asks for implementation. A user may approve the proposed structure before authoring begins.

## Discovery

1. Read repository guidance relevant to Docs pages and examples.
2. Inspect the component's current Docs page, its example components, and any existing child pages. Follow repository code-discovery guidance; when codebase-memory MCP is available, prefer its graph search and snippets for symbols, then inspect source where needed.
3. Verify candidate topics against the Vek component's public API and behavior. Do not infer support from a MudBlazor page, a component name, or an example alone.
4. Record the current page's order, repeated or generic examples, and Vek-specific capabilities that are under-explained or missing from the current progression.
5. If the user names another library as a reference, use that material only to understand the concern or compare a specific claim. Do not copy its headings, sample data, prose, page order, or assumptions as the proposed Vek structure.

## Derive the structure

- Start from tasks developers need to complete with the component. Name each proposed subpage by the outcome or task, not by a parameter or implementation detail. Put API names in the supporting topic list and examples.
- Group related capabilities when they contribute to one coherent task. Avoid a page per parameter and avoid a flat catalog of every feature.
- Keep the component's main page as a concise introduction with one to three core, practical examples when the user wants a subpage structure. Include a canonical quick start and, when useful, one or two common use cases. Put specialized or advanced examples on the relevant child pages so the overview remains an introduction rather than a feature catalog.
- When the component has feature subpages, place **Explore Features** as the final section at the very bottom of the overview page, after all examples and other overview content. Use the supporting line "Browse specific features and usage patterns in detail:" and link to every proposed child page. Do not place any content after Explore Features.
- Keep advanced keyboard navigation and accessibility guidance out of overview pages. When this material is relevant, put it on a dedicated standalone specialist page and link to it from Explore Features.
- Make each feature subpage a standalone Docs page with its own `@page` route, navigation entry, and direct card link, following the DataGrid docs pattern. Do not implement specialist use cases only as anchors, tabs, accordions, or sections inside the overview page. A route may remain hierarchically named under the component (for example, `/components/datepicker/typed-input`); it must still resolve as its own page.
- Split a distinct companion component into its own child page when it has a separate public API and enough related behavior to teach independently (for example, a range-selection component under a date-picker family).
- Order pages from the common first task to more specialized work, based on the component's actual learning path and Vek's strengths. Do not preserve an upstream order merely because it exists elsewhere.
- Make examples small and directly about the component. Each should show a real, recognizable use case and at least one relevant Vek API or behavior. Keep focused component examples in component docs; reserve complete, multi-step product experiences for Showcases.
- Prefer Vek-specific capabilities as proof points where the API supports them. Do not claim a capability is unique to Vek without evidence, and do not make examples feel different through arbitrary renaming or cosmetic changes alone.
- Keep introductory accessibility notes concise when they help explain a core example. Move advanced keyboard shortcuts and interaction guidance to a standalone specialist page rather than adding it to the overview.

## Explore Features card design

- Inspect the current Vek Docs overview card treatment and reusable components before proposing or implementing the feature grid. Use `/docs/overview`, `DocsExploreCard`, and its styles in `src/Docs/Vek.Docs/Styles/pages/_overview.scss` as visual references only; do not add Overview itself as a DatePicker page, feature, or card destination unless the user asks for that.
- Present feature links in a responsive bento-style grid. Keep the overview's visual language: a contained preview area, subtle dotted surface, rounded surface card, clear category/title/description hierarchy, palette-based accents, and restrained hover motion such as a small lift, shadow, and arrow shift.
- Give each card its own relevant icon or miniature component preview that communicates the task. Avoid repeating a generic icon across all cards. Keep the whole card an accessible link with visible keyboard focus, and respect the project's reduced-motion conventions.
- Reuse `DocsExploreCard` when its API and style scope fit. If its styles are scoped to the Overview page, do not assume they apply elsewhere; reuse the component safely or follow the same design with existing Vek tokens and local styles.
- In the proposal, describe the card preview and interaction treatment along with its title, destination, use case, APIs, and example ideas.

## When documentation implementation is requested

- After changing Docs pages or routes, always inspect `src/Docs/Vek.Docs.Wasm/wwwroot/sitemap.xml`, including when the edit only changes page content.
- Add sitemap entries for new public routes and remove or correct entries for routes that were removed or renamed. Preserve the sitemap's existing canonical host and organization.
- If all public URLs remain the same, leave the sitemap unchanged and report that it was checked.

## Output

Return a proposal the user can review before any edits:

1. **Main page role:** what remains on the component's landing page.
2. **Main-page examples:** propose one to three core practical examples and identify the specialized examples that belong on child pages.
3. **Overview section order:** explicitly show **Explore Features** as the final section on the overview page; keep advanced keyboard and accessibility guidance on a dedicated specialist page.
4. **Explore Features section:** give the heading and supporting line, proposed responsive card layout, each card's title, standalone route, task, unique icon/preview idea, and motion treatment.
5. **Proposed subpages, in order:** for each, give a task-first title, its standalone route, the use case it serves, the Vek API/capabilities it covers, and one or two focused specialist example ideas.
6. **Separate companion pages:** name any related component that merits its own child page and why.
7. **Current material mapping:** indicate what moves, stays, is regrouped, or needs new evidence/examples.
8. **Open decisions:** ask only for decisions that cannot be resolved from the repository or the user's instructions. Ask one question at a time when interviewing the user.

Keep the proposal proportional to the component. Do not force a fixed number of subpages or use cases. If the existing material already has a strong structure, recommend targeted changes instead of restructuring for its own sake.

## Illustrative example

For the current DatePicker family, the method can produce this task-centered map from the documented capabilities:

- **Accepting typed dates:** input masking and text parsing.
- **Controlling selectable dates:** disabled days, custom day classes, and day-cell content.
- **Displaying and navigating the calendar:** calendar views, decade navigation, `GoToDate`, and fixed values.
- **Embedding the calendar in a page:** inline and static display, dialog mode, and picker actions.
- **Formatting dates for a culture:** culture-specific calendars and the default date format.
- **Selecting a date range:** a separate `DateRangePicker` child page for dual-calendar selection, presets, and automatic date swapping.

This map is an example of the method, not a default taxonomy. Derive each component's structure from its own API and user tasks; do not reuse these DatePicker labels for unrelated components.
