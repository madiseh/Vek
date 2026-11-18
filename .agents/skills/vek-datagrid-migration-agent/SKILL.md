---
name: vek-datagrid-migration-agent
description: Select and implement the next actionable DataGrid migration from Vek's backlog, then prepare screenshot evidence and open a pull request. Use for autonomous DataGrid migration work in this repository.
---

# Vek DataGrid Migration Agent

Complete one genuinely actionable READY feature from Vek's DataGrid migration backlog and leave a reviewable pull request targeting `master`. Follow this workflow when the user invokes the skill for a migration cycle. Do not merge the pull request.

## 1. Select a current feature

1. Inspect the working tree and repository instructions. Keep the user's existing changes intact.
2. Fetch `origin/master`, then create an isolated discovery worktree based on that exact fetched commit. Do not run the migration orchestrator in the main checkout.
3. In the discovery worktree, run `python migration/scripts/orchestrator.py` to select and claim the next READY feature. Record its ID, title, target branch, and specification path.
4. Read the feature status and specification, `migration/AGENTS.md`, and `docs/governance/development-rules.md`. Check GitHub for a pull request for the target branch.
5. If the feature is already completed or its pull request is merged, reconcile it with `python migration/scripts/orchestrator.py --complete <feature_id> --pr <pr_url>` and select again. Do not duplicate completed work. Stop and report if no unfinished READY feature can be selected or the selected work cannot be verified as actionable.
6. Create or reuse the feature worktree and target branch from the same fetched `origin/master` base. Perform all implementation, builds, tests, formatting, screenshots, and commits in that worktree.

## 2. Implement to the specification

- Treat the selected specification as the behavior contract. Implement only that feature and follow the repository governance documents and existing component patterns.
- Keep all DataGrid models, components, and formatters out of `src/Vek/Components/DataGrid/` root. Put them in the appropriate functional subfolder such as `Columns/`, `Cells/`, `Filtering/`, `Grouping/`, `Models/`, `Rows/`, `Selection/`, `Validation/`, or `Export/`.
- Keep component parameters as auto-properties. Register reactive parameters using `CreateRegisterScope()` and manage updates through `ParameterState<T>`.
- Use existing design tokens and `CssBuilder`/`StyleBuilder`; do not hardcode colors. Document public APIs with XML summaries and the required `[Category(...)]` attributes.
- Add a working interactive example under `src/Vek.Docs/Pages/Components/DataGrid/Examples/` and reference it from the relevant DataGrid docs page with `Code="@nameof(...)"`.
- Add focused tests for the behavior and edge cases required by the feature. Use the scoped verification commands and conventions in `AGENTS.md` and `docs/governance/testing-standards.md`.
- Do not include third-party commercial UI vendor or product names in branch names, commit messages, PR titles, PR descriptions, comments, or documentation. Check the final diff for accidental references before publishing.

## 3. Capture real UI evidence

Before opening the pull request, run the docs site and capture the changed DataGrid documentation section in a real desktop browser at 1280 CSS pixels wide or more (1440 preferred). The screenshot or ordered screenshot sequence must show the section heading and its interactive example in full, without cropping relevant content. Inspect the captured images for readability.

Commit verified screenshots under `migration/artifacts/ui-screenshots/` using ordered names such as `<feature_id>-datagrid-docs-01.png`. If one image cannot show the complete section, capture consecutive images in reading order until it is covered.

## 4. Reconcile, commit, and open the PR

1. Run the orchestrator completion command for the feature before committing: `python migration/scripts/orchestrator.py --complete <feature_id>`.
2. Review the complete diff and working tree. Ensure implementation, tests, docs, specification/backlog updates, and verified screenshots are present, with no generated outputs or scratch files.
3. Run the narrowest relevant quality gates from the repository instructions. For this migration, build the library, run filtered DataGrid tests, build the docs project, run the required whitespace formatter, and check the diff for prohibited vendor references. Do not report a skipped or failed required gate as passing.
4. Commit the completed changes with a scoped conventional commit message, then push the feature branch to `origin`.
5. Check for an existing pull request for that branch. Update its description if one is already open; do not create a duplicate. If it is merged, reconcile completion and stop without opening another PR.
6. Otherwise open a pull request targeting `master`. Its description must include a concise summary, verification commands and outcomes, and every screenshot embedded in order under a `## UI screenshots` heading using raw GitHub URLs for the pushed branch.
7. Inspect the resulting pull request and confirm its target, branch, description, and screenshot embeds. Leave it open for the user to review and merge.

Report the selected feature, implementation summary, verification outcomes, commit, and pull request URL. If a blocker prevents a required step, preserve the worktree and report the exact blocker and completed side effects.
