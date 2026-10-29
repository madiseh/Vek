# Git Commit Messages

Vek uses [Conventional Commits 1.0.0](https://www.conventionalcommits.org/en/v1.0.0/) for commit messages. The specification gives commit headers machine-readable meaning; the project rules below make the convention consistent across this repository.

## Message Format

```text
<type>[optional scope][optional !]: <description>

[optional body]

[optional footer(s)]
```

The header is required. The body and footers are optional. Leave one blank line before a body and before the footer section.

## Specification Requirements

- Every commit starts with a type, followed by an optional scope, an optional breaking-change marker, a colon, a space, and a short description.
- `feat` means the commit adds a feature. It corresponds to a SemVer `MINOR` change.
- `fix` means the commit fixes a bug. It corresponds to a SemVer `PATCH` change.
- A scope is optional. When used, it is a noun in parentheses that identifies the affected area, such as `fix(components): ...`.
- A body is optional and may contain free-form text and multiple paragraphs.
- Footers are optional. A footer uses a token followed by `: ` or ` #` and a value. Multi-word tokens use hyphens, such as `Reviewed-by`.
- A breaking change is indicated either by `!` immediately before the header colon, by a `BREAKING CHANGE: ` footer, or by both. A breaking change corresponds to a SemVer `MAJOR` change, regardless of the commit type.
- If used as a footer token, `BREAKING CHANGE` is uppercase and is followed by a colon, a space, and an explanation.
- The specification permits additional types. Types other than `feat` and `fix` have no prescribed SemVer effect unless the commit indicates a breaking change.

## Vek Types

Use lowercase types. The following set is the Vek project vocabulary; only the meanings and version effects described above are assigned by the Conventional Commits specification.

| Type | Use it for |
| --- | --- |
| `feat` | A user-visible capability added to the library or demo. |
| `fix` | A correction to behavior that does not add a new capability. |
| `build` | Build configuration or dependency changes. |
| `chore` | Repository maintenance that does not fit another type. |
| `ci` | GitHub Actions and other automation changes. |
| `docs` | Documentation-only changes. |
| `perf` | A change made to improve runtime or build performance. |
| `refactor` | Internal code changes that preserve externally visible behavior. |
| `revert` | A change that reverses an earlier commit. |
| `style` | Formatting or style changes without behavior changes. |
| `test` | Test project, fixture, or test case changes. |

## Scopes

Scopes are optional. Use a short lowercase noun that names the area changed. Suggested Vek scopes include:

- `components`
- `demo`
- `ports`
- `simulated-data`
- `tests`
- `build`
- `docs`
- `release`

Add a scope when it makes the header clearer; do not invent a scope just to fill the parentheses.

## Examples

```text
feat(components): add a button component
fix(components): preserve disabled state after parameter updates
docs(architecture): explain the demo data adapter
test(components): cover button rendering
build: centralize package versions
ci(release): publish the package from a version tag
```

Breaking-change examples:

```text
feat(components)!: rename the button variant parameter
```

```text
feat(components): rename the button variant parameter

BREAKING CHANGE: replace `Variant` with `Appearance` in the public component API.
```

## Practical Guidance

- Keep each commit focused on one logical change. Split unrelated changes when practical.
- Make the description short and specific. A body is useful when the reason or impact is not clear from the header.
- Issue references and other footers are optional; this repository does not require an issue number for every commit.
- The specification does not define a mandatory description case, punctuation rule, or maximum subject length. Keep wording readable and consistent with nearby commits.
- Conventional Commit types describe changes; they do not publish a NuGet package by themselves. Vek's publishing workflow is triggered by version tags.
- The specification does not prescribe revert semantics. For a revert, Vek recommends a `revert:` type and a `Refs: <commit-sha>` footer.
