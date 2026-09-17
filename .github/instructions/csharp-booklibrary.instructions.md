---
applyTo: "**/*.cs"
---

# C# conventions for BookLibrary

## Required behaviors
- Keep file-scoped namespaces.
- Prefer `ArgumentNullException.ThrowIfNull(...)` for null guards.
- Use `ArgumentException` for invalid string paths or blank search terms.
- Preserve schema validation behavior for XML input.
- Do not change existing exception contracts without updating tests.

## Implementation preferences
- Prefer small private helpers over large monolithic methods.
- Keep methods side-effect free where practical.
- When exposing list data, prefer a defensive copy when a snapshot is required.
- Use LINQ only when it keeps code clearer and more readable.

## Test-first workflow
- If a bug is reported or a failing test exists, reproduce it and fix the root cause.
- Add or update a focused unit test before making the fix when the change is behaviorally significant.
- Verify with a targeted `dotnet test` command.
