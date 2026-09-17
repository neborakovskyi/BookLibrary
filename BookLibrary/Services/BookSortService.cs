using BookLibrary.Interfaces;
using BookLibrary.Models;

namespace BookLibrary.Services;

/// <summary>
/// Sorts a book sequence: primary = Author, secondary = Name,
/// both ordinal / case-insensitive (SRP — sorting only).
/// </summary>
public sealed class BookSortService : IBookSortService
{
    private static readonly StringComparer Comparer =
        StringComparer.OrdinalIgnoreCase;

    /// <inheritdoc/>
    public IReadOnlyList<Book> Sort(IEnumerable<Book> books)
    {
        ArgumentNullException.ThrowIfNull(books);

        return books
            .OrderBy(b => b.Author, Comparer)
            .ThenBy(b => b.Name,   Comparer)
            .ToList()
            .AsReadOnly();
    }
}
