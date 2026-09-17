using BookLibrary.Models;

namespace BookLibrary.Interfaces;

/// <summary>
/// Defines search behaviour for a collection of books (ISP).
/// </summary>
public interface IBookSearchService
{
    /// <summary>
    /// Returns all books whose name contains <paramref name="namePart"/>
    /// (case-insensitive substring match).
    /// </summary>
    /// <param name="books">The collection to search.</param>
    /// <param name="namePart">The non-empty search term.</param>
    /// <returns>Matching books; empty list when nothing matches.</returns>
    /// <exception cref="ArgumentException">When <paramref name="namePart"/> is null or whitespace.</exception>
    IReadOnlyList<Book> SearchByName(IEnumerable<Book> books, string namePart);
}
