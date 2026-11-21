---
name: vek-component-adoption
description: Adopt and migrate a Blazor consumer project to Vek components, including existing controls, and file evidence-based issues for Vek gaps. Use when a project is expected to use Vek; this skill does not migrate Vek itself.
---

# Vek Component Adoption

Use this skill in a Blazor application or library that consumes Vek. Its goal is to make consumer-owned UI consistently use the appropriate Vek components while preserving the project's behavior, semantics, accessibility, and conventions. This workflow does not depend on codebase-memory or any other repository index; use the discovery tools available in the target project, such as IDE symbol search, `rg`, or equivalent.

## Operating principles

- Treat the installed Vek package version and the target project's actual needs as authoritative. Verify exact component and parameter names from that version's XML/API docs, source, or official Vek documentation. Do not infer API names from examples or component concepts (`VekTextBox`, `VekTextField`, and similar names are not interchangeable unless verified).
- Prefer Vek when it provides a behaviorally and accessibly suitable equivalent. Preserve meaningful HTML semantics and specialized controls when there is no suitable Vek equivalent.
- Refactor existing consumer-owned UI as well as new UI within the requested migration scope. Do not use blind textual replacement.
- File an issue only for a concrete capability needed by existing project usage. Do not turn speculative ideas or an API inventory into feature requests.
- A request to use this skill authorizes creating appropriate issues in the verified official Vek repository for confirmed gaps. Do not ask for confirmation again before each issue. Public issue content must be sanitized and minimal.

## Gate 1: Confirm the target and Vek usage

1. Read the target repository's agent instructions, contributor guidance, and relevant project documentation. Follow its scope, coding, and verification rules.
2. Identify the relevant Blazor solution/projects and consumer-owned UI source. Establish the requested scope from the user's request; if it covers the project generally, inspect all relevant authored application UI, not generated or vendored code.
3. Confirm Vek use from concrete evidence, such as a `Vek` package reference (including central package management), a resolved dependency/lock file, Vek component imports, or existing Vek usage.
4. If no reliable evidence shows that the target consumes Vek, stop before modifying code or installing/upgrading packages. Report the evidence checked and that the adoption workflow did not proceed.

## Gate 2: Establish the available Vek API

Before mapping controls, determine the effective Vek version for each target project and the source of API truth available in that repository. Prefer, in order, installed package XML/API documentation or source, version-matched official docs, and then current official Vek source/docs when version-matched information is unavailable. Clearly account for version differences.

Build a small working map from existing UI patterns to verified Vek APIs. For example, inspect actual button, text-entry, select, checkbox, radio, date/time, table/grid, menu, and dialog usage where present. These are discovery examples, not a requirement to introduce every component category. For every proposed mapping, confirm:

- the component exists in the project's Vek version;
- it supports the needed value, events, validation, composition, and customization;
- it preserves the original semantic role and expected user interaction;
- it has an accessible name and appropriate keyboard behavior in the actual composition.

If the API or version compatibility cannot be established, do not guess. Mark the mapping unresolved and continue with independent, verified migrations.

## Gate 3: Inventory and migration plan

Use project-appropriate code discovery. Search authored Razor/C# and any UI abstraction layers for native controls and third-party component equivalents. Follow references and usages far enough to understand behavior; inspect representative call sites and shared wrappers before editing them.

Classify each relevant pattern as one of:

1. **Supported migration:** a verified Vek component satisfies the existing behavior and semantics.
2. **Supported composition:** a composition of Vek components faithfully supplies the behavior and accessibility.
3. **Semantic/specialized exception:** the element has a distinct purpose (for example, a navigation link or specialized input) and changing it would damage semantics or behavior.
4. **Vek capability gap:** existing project behavior requires a capability that neither one Vek component nor a sound Vek composition provides.
5. **Unresolved:** evidence is insufficient to decide safely.

Create a concise plan by pattern and affected locations before broad edits. Include existing controls that should migrate, not only controls touched by the immediate feature. Exclude generated output, vendored dependencies, package contents, and build artifacts. Preserve project-specific wrappers where they provide a real abstraction, but update their implementation to use Vek when appropriate.

## Gate 4: Resolve and report Vek capability gaps

For each **Vek capability gap**, first verify that it is not caused by misuse of the existing API or a version mismatch. Record the concrete current-project behavior that requires the capability and why an available Vek component or composition does not satisfy it. A gap without evidence from current usage is not an issue candidate.

