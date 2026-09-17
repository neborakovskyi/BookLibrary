# GitHub Copilot instructions for BookLibrary

## Scope
These instructions apply to all code generation, edits, and test work in this repository.

## Expectations
- Keep the library simple and idiomatic for .NET.
- Use C# 10+ syntax where appropriate, but do not add unnecessary complexity.
- Preserve XML file compatibility and existing test semantics.
- Do not broaden the project surface area unless the task requires it.

## Build and test
Run the smallest relevant verification command, for example:

```bash
dotnet test --filter "Books_ReturnsSnapshot_NotLiveReference"
```

Use the full suite only when a broader regression check is needed.

## Style
- Prefer `public sealed class` for concrete types when appropriate.
- Validate arguments early and throw meaningful exceptions.
- Keep methods small and responsibilities separated.
- Favor `IReadOnlyList<T>` when exposing read-only data.

## Important conventions
- `Books` access should behave like a snapshot when the contract expects one.
- Sorting should be case-insensitive and deterministic.
- Search by title should be substring-based and case-insensitive.
- XML load/save should preserve the document root and element names used by the repository tests.
