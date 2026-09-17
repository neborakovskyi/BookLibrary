using BookLibrary.Models;

namespace BookLibrary.Interfaces;

/// <summary>
/// Defines sorting behaviour for a collection of books (ISP).
/// </summary>
public interface IBookSortService
{
    /// <summary>
    /// Returns a new sorted list: primary key = Author, secondary key = Name,
    /// both ordinal and case-insensitive.  The source sequence is not modified.
    /// </summary>
    IReadOnlyList<Book> Sort(IEnumerable<Book> books);
}
