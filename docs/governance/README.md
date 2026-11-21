# Vek Governance & Architectural Documentation

Welcome to the central governance, architecture, and AI-agent knowledge repository for **Vek** (Modern, Fast & Extensible Component Suite for Blazor).

---

## Purpose & Scope

This documentation directory serves two complementary purposes:
1. **Engineering Specifications**: Serves as the authoritative architectural blueprint and engineering reference for human developers contributing to Vek.
2. **AI-Friendly Knowledge Base**: Provides structured, high-context operational guidelines and domain principles for AI coding agents to make informed, non-breaking, and idiomatic contributions.

---

## Document Index & Routing Policy

To maintain repository cleanliness and prevent documentation sprawl, all project-level knowledge, architectural patterns, and user directives must be recorded strictly within these canonical documents:

| Document | Scope & Focus | When to Consult & Update |
| :--- | :--- | :--- |
| **[architecture.md](architecture.md)** | System architecture, multi-targeting (`net8.0`/`net9.0`/`net10.0`/`net11.0`), project dependencies, docs site layers and route composition, component lifecycle, `ParameterState<T>`, root rendering, CSS design tokens, Bun asset pipeline, and Test Viewer architecture. | Consult before introducing new architectural abstractions or component foundations. Update when structural patterns, runtime targets, or component pipelines evolve. |
| **[development-rules.md](development-rules.md)** | C# coding standards, XML documentation rules (`<summary>`, constructor guidelines, member comments), parameter registration, `UserAttributes` precedence, Roslyn analyzer rules, dependency policies, and Git commit/PR title conventions. | Consult before writing or refactoring C#/Razor code or preparing commits and pull requests. Update whenever new coding constraints, analyzer requirements, API conventions, or Git conventions are established. |
| **[testing-standards.md](testing-standards.md)** | Test taxonomy (bUnit, Analyzer, Docs, Viewer), Microsoft.Testing.Platform runner syntax, fake time (`FakeTimeProvider`), fixture isolation, parallelization rules, and APG accessibility verification. | Consult when authoring or running tests. Update when test infrastructure, execution flags, or verification practices change. |

---

## Anti-Sprawl Policy for AI Agents

All AI coding agents operating on the Vek codebase must adhere to the following rules:

1. **Existing-First In-Place Updates**:
   - Do **NOT** create ad-hoc documentation files for single features, small bug fixes, or routine adjustments.
   - When a session produces new operational rules, user preferences, or architectural decisions, integrate them directly into the relevant existing document in `docs/governance/`.
2. **Threshold for New Documents**:
   - Creating a new markdown file inside `docs/governance/` requires introducing a fundamentally new subsystem (e.g. an entirely new compiler pipeline, new client runtime host, or major external platform integration) that cannot logically fit into existing documents.
3. **Task Completion Protocol**:
   - At the conclusion of every feature, refactor, or bug fix, execute the **`/review-finalize`** skill to synchronize learned rules and verify repository integrity before concluding the task.
