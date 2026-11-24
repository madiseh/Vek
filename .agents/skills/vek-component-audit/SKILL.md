---
name: vek-component-audit
description: Find and fix Vek component behavior bugs by reproducing the UI issue, adding a focused failing regression test, correcting the cause, and verifying the fix in tests and the UI.
---

# Vek Component Audit

Use this skill for any Vek component when the user wants to check, diagnose, or fix whether a behavior works correctly. The goal is a verified behavior fix protected by a project-appropriate regression test. Do not specialize the workflow to a particular component, feature, control, or example.

## Workflow

### 1. Establish the behavior contract

- Read repository instructions and the relevant project documentation before editing.
- Restate the reported user-visible behavior as an observable contract: starting state, action or action sequence, expected result, and actual result.
- Establish expected behavior independently from the implementation using the user's intent, public API docs, existing tests, and established examples. If these conflict or leave a material ambiguity, ask the user before making a behavior-changing fix.
- If the user names a component without a specific behavior, derive a bounded audit from its primary documented flows and state transitions. Do not attempt exhaustive testing of every parameter combination.
- Keep published site observations separate from the current checkout. Record the site/version tested; use the local app or Test Viewer to verify the code being changed.

### 2. Reproduce the failure in the UI

- Use the available Playwright MCP when the issue is interactive or user-visible. Prefer the project's Test Viewer or local app for current-branch behavior; use a published docs site to inspect that published version only.
- Inspect the page and component before interacting. Scope locators to the target component surface, since labels can repeat elsewhere on a docs page.
- Replay the user's exact steps in order. Capture meaningful state before and after each action, including state that should remain unchanged. A successful click alone does not prove the behavior worked.
- Test sequences as well as isolated actions: a control may work from its initial state but fail after another action changes the component's value, selection, mode, or navigation state.
- Reload or recreate the component to establish a known starting state before a repeat run. Repeat the exact sequence when practical. Wait for observable state, not fixed delays.
- If the problem cannot be reproduced, do not guess at a fix. Compare the user's environment with the test environment and report what is still needed to reproduce it.

### 3. Trace the cause and derive the edge case

- In this repository, use codebase-memory graph tools first for symbol and call discovery; inspect the relevant source, callers, tests, and showcase. Treat source as ground truth when graph coverage is incomplete.
- Trace the failing action from the component's public boundary to the state update or render that produces the observed result. Compare it with a working path through the same component when useful.
- Derive the regression case from the reproduced failure. Include the preceding state transition when it is necessary to trigger the bug. Add relevant boundary cases only when the component contract or root cause makes them meaningful; do not generate a generic edge-case checklist for every component.
- Check existing tests first. Add a test only when it protects a distinct observable behavior and can fail on the reproduced bug.
- Choose the cheapest test seam that faithfully catches the defect. For Vek component behavior, prefer a focused bUnit test. If the defect depends on browser layout, JavaScript, focus, or another behavior bUnit cannot observe, use the appropriate Viewer/Playwright test and explain why a unit test alone is insufficient.

### 4. Test first, then fix

- Write one focused regression test for the minimized failure before changing production code.
- Run the narrowest scoped project test command from repository guidance. Confirm it fails at the intended assertion for the reported behavior. If it passes, errors, or fails for an unrelated reason, correct the test or find the proper seam before editing implementation code.
- Apply the smallest change that fixes the root cause. Avoid unrelated refactoring or speculative behavior changes.
- Rerun the same test and confirm it passes. Then run the relevant nearby tests or scoped build required by repository guidance.
- Replay the original UI sequence against the changed checkout and verify the user-visible result. Keep the UI check and test result as separate evidence; one does not replace the other.
- Update component docs or examples when the fix changes public or demonstrated behavior. Follow repository formatting and test rules, including restrictions on sleeps and asynchronous test patterns.

### 5. Maintain the skill and report evidence

- After each implementation or test change in the chat, update this skill with reusable guidance supported by the verified work. Keep it component-neutral: generalize the test technique or remove transient example details instead of hard-coding a specific bug, control, label, or value.
- Report the reproduced steps, root cause, files changed, the regression test's red and green results, and the post-fix UI result. List skipped checks and why. Do not claim a fix is complete while the regression test or requested UI replay remains failing or unverified.

## Vek verification reference

Use the target repository's current `AGENTS.md` and governance docs as the source for test commands and component-specific rules. For C# or Razor behavior, the default is a filtered `dotnet test` of `tests/Components/Vek.UnitTests/Vek.UnitTests.csproj` with `-p:SkipBunCompile=true`, unless the changed behavior needs rebuilt assets or a different project. Run the exact red and green test filter using the repository's Microsoft.Testing.Platform syntax.
