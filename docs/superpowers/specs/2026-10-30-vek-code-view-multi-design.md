# Vek Code View Multi Design

## Intent

Let documentation and consuming applications switch between several titled code examples in one `Vek.Components` viewer, while keeping `VekCodeView` focused on one sample.

## Proposed Interface

- Add a separate `VekCodeViewMulti` component alongside `VekCodeView`.
- It accepts a required `Tabs` collection of `VekCodeViewTab` values. Each tab has a `Title`, `Language`, and `Code`.
- The first tab is selected initially. Selecting another tab updates the title and code shown by composing the existing `VekCodeView`.
- Render tabs with accessible tab semantics and support pointer selection, Left/Right arrow selection, Home, and End.
- An empty collection renders no code panel and does not throw.

## Demo and Navigation

- Keep the existing single-sample VekCodeView Basic Usage page unchanged.
- Add a Multi-Sample page under the VekCodeView documentation group, with multiple titled language samples shown through `VekCodeViewMulti`.

## Alternatives Considered

1. **Separate multi-sample component — selected.** Preserves the small, existing `VekCodeView` interface and makes the tab behavior opt-in.
2. **Expand `VekCodeView` to accept either one sample or a tab collection.** This combines two distinct rendering modes in one interface and makes its required parameters conditional.

## Scope and Constraints

- Keep the new component and tab model in `Vek.Components`; keep sample content and documentation in `Vek.Demo`.
- Reuse the existing syntax highlighter and copy behavior through `VekCodeView`.
- Add component tests for initial selection, switching samples, keyboard navigation, and empty input; add a Demo navigation/rendering test for the Multi-Sample page.
- Do not add third-party dependencies.
