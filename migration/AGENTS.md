# Autonomous Feature Migration Rules

This document governs the Autonomous Feature Migration Pipeline for Vek. It establishes the operational boundaries, contracts, and quality gates for multi-agent feature migration.

---

## 1. Core Principle

> **Project B is the Reference for Behavior; Project A (Vek) is the Reference for Architecture.**

The agents must never simply copy code from Project B to Project A. Instead, they must observe and deconstruct the business logic, edge-case behaviors, and interaction models of the feature in Project B, and reconstruct that feature 100% natively using Project A's architecture, patterns, abstractions, and design tokens.

---

## 2. Operational Rules

### Source Project (Project B)
- **Strictly Read-Only**: Project B is strictly a reference source. No files in Project B may ever be modified, created, or deleted.
- **Behavior Preservation**: Preserve the observable runtime behavior, edge cases, formulas, and user-facing interactions of the source feature.
- **Do Not Copy Architecture**: Ignore the internal class hierarchies, proprietary frameworks, legacy paradigms, or patterns of Project B.

### Target Project (Project A - Vek)
- **Sole Writable Target**: Project A is the only project where code, tests, and documentation are authored.
- **Architectural Conformance**: Follow all repository guidelines in `docs/governance/` and `AGENTS.md`:
  - `ParameterState<T>` registered in constructors via `CreateRegisterScope()`.
  - Auto-properties only for component parameters.
  - Functional subfolder discipline (e.g., `src/Vek/Components/DataGrid/` subdirectories: `Cells/`, `Columns/`, `Filtering/`, `Grouping/`, `Models/`).
  - Allocation-conscious rendering (avoid extra wrapper components or allocations per cell/row).
  - Design token integration via `CssBuilder`, `StyleBuilder`, and Vek theme variables.
- **Reuse Existing Abstractions**: Always prefer existing Vek utilities and components (e.g., `Splitter.GetFragments`, `VekTextField`, `VekButton`) over inventing duplicate helpers.

### Anonymity & Brand Cleanliness
- **Zero Third-Party Brand Traces**: Under no circumstances may third-party commercial UI vendors, product suites, or proprietary brand names be referenced in commit messages, branch names, PR titles, PR descriptions, source code comments, or documentation.
- **Functional Terminology**: Always describe features in vendor-neutral, capability-oriented terms (e.g., "DataGrid Data Export to Excel and CSV", "Search Panel with Live Highlighting").

### Scope & Isolation
- **One Feature Per Task**: Implement exactly one feature per migration cycle.
- **Isolated Branch**: Every feature migration must execute in an isolated git worktree and feature branch (e.g., `feature/migration-<id>-<slug>`).
- **No Unrelated Refactoring**: Do not refactor unrelated files, reformat unchanged trees, or alter existing public API contracts unnecessarily.

### Quality Gates & Review
- **Mandatory CI Build & Test**:
  - `dotnet build` with 0 errors and 0 warnings (`TreatWarningsAsErrors=true`).
  - Scoped `dotnet test` with 100% passing tests.
  - Final pre-PR formatting pass (`dotnet format whitespace`).
- **Comprehensive Test Coverage**: Every migrated feature must include unit tests and bUnit component tests exercising the reconstructed behaviors and edge cases.
- **Bounded Repair Attempts**: The automated repair loop between Reviewer/Test Gate and Implementer is capped at **2 iterations**. If the build or tests still fail after 2 retries, the attempt is marked as `Failed` / `Needs Human Intervention` and halts.
- **Human Merge Only**: The automated pipeline never merges directly into `master`. It opens a detailed Pull Request for human review and sign-off.

---

## 3. Orchestration & State Lifecycle

The migration pipeline is driven by `migration/scripts/orchestrator.py`, which synchronizes state bidirectionally between `migration/backlog.json` and the individual contract specifications in `migration/specifications/XXX-*.md`:

### Common Commands
- **List All Features**:
  ```bash
  python migration/scripts/orchestrator.py --list
  ```
  Displays the full status matrix of all 30 features with dependencies, statuses (`[COMPLETED]`, `[IN PROGRESS]`, `[READY]`, `[BACKLOG]`), and PR URLs.

- **Pick / Start a Specific Feature**:
  ```bash
  python migration/scripts/orchestrator.py --pick 003
  ```
  Validates prerequisites, sets feature state to `in_progress` across both `backlog.json` and the `.md` specification, and outputs the target branch and instructions.

- **Complete a Feature**:
  ```bash
  python migration/scripts/orchestrator.py --complete 003 --pr https://github.com/madiseh/Vek/pull/12
  ```
  Transitions feature state to `completed` in both `backlog.json` and the `.md` contract, records completion timestamp and PR URL, and automatically promotes any dependent backlog items whose prerequisites are now satisfied to `ready`.

- **Synchronize Specifications and Backlog**:
  ```bash
  python migration/scripts/orchestrator.py --sync
  ```
  Scans all specification files, reads their metadata headers, resolves dependencies, and ensures `backlog.json` and Markdown contracts stay in 100% synchronization.

- **Run Quality Gates**:
  ```bash
  python migration/scripts/orchestrator.py --verify
  ```
  Runs build, unit tests, whitespace formatting, and brand cleanliness verification.