Use `madiseh/vek` as the expected official issue repository, but verify that it is the official Vek repository using reliable package/project metadata or official Vek links before publishing. If the repository identity cannot be verified, do not publish; prepare a draft and explain the blocker.

Before creating an issue:

1. Search the official repository for an equivalent issue, including closed issues. If an equivalent issue exists, do not create a duplicate; report its link and status.
2. Separate distinct capabilities into distinct issues when they can be implemented independently. Combine closely related details only when they describe one coherent capability.
3. Classify a defect in existing documented behavior as a bug. Classify absent behavior as a feature request. Do not choose labels or a milestone by guesswork; inspect the repository's current labels and use only applicable existing labels. Set a milestone only when the repository clearly provides the intended milestone.
4. Write a concise title and a useful body grounded in the observed use case: affected component, current behavior/limitation, desired behavior, and a minimal sanitized example when helpful.
5. Remove proprietary code, customer or personal data, secrets, internal URLs, and unrelated project details. Describe the requirement generically. If a safe, accurate summary cannot be made, do not publish; provide a draft and state why it was withheld.
6. Create the issue directly when an authorized GitHub integration or authenticated CLI is available. Verify the created issue and include its link. If access or write permission is unavailable, preserve a ready-to-file draft, report the precise limitation, and continue the rest of the migration.

Do not comment on an existing issue unless the user separately requests that action. A matching issue link is enough to connect the consumer project's need to the Vek backlog.

## Implement the migration

Migrate supported patterns using the project's established code style and the verified Vek API. Keep the change focused on consumer-owned UI in scope. Preserve behavior, including as applicable:

- value binding, callbacks, validation, and form integration;
- disabled, loading, error, and empty states;
- labels, accessible names, ARIA relationships, keyboard interaction, and focus behavior;
- meaningful styling, layout, and responsive behavior;
- caller-facing component APIs and existing customization points.

Do not replace links with buttons or buttons with links based only on appearance. Do not retain a non-Vek control merely because it already exists when a suitable Vek equivalent is verified. For unsupported needs, use a faithful Vek composition if available; otherwise leave that control as a clearly recorded exception while migrating independent supported patterns. Do not build an imitation of a missing Vek feature that weakens accessibility or silently changes behavior.

Update relevant tests, examples, or documentation when required by the target repository's conventions or when needed to keep the changed behavior and public usage clear. Do not modify generated tests or generated artifacts by hand.

## Gate 5: Verification

Use the target repository's instructions and the narrowest meaningful verification for the changes. Inspect the relevant project test/build guidance first. Run an appropriate scoped build and the relevant existing tests when available; include accessibility or browser-level verification when the change materially affects interaction or rendering and the repository provides such a workflow. Do not claim verification that was not run.

Then check each gate:

- **Build/test:** relevant verification completed successfully, or every failure is documented with evidence distinguishing migration regressions from known/baseline failures where possible.
- **Migration coverage:** revisit the inventory and confirm every supported pattern in scope was migrated or has a specific reason for remaining.
- **Residual controls:** search the scoped authored code for native or third-party equivalents that should have migrated. Classify each remaining result as intentional, unsupported, generated/vendor/out of scope, or unresolved; do not rely on search alone to infer semantic equivalence.
- **Behavior and semantics:** review changed call sites for lost bindings, callbacks, validation, labels, keyboard/focus behavior, styling, or caller customization.
- **Issue follow-through:** verify every capability gap has a created issue link, an existing duplicate link, or a ready-to-file draft with a stated blocker. No unresolved gap should disappear from the final report.

If verification reveals a regression introduced by the migration, fix it and rerun the relevant check. If verification cannot pass because of an unrelated baseline issue, leave the migration state explicit and report the failure; do not describe the overall result as fully verified.

## Final report

Give a concise, evidence-based report with:

- the projects and scope inspected;
- the Vek package/version evidence;
- the component patterns migrated and the main locations changed;
- **remaining components**, grouped by intentional semantic/specialized exception, missing Vek capability, or unresolved mapping, with a short reason and location;
- each capability issue created (link), existing duplicate (link/status), or draft not filed (reason);
- the build/tests and any other verification run, with pass/fail status and material limitations.

Never report complete migration while supported in-scope controls or unresolved gaps remain unaccounted for.
