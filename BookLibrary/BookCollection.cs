using System.Xml.Linq;

namespace BookLibrary;

/// <summary>
/// Manages an in-memory list of <see cref="Book"/> objects with
/// load/save (XML), add, sort and search capabilities.
/// </summary>
public sealed class BookCollection
{
    // ---------------------------------------------------------------
    // Internal state
    // ---------------------------------------------------------------

    private readonly List<Book> _books = new();

    // ---------------------------------------------------------------
    // Public read-only access
    // ---------------------------------------------------------------

    /// <summary>
    /// Returns a snapshot of the current book list.
    /// Modifications to the returned collection do not affect the library.
    /// </summary>
    public IReadOnlyList<Book> Books => new List<Book>(_books).AsReadOnly();

    // ---------------------------------------------------------------
    // 1. Load from XML
    // ---------------------------------------------------------------

    /// <summary>
    /// Replaces the current list with books loaded from an XML file.
    /// </summary>
    /// <param name="filePath">Path to the XML file.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or whitespace.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="InvalidDataException">Thrown when the XML schema is unexpected.</exception>
    /// <remarks>
    /// Expected XML format:
    /// <code>
    /// &lt;Books&gt;
    ///   &lt;Book&gt;
    ///     &lt;Name&gt;The Little Mermaid&lt;/Name&gt;
    ///     &lt;Author&gt;Andersen&lt;/Author&gt;
    ///     &lt;Pages&gt;64&lt;/Pages&gt;
    ///   &lt;/Book&gt;
    /// &lt;/Books&gt;
    /// </code>
    /// </remarks>
    public void LoadFromXml(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path must not be null or whitespace.", nameof(filePath));
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"XML file not found: {filePath}", filePath);

        XDocument doc;
        try
        {
            doc = XDocument.Load(filePath);
        }
        catch (Exception ex)
        {
            throw new InvalidDataException($"Failed to parse XML file '{filePath}'.", ex);
        }

        _books.Clear();
        _books.AddRange(ParseDocument(doc, filePath));
    }

    /// <summary>
    /// Parses an <see cref="XDocument"/> and returns the books it describes.
    /// </summary>
    private static IEnumerable<Book> ParseDocument(XDocument doc, string sourceName)
    {
        var root = doc.Root;
        if (root is null || root.Name.LocalName != "Books")
            throw new InvalidDataException(
                $"Root element must be <Books> in '{sourceName}'.");

        foreach (var element in root.Elements("Book"))
        {
            var title  = ReadRequiredText(element, "Name",  sourceName);
            var author = ReadRequiredText(element, "Author", sourceName);
            var pagesRaw = ReadRequiredText(element, "Pages", sourceName);

            if (!int.TryParse(pagesRaw, out int pages) || pages <= 0)
                throw new InvalidDataException(
                    $"<Pages> must be a positive integer in '{sourceName}'. Got: '{pagesRaw}'.");

            yield return new Book(title, author, pages);
        }
    }

    private static string ReadRequiredText(XElement parent, string childName, string sourceName)
    {
        var child = parent.Element(childName);
        if (child is null || string.IsNullOrWhiteSpace(child.Value))
            throw new InvalidDataException(
                $"Missing or empty <{childName}> element inside <Book> in '{sourceName}'.");
        return child.Value.Trim();
    }

    // ---------------------------------------------------------------
    // 2. Add a book
    // ---------------------------------------------------------------

    /// <summary>
    /// Adds a book to the collection.
    /// </summary>
    /// <param name="book">The book to add. Must not be <c>null</c>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="book"/> is <c>null</c>.</exception>
    public void Add(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        _books.Add(book);
    }

    /// <summary>
    /// Convenience overload: creates and adds a new <see cref="Book"/> from its parts.
    /// </summary>
    /// <param name="title">Name of the book.</param>
    /// <param name="author">Author of the book.</param>
    /// <param name="pages">Number of pages.</param>
    public void Add(string title, string author, int pages) =>
        Add(new Book(title, author, pages));

    // ---------------------------------------------------------------
    // 3. Sort
    // ---------------------------------------------------------------

    /// <summary>
    /// Sorts the collection in-place: primary key = Author (ordinal,
    /// case-insensitive); secondary key = Name (ordinal, case-insensitive).
    /// </summary>
    public void Sort()
    {
        _books.Sort((a, b) =>
        {
            int cmp = string.Compare(a.Author, b.Author,
                                     StringComparison.OrdinalIgnoreCase);
            if (cmp != 0) return cmp;
            return string.Compare(a.Name, b.Name,
                                  StringComparison.OrdinalIgnoreCase);
        });
    }

    /// <summary>
    /// Returns a new sorted list without modifying the collection.
    /// Same ordering as <see cref="Sort"/>.
    /// </summary>
    public IReadOnlyList<Book> GetSorted()
    {
        var copy = _books.ToList();
        copy.Sort((a, b) =>
        {
            int cmp = string.Compare(a.Author, b.Author,
                                     StringComparison.OrdinalIgnoreCase);
            if (cmp != 0) return cmp;
            return string.Compare(a.Name, b.Name,
                                  StringComparison.OrdinalIgnoreCase);
        });
        return copy.AsReadOnly();
    }

    // ---------------------------------------------------------------
    // 4. Search by title substring
    // ---------------------------------------------------------------

    /// <summary>
    /// Returns all books whose title contains <paramref name="titlePart"/>
    /// (case-insensitive, basic substring match — no fuzzy logic).
    /// </summary>
    /// <param name="titlePart">Substring to look for.</param>
    /// <returns>A (possibly empty) list of matching books.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="titlePart"/> is null or whitespace.</exception>
    public IReadOnlyList<Book> SearchByTitle(string titlePart)
    {
        if (string.IsNullOrWhiteSpace(titlePart))
            throw new ArgumentException(
                "Search term must not be null or whitespace.", nameof(titlePart));

        return _books
            .Where(b => b.Name.Contains(titlePart.Trim(),
                                         StringComparison.OrdinalIgnoreCase))
            .ToList()
            .AsReadOnly();
    }

    // ---------------------------------------------------------------
    // 5. Save to XML
    // ---------------------------------------------------------------

    /// <summary>
    /// Saves the current list of books to an XML file.
    /// Overwrites the file if it already exists.
    /// The parent directory is created automatically when absent.
    /// </summary>
    /// <param name="filePath">Destination file path.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or whitespace.</exception>
    public void SaveToXml(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path must not be null or whitespace.", nameof(filePath));

        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var doc = new XDocument(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("Books",
                _books.Select(b => new XElement("Book",
                    new XElement("Name",  b.Name),
                    new XElement("Author", b.Author),
                    new XElement("Pages",  b.Pages)))));

        doc.Save(filePath);
    }

    // ---------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------

    /// <summary>Removes all books from the collection.</summary>
    public void Clear() => _books.Clear();

    /// <summary>Gets the number of books currently in the collection.</summary>
    public int Count => _books.Count;
}
