using BookLibrary.Exceptions;

namespace BookLibrary.Models;

/// <summary>
/// Represents a single book.  Immutable after construction (SRP: data only).
/// Mutation produces a new instance via <c>With*</c> factory methods.
/// </summary>
public sealed class Book
{
    /// <summary>
    /// Gets the title of the book.  Never null or whitespace.
    /// </summary>
    public string Name  { get; }
    /// <summary>
    /// Gets the author of the book.  Never null or whitespace.
    /// </summary>
    public string Author { get; }
    /// <summary>
    /// Gets the number of pages in the book.  Always greater than zero.
    /// </summary>
    public int    Pages  { get; }

    /// <exception cref="BookValidationException">
    /// Thrown for blank title / author or non-positive page count.
    /// </exception>
    public Book(string title, string author, int pages)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BookValidationException(nameof(Name),
                "Name must not be null or whitespace.");

        if (string.IsNullOrWhiteSpace(author))
            throw new BookValidationException(nameof(Author),
                "Author must not be null or whitespace.");

        if (pages <= 0)
            throw new BookValidationException(nameof(Pages),
                $"Pages must be greater than zero; got {pages}.");

        Name  = title.Trim();
        Author = author.Trim();
        Pages  = pages;
    }

    // Non-destructive "edit" helpers (ISP: callers only touch what they need)
    /// <summary>
    /// Creates a new book instance with the specified title.
    /// </summary>
    /// <param name="title">The title of the book.</param>
    /// <returns>A new book instance with the specified title.</returns>
    public Book WithTitle(string title)   => new(title, Author, Pages);
    /// <summary>
    /// Creates a new book instance with the specified author.
    /// </summary>
    /// <param name="author">The author of the book.</param>
    /// <returns>A new book instance with the specified author.</returns>
    public Book WithAuthor(string author) => new(Name, author, Pages);
    /// <summary>
    /// Creates a new book instance with the specified number of pages.
    /// </summary>
    /// <param name="pages">The number of pages in the book.</param>
    /// <returns>A new book instance with the specified number of pages.</returns>
    public Book WithPages(int pages)      => new(Name, Author, pages);
    /// <summary>
    /// Returns a string representation of the book in the format "Author — Name (Pages pp.)".
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"{Author} — {Name} ({Pages} pp.)";
}
