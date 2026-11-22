---
name: vek-migration-batch-agent
description: Prepare and execute an approved batch of up to three independent feature migrations from Vek's READY backlog, with one pull request per feature. Use for parallel, cross-module migration work; this skill does not discover new features.
---

# Vek Migration Batch Agent

Prepare and execute one approved batch of up to three feature migrations from the current backlog. The batch coordinator owns selection, approval, claims, and reporting. Delegate exactly one feature to each worker. Leave every successful PR open and ready for the user to review; never merge.

This is an explicit batch-level exception to the repository's one-feature-per-task guidance: the coordinator may manage up to three tasks, while every worker, worktree, branch, and PR remains scoped to one feature.

## 1. Build a read-only batch proposal

1. Read the repository instructions, `migration/AGENTS.md`, and the relevant governance docs. Fetch the latest `origin/master` and inspect its backlog without running a command that claims or changes a feature.
2. Select only existing `READY` features whose dependencies are completed. Do not run `orchestrator.py --discover`; backlog discovery belongs to a separate skill/agent.
3. Exclude features with an open PR or an active unexpired claim. Inspect each candidate specification and likely code, test, documentation, and asset paths. Prefer candidates with disjoint files and components; keep the backlog priority order among similarly isolated candidates.
4. Select up to three eligible candidates. If fewer than three low-conflict candidates are available, propose the available count; do not invent work or relax the isolation requirement to fill the batch.
5. Derive each target branch from the current orchestrator convention and backlog pipeline settings without running `--pick`. Present each candidate's feature ID, title, module, specification, target branch, and likely touched paths. State the batch size and any priority items skipped due to open PRs, claims, dependencies, or path overlap. Wait for the user's explicit approval before creating claims, worktrees, branches, or editing files.

Approval authorizes a batch of up to three next eligible features, not only a frozen set of IDs. If a candidate is claimed by another agent after approval, skip it and substitute the next eligible low-conflict candidate without asking again. If the source backlog changes, do not add newly discovered work to the approved batch unless it is needed as such a substitution.

## 2. Claim each feature across agents

Local `in_progress` state in an isolated worktree is not a shared lock. Use a remote claim ref for each feature, for example `migration-claims/<feature_id>`, containing a machine-readable `claim.json` at its root.

- A claim record must include the feature ID, worker/run ID, owner, state, claim time, and UTC lease expiry. Give each initial claim a unique run ID and create it with a normal push to a previously absent ref; only the first competing push succeeds. Never force-push an active claim.
- If the ref exists, inspect its record and the feature's PRs. Skip the feature when an unmerged PR exists or the claim lease is active.
- Claims expire 48 hours after the last heartbeat. Renew at meaningful progress checkpoints. To reclaim an expired claim, append a new claim record to the existing ref and push normally; a stale worker's non-fast-forward push must fail.
- If a feature becomes blocked, record the blocked state and keep renewing its claim while the batch is active. After a PR is opened and verified, remove that feature's claim ref; the open PR then prevents duplicate selection.
- A claim race or push rejection means another worker won. Do not overwrite its ref; select the next eligible candidate.

Claim the approved candidates before dispatching them. Give each worker its feature ID, specification, target branch, worktree path, and claim ref.

## 3. Run one isolated worker per feature

1. Create one subagent per feature, with one isolated worktree and target branch per worker, all based on the fetched `origin/master` commit. Never assign two workers to the same feature. After acquiring the remote claim, run `python migration/scripts/orchestrator.py --pick <feature_id>` in that feature worktree; do not use the no-argument selector to start an already approved batch item.
2. Run workers in parallel up to both the three-feature batch cap and the runtime's available subagent capacity. If fewer than three slots are available, queue the remaining approved items and start the next worker as a slot frees.
3. Each worker must read the selected specification, `migration/AGENTS.md`, root `AGENTS.md`, and applicable governance docs. Use the codebase knowledge graph for code discovery where available; use text search for literals, configuration, and non-code files.
4. Implement only the assigned feature, following Vek architecture and the repository's test, formatting, and build requirements. Derive implementation, test, and documentation paths from that feature's module and existing patterns; never assume every feature belongs to DataGrid.
5. Keep style edits in `src/Vek/Styles/**/*.scss` and script edits in `src/Vek/TScripts/**/*.ts`. Do not edit or commit generated `*.min.*` bundles or their source maps from `src/Vek/wwwroot/` in a feature PR. If a local build regenerates these outputs, restore them before committing. Include or update minified bundles and maps only when preparing release artifacts.
6. Every feature must have a working interactive documentation example on its own component page, focused tests appropriate to its behavior, and real-browser screenshot evidence of the changed documentation/example. Use the repository's existing browser tooling. Capture screenshots at a desktop viewport at least 1280 CSS pixels wide and commit them under `migration/artifacts/ui-screenshots/` with feature-specific ordered names. Inspect the images for readability.
7. Preserve the source project as read-only when a specification uses one as a behavior reference. Reconstruct behavior natively in Vek. Avoid unrelated refactoring and third-party commercial UI branding in branches, commits, PRs, code comments, and docs.
8. Run the narrowest relevant quality gates required by the current repository instructions. If a required gate fails, diagnose and make at most two focused repair iterations. If the feature still cannot pass, report it as blocked, preserve its worktree and claim, and let the coordinator assign a replacement candidate. Do not claim a blocked feature is complete.
9. Before opening a PR, run the repository CI `dotnet format` verification on every changed `src/**` and `tests/**` file. After committing the feature, derive the include list from `git diff --name-only <target-branch>...HEAD`; include every changed source and test file rather than a hand-picked subset. The final `dotnet format whitespace --folder` pass required by `AGENTS.md` only checks whitespace, line endings, and encoding; it does not catch style diagnostics such as `IMPORTS: Fix imports ordering`. From the repository root, repair and verify changes with `dotnet format Vek.slnx --no-restore --include <changed src and tests paths>` and `dotnet format Vek.slnx --no-restore --verify-no-changes --include <changed src and tests paths>`. Use paths relative to the repository root. If the root `.editorconfig` changes, follow CI and run the repository-wide format verification without `--include`. Resolve all reported diagnostics and rerun verification before creating the PR.

