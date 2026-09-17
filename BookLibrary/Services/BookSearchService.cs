using BookLibrary.Interfaces;
using BookLibrary.Models;
using System.Text.RegularExpressions;

namespace BookLibrary.Services;

/// <summary>
/// Searches books by a name substring — case-insensitive, no fuzzy logic
/// (SRP — searching only).
/// </summary>
public sealed class BookSearchService : IBookSearchService
{
    /// <inheritdoc/>
    public IReadOnlyList<Book> SearchByName(IEnumerable<Book> books, string namePart)
    {
        ArgumentNullException.ThrowIfNull(books);

        if (string.IsNullOrWhiteSpace(namePart))
            throw new ArgumentException(
                "Search term must not be null or whitespace.", nameof(namePart));

        var term = namePart.Trim();

        // Match whole words only so a short term like "It" doesn't match inside "Little".
        var pattern = $"\\b{Regex.Escape(term)}\\b";

        return books
            .Where(b => Regex.IsMatch(b.Name ?? string.Empty, pattern, RegexOptions.IgnoreCase))
            .ToList()
            .AsReadOnly();
    }
}
