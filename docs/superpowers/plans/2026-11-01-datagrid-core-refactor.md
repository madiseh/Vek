# DataGrid Core Architecture Refactoring Implementation Plan

**Scope revision:** After the initial filter and sort stages, the user authorized migration of all remaining shared methods. Paging, column commands, selection, detail expansion, group expansion, and editing now use the shared host extensions. See [the implemented command map](../../1.governance/datagrid-shared-operations.md). Native lifecycle, rendering, and provider invocation remain in adapters. The broader model/template consolidation tasks below are separate from this command migration.

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

- [x] Establish a Core-only test project and move existing engine tests into it.
- [x] Test optional host workflow continuation ordering, omission, asynchronous completion, and error propagation before implementing `IVekDataGridHost` and shared extensions.
- [x] Replace duplicated query wrappers with the shared host surface in both adapters; keep native notifications in optional semantic capabilities.
- [ ] Deferred contract/presentation work: consolidate remaining template contexts and strongly typed aggregate definitions; this is separate from shared command migration.
- [x] Move shared selection, expansion, edit state, query/paging and column operation rules out of adapters, protecting newly exposed boundary cases first.
- [x] Complete native WPF command integration for shared state and detail/edit template contexts; retain native projections and event translation in that adapter.
- [ ] Deferred contract/presentation work: split remaining presentation model files by type.
- [x] Run Core, Blazor, WPF, and demo suites; audit resolved Core dependencies, source semantics, public surface shadowing, and remaining duplicate command algorithms. Final solution run: 201 passed.
- [x] Update architecture documentation to distinguish implemented shared commands from illustrative sketches and deferred contract/presentation work.

## Review focus

- External UI updates must dispatch on the adapter thread, with awaited errors and no mandatory rendering callback.
- Filter/page operations must update both Core state and the rows visible in each adapter.
- Replacing source rows must reconcile selection and expanded rows without introducing a second authoritative state store.
- Edit rejection must retain the draft; cancellation and success must clear the same authoritative session.
- Every shared template slot must remain strongly typed and usable in both adapters.
