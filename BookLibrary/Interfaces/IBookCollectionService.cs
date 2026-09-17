using BookLibrary.Models;

namespace BookLibrary.Interfaces;

/// <summary>
/// High-level facade that coordinates the repository, sort and search
/// services.  External modules depend on this interface, not on any
/// concrete class (DIP).
/// </summary>
public interface IBookCollectionService
{
    /// <summary>Current in-memory book list.</summary>
    IReadOnlyList<Book> Books { get; }

    // ── Persistence ───────────────────────────────────────────────────

    /// <summary>Replaces the in-memory list with books from the store.</summary>
    Task LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>Persists the current in-memory list to the store.</summary>
    Task SaveAsync(CancellationToken cancellationToken = default);

    // ── Mutation ──────────────────────────────────────────────────────

    /// <summary>Adds a book to the in-memory list.</summary>
    void Add(Book book);

    /// <summary>Convenience overload: constructs and adds a book.</summary>
    void Add(string name, string author, int pages);

    /// <summary>Removes all books from the in-memory list.</summary>
    void Clear();

    // ── Query ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a new sorted list without modifying the in-memory list.
    /// </summary>
    IReadOnlyList<Book> GetSorted();

    /// <summary>
    /// Sorts the in-memory list in place.
    /// </summary>
    void Sort();

    /// <summary>
    /// Returns books whose name contains <paramref name="namePart"/>
    /// (case-insensitive substring).
    /// </summary>
    IReadOnlyList<Book> SearchByName(string namePart);
}
