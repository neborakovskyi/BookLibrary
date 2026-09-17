namespace BookLibrary;

/// <summary>
/// Represents a single book with a title, author and page count.
/// </summary>
public sealed class Book
{
    /// <summary>Gets or sets the title of the book.</summary>
    public string Name { get; set; }

    /// <summary>Gets or sets the author of the book.</summary>
    public string Author { get; set; }

    /// <summary>Gets or sets the number of pages in the book.</summary>
    public int Pages { get; set; }

    /// <summary>
    /// Initialises a new <see cref="Book"/> instance.
    /// </summary>
    /// <param name="title">Name of the book. Must not be null or whitespace.</param>
    /// <param name="author">Author of the book. Must not be null or whitespace.</param>
    /// <param name="pages">Number of pages. Must be greater than zero.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="title"/> or <paramref name="author"/> is null/whitespace,
    /// or when <paramref name="pages"/> is less than or equal to zero.
    /// </exception>
    public Book(string title, string author, int pages)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Name must not be null or whitespace.", nameof(title));
        }
        if (string.IsNullOrWhiteSpace(author))
        {
            throw new ArgumentException("Author must not be null or whitespace.", nameof(author));
        }
        if (pages <= 0)
        {
            throw new ArgumentException("Pages must be greater than zero.", nameof(pages));
        }

        Name = title.Trim();
        Author = author.Trim();
        Pages = pages;
    }

    /// <inheritdoc/>
    public override string ToString() => $"{Author} — {Name} ({Pages} pp.)";
}
