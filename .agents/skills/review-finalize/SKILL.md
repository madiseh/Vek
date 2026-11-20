---
name: review-finalize
description: Run upon completion of a task or feature implementation to perform diff review, targeted test verification, documentation synchronization (extracting learned user rules, constraints, or corrections into existing docs/governance/), and commit readiness preparation.
---

# Review & Finalize Protocol for Vek

Execute this skill when completing an implementation, refactoring, bug-fix, or architectural task in Vek. It ensures code quality, targeted test validation, synchronization of newly established rules into project governance documentation, and commit readiness.

---

## Workflow Phases

```
┌─────────────────────────┐
│ 1. Code Diff & Safety   │──► Verify changes are minimal, scoped, and conform to Vek patterns
└───────────┬─────────────┘
            ▼
┌─────────────────────────┐
│ 2. Targeted Validation  │──► Run targeted tests using Vek's Microsoft.Testing.Platform loop
└───────────┬─────────────┘
            ▼
┌─────────────────────────┐
│ 3. Documentation Sync   │──► Extract chat rules & update existing docs/governance/ in-place
└───────────┬─────────────┘
            ▼
┌─────────────────────────┐
│ 4. Formatting & Safety  │──► Run whitespace format (UTF-8 BOM), ensure working tree is clean
└───────────┬─────────────┘
            ▼
┌─────────────────────────┐
│ 5. Structured Report    │──► Deliver 4-part summary and proposed commit message
└─────────────────────────┘
```

---

## Phase 1: Code Diff & Safety Verification

1. **Diff Inspection**:
   - Run `git status -s` and inspect `git diff` for all modified files.
   - Confirm changes are strictly within task scope.
   - Verify no accidental edits, leftover debug statements, temporary files, or commented-out dead code.
2. **Architecture & Standards Alignment**:
   - Verify C# XML documentation conforms to [Development & Documentation Rules](../../../docs/governance/development-rules.md) (concise summaries, no XML above constructors, `//` for private state, `[Category]` on public parameters).
   - Ensure parameter state conventions are followed: auto-properties with `[Parameter, ParameterState]`, registration in `CreateRegisterScope()`, no property setter logic.
   - Verify no `#region` directives are used.

---

## Phase 2: Targeted Validation & Testing

Follow Vek's scoped testing policy:
1. **Targeted Tests First**:
   - Run filtered tests using Microsoft.Testing.Platform runner syntax and `-p:SkipBunCompile=true`:
     ```pwsh
     dotnet test --project tests/Components/Vek.UnitTests/Vek.UnitTests.csproj --no-restore -p:SkipBunCompile=true -- --filter "FullyQualifiedName~<TargetComponentTests>" --output Normal --no-ansi --hangdump --hangdump-timeout 30s
     ```
2. **Analyzer & Code-Fix Tests**:
   - If analyzers or code fixes were touched, run:
     ```pwsh
     dotnet test --project tests/Analyzers/Vek.UnitTests.Analyzers/Vek.UnitTests.Analyzers.csproj --no-restore -- --filter "FullyQualifiedName~<AnalyzerTests>" --output Normal --no-ansi --hangdump --hangdump-timeout 30s
     ```
3. **No Default Full-Solution Runs**:
   - Do not execute full solution tests for localized changes.
4. **Leftover Host Cleanup (Windows)**:
   - If output assemblies are locked with `MSB3027`, stop leftover test hosts:
     ```pwsh
     Get-Process Vek.UnitTests -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "$PWD\src\*" } | Stop-Process -Force
     ```
5. **Failure Protocol**:
   - If tests fail, diagnose and fix the issue before proceeding. Never bypass or ignore test failures.

---

## Phase 3: Conversation Knowledge Extraction & Documentation Sync

Analyze the conversation transcript, user instructions, and chat corrections to identify persistent knowledge that must be preserved for future tasks and agents.

### What to Extract
- **User Directives & Rules**: Any rule, constraint, or convention explicitly stated or corrected by the user during the chat (e.g. "always do X", "never use Y", "wrap Z in this manner").
- **Contradiction Resolution**: If the user corrected an existing rule or specified a behavior that supersedes an existing rule in `docs/governance/`, reconcile and update the rule in-place.
- **Architectural & Design Decisions**: New abstractions, service lifecycles, component patterns, or configuration models introduced during the task.

### Anti-Sprawl & Routing Policy (Strict Existing-First)
- **Do NOT create new files for routine changes, individual rules, or corrections.**
- Route extracted knowledge strictly into existing documents:
  - **Component architecture, layers, parameter state, asset pipeline, viewer** ➔ [`docs/governance/architecture.md`](../../../docs/governance/architecture.md)
  - **Coding standards, XML docs, parameter rules, analyzers, formatting** ➔ [`docs/governance/development-rules.md`](../../../docs/governance/development-rules.md)
  - **Testing commands, runner flags, fixture isolation, accessibility** ➔ [`docs/governance/testing-standards.md`](../../../docs/governance/testing-standards.md)
- **Threshold for New Files**:
  - A new document in `docs/governance/` is permitted **ONLY** if the change introduces an entirely new subsystem or standalone platform not represented in existing docs.

### Update Mechanics
- Edit the target markdown file cleanly.
- Keep phrasing concise, imperative, and consistent with existing document styles.
- Preserve all existing unrelated sections.

---

## Phase 4: Formatting & Git Working Tree Safety

1. **Pre-PR Whitespace Formatting**:
   - Run `dotnet format whitespace` from the repository root targeting modified files:
     ```pwsh
     dotnet format whitespace --folder --include <path/to/changed/files/relative/to/repository-root>
     ```
   - This ensures correct line endings (LF) and applies UTF-8 with BOM (`charset = utf-8-bom`) required by CI.
2. **Git Working Tree Safety**:
   - Confirm that no untracked scratch files, build artifacts, or secret keys remain.
   - **Safety Guarantee**: Keep changes **uncommitted in the working tree** until the user explicitly authorizes a commit. Do not push or modify remote git state.

---

## Phase 5: Structured Finalize Report

Produce a concise, 4-part summary for the user:

### 1. Verification & Test Status
- List tests executed (command + outcome).
- Confirm code diff cleanliness and boundary compliance.

### 2. Documentation Sync Summary
- List documentation files modified or added.
- Highlight specific rules, corrections, or policies extracted from the session and added to `docs/governance/`.

### 3. Git Working Tree Status
- List modified/untracked files currently in the working tree.

### 4. Proposed Commit Message
- Suggest a clear, conventional commit message (e.g. `feat(...)`, `fix(...)`, `docs(...)`) ready for user authorization.
