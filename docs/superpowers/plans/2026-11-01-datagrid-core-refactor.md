# DataGrid Core Architecture Refactoring Implementation Plan

**Scope revision:** The user limited the active work to `ApplyFilterAsync` and `RemoveFilterAsync` through `IVekDataGridHost.Engine` and `AfterCoreChangeAsync`. The broader tasks below are deferred. Only these two filtering operations use the shared extensions in this step; sort, paging, grouping, selection, and editing remain outside the active refactoring.

**Goal:** Refactor the complete DataGrid family against the Core Architecture Rule while preserving observable behavior.

**Spec:** `docs/1.governance/core-architecture.md`

**Architecture:** One neutral Core engine owns shared rules and state. Adapters implement a host contract and reuse shared extension operations. Optional query/selection/edit continuations translate results into native lifecycle and binding work; the base host requires no UI hook.

**Execution:** Inline in the current checkout, preserving existing staged and unstaged work. The user has authorized implementation. No commits or publishing are part of this request.

## Constraints

- Plain `net10.0`, zero platform references, names, or specialized algorithms in Core.
- Shared contracts and behavior are canonical; strongly typed native template parameters remain opaque.
- No per-operation forwarding methods in adapters; native lifecycle and binding members stay native.
- One top-level class/interface/record per source file, shared imports centralized.
- Existing adapter behavior is protected by the existing suites; new tests must protect a distinct observable contract.

## Tasks

- [ ] Establish a Core-only test project and move existing engine tests into it.
- [ ] Test optional host workflow continuation ordering, omission, asynchronous completion, and error propagation before implementing `IVekDataGridHost` and shared extensions.
- [ ] Replace duplicated query wrappers with the shared host surface in both adapters; keep native notifications in optional semantic capabilities.
- [ ] Consolidate shared template contexts and strongly typed aggregate definitions into Core; update adapters, demos, and tests together.
- [ ] Move shared selection, expansion, edit state, query/paging and column operation rules out of adapters, protecting newly exposed boundary cases first.
- [ ] Complete native WPF integration for shared state and template slots; retain only native projections and event translation in that adapter.
- [ ] Split remaining aggregate source files by responsibility/type and centralize imports.
- [ ] Run Core, Blazor, WPF, and demo suites; audit resolved Core dependencies, source semantics, public surface shadowing, and remaining duplicate algorithms.
- [ ] Update architecture documentation to distinguish the final implementation from illustrative sketches and record actual validation results.

## Review focus

- External UI updates must dispatch on the adapter thread, with awaited errors and no mandatory rendering callback.
- Filter/page operations must update both Core state and the rows visible in each adapter.
- Replacing source rows must reconcile selection and expanded rows without introducing a second authoritative state store.
- Edit rejection must retain the draft; cancellation and success must clear the same authoritative session.
- Every shared template slot must remain strongly typed and usable in both adapters.
