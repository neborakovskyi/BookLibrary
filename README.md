# BookLibrary

A C# / .NET 8 class library for managing a book collection with XML persistence.  
Built with **SOLID principles**, **async/await file I/O**, **custom exceptions** and **xUnit tests**.

---

## Project structure

```
BookLibrary.sln
├── BookLibrary/
│   ├── Exceptions/
│   │   ├── BookLibraryException.cs      ← base exception
│   │   ├── BookValidationException.cs   ← invalid field values
│   │   ├── BookPersistenceException.cs  ← I/O / XML schema errors
│   │   └── BookNotFoundException.cs     ← lookup found nothing
│   ├── Interfaces/
│   │   ├── IBookRepository.cs           ← async load/save contract
│   │   ├── IBookSortService.cs          ← sort contract
│   │   ├── IBookSearchService.cs        ← search contract
│   │   └── IBookCollectionService.cs    ← facade consumed by callers
│   ├── Models/
│   │   └── Book.cs                      ← immutable value object
│   └── Services/
│       ├── XmlBookRepository.cs         ← async XML file I/O
│       ├── BookSortService.cs           ← Author → Name sort
│       ├── BookSearchService.cs         ← substring title search
│       └── BookCollectionService.cs     ← facade / DI root
└── BookLibrary.Tests/
    ├── BookTests.cs
    ├── XmlBookRepositoryTests.cs
    ├── BookSortServiceTests.cs
    ├── BookSearchServiceTests.cs
    └── BookCollectionServiceTests.cs    ← NSubstitute mocks
```

---

## SOLID principles applied

| Principle | Where |
|---|---|
| **SRP** | `BookSortService` sorts only; `BookSearchService` searches only; `XmlBookRepository` handles persistence only |
| **OCP** | Swap XML for a DB by implementing `IBookRepository` — no existing code changes |
| **LSP** | Any `IBookRepository` / `IBookSortService` / `IBookSearchService` can be substituted; tests prove this via mocks |
| **ISP** | Four narrow interfaces instead of one fat one — callers reference only what they need |
| **DIP** | `BookCollectionService` depends on interfaces, not concrete classes; concrete types are injected via constructor |

---

## Async / await

`XmlBookRepository` uses:
- `File.ReadAllTextAsync` — non-blocking read  
- `FileStream` with `useAsync: true` + `XmlWriter` with `Async = true` + `WriteToAsync` — non-blocking write  
- Write-to-temp-then-atomic-`File.Move` — prevents corruption on cancellation  
- `CancellationToken` flows through every async method

---

## Custom exceptions

| Exception | When thrown |
|---|---|
| `BookLibraryException` | Base class — catch this to handle *any* library error |
| `BookValidationException` | Blank title/author or non-positive page count; carries `FieldName` |
| `BookPersistenceException` | File not found, malformed XML, missing element; carries `FilePath` |
| `BookNotFoundException` | Reserved for single-result lookups that find nothing |

---

## Build & test

```bash
dotnet restore
dotnet build
dotnet test --logger "console;verbosity=normal"
```

---

## Wiring up (DI example)

```csharp
// Composition root — wire once, use everywhere
var repository = new XmlBookRepository("books.xml");
var service    = new BookCollectionService(
    repository,
    new BookSortService(),
    new BookSearchService());

// Use via interface only
IBookCollectionService lib = service;

await lib.LoadAsync();
lib.Add("The Snow Queen", "Andersen", 80);
lib.Sort();

var results = lib.SearchByTitle("snow");
await lib.SaveAsync();
```

---

## XML format

```xml
<?xml version="1.0" encoding="utf-8"?>
<Books>
  <Book>
    <Name>The Little Mermaid</Name>
    <Author>Andersen</Author>
    <Pages>48</Pages>          <!-- positive integer, required -->
  </Book>
</Books>
```
