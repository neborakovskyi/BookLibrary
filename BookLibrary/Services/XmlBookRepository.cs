using System.Xml;
using System.Xml.Linq;
using BookLibrary.Exceptions;
using BookLibrary.Interfaces;
using BookLibrary.Models;

namespace BookLibrary.Services;

/// <summary>
/// Loads and saves books as UTF-8 XML using fully async file I/O.
/// Implements <see cref="IBookRepository"/> (OCP: swap for JSON/DB
/// without touching higher-level code).
/// </summary>
/// <remarks>
/// Expected XML schema:
/// <code>
/// &lt;Books&gt;
///   &lt;Book&gt;
///     &lt;Name&gt;…&lt;/Name&gt;
///     &lt;Author&gt;…&lt;/Author&gt;
///     &lt;Pages&gt;…&lt;/Pages&gt;
///   &lt;/Book&gt;
/// &lt;/Books&gt;
/// </code>
/// </remarks>
public sealed class XmlBookRepository : IBookRepository
{
    private readonly string _filePath;

    /// <param name="filePath">
    /// Path to the XML file.  The file is created on first save if absent.
    /// </param>
    public XmlBookRepository(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path must not be null or whitespace.", nameof(filePath));
        }

        _filePath = filePath;
    }

    // ──────────────────────────────────────────────────────────────────
    // Load
    // ──────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Book>> LoadAsync(CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            throw new BookPersistenceException(_filePath, $"XML file not found: '{_filePath}'.");
        }

        string xml;
        try
        {
            // Truly async read — no blocking FileStream.Read
            xml = await File.ReadAllTextAsync(_filePath, cancellationToken)
                            .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw; // propagate cancellation unchanged
        }
        catch (Exception ex)
        {
            throw new BookPersistenceException(_filePath,
                $"Failed to read file '{_filePath}'.", ex);
        }

        return ParseXml(xml);
    }

    // ──────────────────────────────────────────────────────────────────
    // Save
    // ──────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task SaveAsync(IEnumerable<Book> books,
                                CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(books);

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var doc = BuildDocument(books);

        // Write to a temp file first, then atomic-replace (safe against corruption)
        var tmpPath = _filePath + ".tmp";
        try
        {
            // Async write using XDocument.SaveAsync; avoids unsupported XmlWriter.CreateAsync API.
            await using var stream = new FileStream(
                tmpPath,
                FileMode.Create, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true);

            await doc.SaveAsync(stream, SaveOptions.None, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            TryDelete(tmpPath);
            throw;
        }
        catch (Exception ex)
        {
            TryDelete(tmpPath);
            throw new BookPersistenceException(_filePath,
                $"Failed to write file '{_filePath}'.", ex);
        }

        // Atomic replace
        File.Move(tmpPath, _filePath, overwrite: true);
    }

    // ──────────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────────

    private IReadOnlyList<Book> ParseXml(string xml)
    {
        XDocument doc;
        try
        {
            doc = XDocument.Parse(xml);
        }
        catch (XmlException ex)
        {
            throw new BookPersistenceException(_filePath,
                $"Malformed XML in '{_filePath}'.", ex);
        }

        if (doc.Root?.Name.LocalName != "Books")
            throw new BookPersistenceException(_filePath,
                $"Root element must be <Books> in '{_filePath}'.");

        var books = new List<Book>();
        foreach (var el in doc.Root.Elements("Book"))
        {
            var name     = RequiredText(el, "Name");
            var author   = RequiredText(el, "Author");
            var pagesRaw = RequiredText(el, "Pages");

            if (!int.TryParse(pagesRaw, out int pages) || pages <= 0)
                throw new BookPersistenceException(_filePath,
                    $"<Pages> must be a positive integer; got '{pagesRaw}' in '{_filePath}'.");

            // BookValidationException can also propagate here — intentional
            books.Add(new Book(name, author, pages));
        }

        return books.AsReadOnly();
    }

    private string RequiredText(XElement parent, string childName)
    {
        var child = parent.Element(childName);
        if (child is null || string.IsNullOrWhiteSpace(child.Value))
            throw new BookPersistenceException(_filePath,
                $"Missing or empty <{childName}> inside <Book> in '{_filePath}'.");
        return child.Value.Trim();
    }

    private static XDocument BuildDocument(IEnumerable<Book> books) =>
        new(
            new XDeclaration("1.0", "utf-8", null),
            new XElement("Books",
                books.Select(b => new XElement("Book",
                    new XElement("Name",  b.Name),
                    new XElement("Author", b.Author),
                    new XElement("Pages",  b.Pages)))));

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort cleanup */ }
    }
}
