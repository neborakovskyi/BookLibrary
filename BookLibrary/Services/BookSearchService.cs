using BookLibrary.Interfaces;
using BookLibrary.Models;

namespace BookLibrary.Services;

/// <summary>
/// Searches books by a title substring — case-insensitive, no fuzzy logic
/// (SRP — searching only).
/// </summary>
public sealed class BookSearchService : IBookSearchService
{
    /// <inheritdoc/>
    public IReadOnlyList<Book> SearchByTitle(IEnumerable<Book> books, string titlePart)
    {
        ArgumentNullException.ThrowIfNull(books);

        if (string.IsNullOrWhiteSpace(titlePart))
            throw new ArgumentException(
                "Search term must not be null or whitespace.", nameof(titlePart));

        return books
            .Where(b => b.Name.Contains(titlePart.Trim(),
                                         StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }
}
