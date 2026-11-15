# Testing Standards & Verification Guidelines

## Overview

This document defines the testing architecture, runner invocation syntax, concurrency constraints, and accessibility verification practices for the **Vek** repository.

---

## 1. Testing Taxonomy

| Test Project | Scope & Technologies | Verification Target |
| :--- | :--- | :--- |
| `src/Vek.UnitTests/` | bUnit, NUnit, Microsoft.Testing.Platform | Component behavior, reactive state transitions, DOM markup, event handling |
| `src/Vek.UnitTests.Analyzers/` | Roslyn Microsoft.CodeAnalysis.Testing | Compile-time analyzer diagnostics, code fixes, and parameter migration hints |
| `src/Vek.UnitTests.Docs/` | Generated bUnit documentation tests | Parity and zero-exception rendering of all documentation examples |
| `src/Vek.UnitTests.Viewer/` | Blazor WASM/Server interactive host | Isolated visual reproduction and APG accessibility tree verification |
| `src/Vek.UnitTests.Shared/` | Shared testing infrastructure | Common fixtures, bUnit test contexts, mock services, and assertion helpers |

---

## 2. CLI Execution & Runner Commands

Vek uses **Microsoft.Testing.Platform** (pinned via `global.json`), not legacy VSTest. All runner-specific arguments must be passed after the `--` separator.

### Default Local Loop
Execute scoped tests without recompiling Bun frontend assets using `-p:SkipBunCompile=true`:

```bash
dotnet test --project src/Vek.UnitTests/Vek.UnitTests.csproj --no-restore -p:SkipBunCompile=true -- --filter "FullyQualifiedName~<FixtureOrTestName>" --output Normal --no-ansi --hangdump --hangdump-timeout 30s
```

### Multi-Test Iteration with Cached Build
When executing multiple test runs against the same compilation, build once and pass `--no-build`:

```bash
# 1. Build test graph once
dotnet build src/Vek.UnitTests/Vek.UnitTests.csproj --no-restore -p:SkipBunCompile=true --nologo

# 2. Run targeted test suites rapidly
dotnet test --project src/Vek.UnitTests/Vek.UnitTests.csproj --no-build --no-restore -- --filter "FullyQualifiedName~MenuTests" --output Normal --no-ansi --hangdump --hangdump-timeout 30s
dotnet test --project src/Vek.UnitTests/Vek.UnitTests.csproj --no-build --no-restore -- --filter "FullyQualifiedName~PopoverTests" --output Normal --no-ansi --hangdump --hangdump-timeout 30s
```

### Critical CLI Rules
- **No VSTest Blame Flags**: Never use `--blame-hang` or `--blame`; Microsoft.Testing.Platform rejects these. Always use `--hangdump` and `--hangdump-timeout 30s`.
- **Zero Tests Ran is a Failure**: If the output reports "Zero tests ran", treat it as a failure caused by an invalid filter or malformed argument.
- **Process Cleanup on Windows**: Leftover `Vek.UnitTests.exe` processes lock output assemblies, producing `MSB3027` errors. Stop stuck test hosts in this checkout using PowerShell:
  ```powershell
  Get-Process Vek.UnitTests -ErrorAction SilentlyContinue | Where-Object { $_.Path -like "$PWD\src\*" } | Stop-Process -Force
  ```

---

## 3. Test Isolation & Concurrency Invariants

### Parallelization Architecture
- **Fixture-Level Parallelism**: Test fixtures run in parallel (`[assembly: Parallelizable(ParallelScope.Fixtures)]`) with a fresh instance per test case (`InstancePerTestCase`).
- **No Intra-Fixture Parallelism**: bUnit's internal renderer requires that test code and render dispatches share a single execution thread (memory-coherence locking). Never switch to `ParallelScope.All`.
- **NonParallelizable Exceptions**: Tests with timing sensitivity or shared static dependencies (e.g. `DialogTests`, debounced inputs under parallel CPU load) are pinned with `[NonParallelizable]`. Do not remove this attribute without empirical concurrency proof under high parallel load.

### Asynchronous & Timing Rules
- **Async Return Types**: Async test methods and helper utilities must return `Task`, never `async void`.
- **Forbidden Patterns**:
  - `Task.Delay(...)` or `Thread.Sleep(...)` used as arbitrary sleeps.
  - Sync-over-async blocking: `.Result`, `.Wait()`, `.GetAwaiter().GetResult()`.
  - Polling loops driven by wall-clock timers.
- **Fake Time Provider**:
  - For bUnit tests requiring timer or debounce verification, register fake time via `Context.AddFakeTimeProvider()`.
  - Advance time explicitly via `FakeTimeProvider.Advance(TimeSpan)`.
- **Task Gates**:
  - Use `TaskCompletionSource` with `TaskCreationOptions.RunContinuationsAsynchronously`.
  - When awaiting gates subject to timeouts, use `[CancelAfter]` and pass `NUnit.Framework.TestContext.CurrentContext.CancellationToken`.

---

## 4. Accessibility (APG) Verification

Vek components must strictly conform to W3C ARIA Authoring Practices Guide (APG) specifications.

### Verification Workflow
1. **Isolated Viewer Hosting**: Host the component in `src/Vek.UnitTests.Viewer/` using `chrome=none`.
2. **Snapshot Accessibility Tree**:
   - Inspect the live accessibility tree via browser developer tooling or CDP (`Accessibility.getFullAXTree`).
   - Validate roles, accessible names, and initial attributes (`aria-expanded="false"`, `aria-haspopup="menu"`).
3. **Step Keyboard Navigation**:
   - Simulate authentic keyboard interactions (Tab, Arrow keys, Enter, Escape).
   - Verify dynamic attribute mutations (`aria-expanded="true"`, `aria-activedescendant="item-id"`, `aria-selected="true"`).
4. **Assert in bUnit**:
   - Transcribe verified patterns into deterministic bUnit unit test assertions verifying element attributes before and after user events.
