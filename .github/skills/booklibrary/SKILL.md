# BookLibrary skill

## Purpose
Support work on the BookLibrary C# project, especially collection behavior, sorting/search, XML persistence, and tests.

## When to use this skill
Use this skill when working on:
- collection state and snapshot behavior
- XML load/save logic
- title search and sorting semantics
- fix validation or exception-related regressions
- xUnit tests in this repository

## Workflow
1. Read the targeted files and the failing test before changing code.
2. Confirm the root cause and keep the fix narrow.
3. Prefer a failing test or minimal reproduction.
4. Implement the minimal root-cause fix.
5. Run the smallest relevant `dotnet test` command to validate the behavior.

## Project conventions
- Keep compatibility with the existing XML format.
- Maintain case-insensitive sorting and searching.
- Respect snapshot semantics for exposed collections.
- Prefer repository-level tests over mock-heavy assertions.

## Common validation commands
```bash
dotnet test --filter "BookCollectionTests"
dotnet test --filter "BookCollectionServiceTests"
dotnet test --filter "Books_ReturnsSnapshot_NotLiveReference"
```
