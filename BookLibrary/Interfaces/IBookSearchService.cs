using BookLibrary.Models;

namespace BookLibrary.Interfaces;

/// <summary>
/// Defines search behaviour for a collection of books (ISP).
/// </summary>
public interface IBookSearchService
{
    /// <summary>
    /// Returns all books whose title contains <paramref name="titlePart"/>
    /// (case-insensitive substring match).
    /// </summary>
    /// <param name="books">The collection to search.</param>
    /// <param name="titlePart">The non-empty search term.</param>
    /// <returns>Matching books; empty list when nothing matches.</returns>
    /// <exception cref="ArgumentException">When <paramref name="titlePart"/> is null or whitespace.</exception>
    IReadOnlyList<Book> SearchByTitle(IEnumerable<Book> books, string titlePart);
}
