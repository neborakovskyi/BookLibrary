using BookLibrary.Models;

namespace BookLibrary.Interfaces;

/// <summary>
/// Defines async load/save operations for a collection of books (DIP).
/// Swap XML for JSON/DB by providing a different implementation.
/// </summary>
public interface IBookRepository
{
    /// <summary>
    /// Loads books from the underlying store asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The loaded books.</returns>
    /// <exception cref="Exceptions.BookPersistenceException">
    /// Thrown on I/O failure or schema mismatch.
    /// </exception>
    Task<IReadOnlyList<Book>> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists <paramref name="books"/> to the underlying store asynchronously.
    /// </summary>
    /// <param name="books">Books to persist.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="Exceptions.BookPersistenceException">
    /// Thrown on I/O failure.
    /// </exception>
    Task SaveAsync(IEnumerable<Book> books, CancellationToken cancellationToken = default);
}
