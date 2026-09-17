namespace BookLibrary.Exceptions;

/// <summary>
/// Thrown when a lookup operation finds no matching book.
/// Callers that expect a nullable result should use
/// <see cref="Interfaces.IBookSearchService.SearchByTitle"/> instead.
/// </summary>
public sealed class BookNotFoundException : BookLibraryException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookNotFoundException"/> class with a specified search term.
    /// </summary>
    /// <param name="searchTerm"></param>
    public BookNotFoundException(string searchTerm)
        : base($"No book found matching title fragment: \"{searchTerm}\".") { }
}