## 4. Open and complete each PR

For each successful worker result:

1. Review the worker's diff, test/build results, screenshot evidence, target branch, and claim state. Reject accidental scope changes or missing required evidence.
2. Ensure implementation, tests, docs, and verified screenshots are committed and pushed to the feature branch before creating a PR.
3. Follow the Git commit and pull request title conventions in `docs/governance/development-rules.md` for every commit subject and PR title. Use `<type>(<scope>): <imperative summary>`; for example, `feat(button): add loading state with auto-disable`. Put `Migration <feature_id>` and its context in the PR description, not as a `[Migration NNN]` title prefix. Reference related GitHub issues in the description with closing keywords.
4. Open a draft PR targeting `master`, with a concise summary, verification results, and all screenshot embeds in reading order under `## UI screenshots`.
   - For committed screenshots, use the raw image URL form `https://raw.githubusercontent.com/<owner>/<repo>/<branch>/<path>` as the Markdown image source.
   - Do not embed `https://github.com/<owner>/<repo>/blob/...` URLs. They serve an HTML file preview, so the link can open when clicked while the image remains broken inline.
   - Check for an existing PR immediately before creation; update an existing open PR instead of creating a duplicate.
5. In that feature worktree, run `python migration/scripts/orchestrator.py --complete <feature_id> --pr <pr_url>`. This records `Completed` and the PR URL in the feature specification and backlog. Commit and push that status update to the same PR.
6. After the PR is opened or updated, inspect its `format-check` result. If it fails, read the complete diagnostics, repair all cited changed `src/**` files, rerun the CI-equivalent verification across the full changed-file set, push the fix, and confirm the new format check passes before marking the PR ready.
7. Verify the PR body, screenshots, base branch, title, and completion commit. Mark the PR ready for review only after the completion update is pushed and `format-check` passes. Then release the claim ref.

For an already-open PR, do not create a duplicate; report or update that PR as appropriate. For a merged PR, reconcile status if needed and skip it. Do not wait for prior batches' PRs to merge before starting a new batch; exclude their features because their PRs are already open.

## 5. Batch completion and report

Continue until three new PRs are ready for review or no more eligible, low-conflict READY features remain. A blocked feature does not stop other workers. If the queue runs out, finish with fewer than three PRs and report that clearly. Do not wait for the backlog discovery agent or run discovery yourself.

### Batch timing report

For each batch, record UTC start and end timestamps per feature for these phases: claim and worktree setup; implementation; tests and builds; documentation and screenshot; formatting and review; commit and push; PR opening and completion metadata; CI wait. Workers should include phase timestamps in each progress/completion report; the coordinator records claim/worktree and PR/CI timestamps. Do not infer elapsed time from message order when timestamps are unavailable; mark it unknown or approximate and state why.

The final report includes:
- **Calendar batch duration**: earliest batch claim/setup start through the latest terminal CI result (or current time if checks remain pending), with pending CI clearly identified.
- **Sum of feature elapsed durations**: sum of each feature's start-to-terminal time, including CI wait; this may exceed calendar duration when workers run in parallel.
- **Agent active time**: sum of the seven non-CI phase durations across features, separately from CI waiting time.
- One table per agent/feature with the eight requested phase durations, total elapsed duration, and any unknown/approximate values.

Report the proposed and approved batch, each resulting feature ID and PR URL, verification outcomes, and every blocked or skipped feature with the reason and its current claim/lease state. Distinguish ready PRs from tasks that still need work. Leave all PRs open for the user; do not merge them.
