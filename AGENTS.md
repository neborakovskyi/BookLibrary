# BookLibrary agent instructions

## Project overview
This repository is a .NET 8/10 class library for managing a book collection with XML persistence and search/sort services.

## Working rules
- Prefer small, focused changes over broad refactors.
- Keep behavior compatible with the existing XML schema: root element must be `Books`, and each `Book` contains `Name`, `Author`, and `Pages`.
- Maintain nullable reference typing and existing `ArgumentException` / `InvalidDataException` style expectations.
- Favor explicit validation and clear exceptions over silent fallback behavior.
- Preserve the current public API unless the task specifically requires a change.
- Prefer the BCL (`System.Xml.Linq`, `List<T>`, LINQ) over adding new dependencies.

## Testing
- Validate changes with the smallest relevant `dotnet test` command possible.
- Follow TDD when fixing a bug: add or update a failing test first, then implement the fix.
- Typical verification command:
  ```bash
  dotnet test --filter "<relevant test name or class>"
  ```

## Coding conventions
- Use file-scoped namespaces.
- Prefer clear method names and XML summary comments for public APIs.
- Keep collection/sorting/search logic deterministic and case-insensitive where the tests expect it.
- Return snapshots for read-only collection access when the contract requires a snapshot, not a live list reference.

## Repo-specific guidance
- The main library code lives under `BookLibrary/`.
- Tests live under `BookLibrary.Tests/`.
- Example XML data is in `sample-books.xml`.
- The documentation in `README.md` should stay aligned with the current code and behavior.
