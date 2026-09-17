using BookLibrary.Interfaces;
using BookLibrary.Models;

namespace BookLibrary.Services;

/// <summary>
/// Facade coordinating repository, sort and search services.
/// All collaborators are injected via constructor (DIP / LSP).
/// Consumers reference only <see cref="IBookCollectionService"/>
/// — never this concrete class.
/// </summary>
public sealed class BookCollectionService : IBookCollectionService
{
    private readonly IBookRepository    _repository;
    private readonly IBookSortService   _sorter;
    private readonly IBookSearchService _searcher;
    private readonly List<Book>         _books = new();

    /// <summary>
    /// Creates the service with its three collaborators.
    /// </summary>
    /// <param name="repository">Persistence strategy (XML, DB, etc.).</param>
    /// <param name="sorter">Sort strategy.</param>
    /// <param name="searcher">Search strategy.</param>
    public BookCollectionService(
        IBookRepository    repository,
        IBookSortService   sorter,
        IBookSearchService searcher)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _sorter     = sorter     ?? throw new ArgumentNullException(nameof(sorter));
        _searcher   = searcher   ?? throw new ArgumentNullException(nameof(searcher));
    }

    // ── IBookCollectionService ────────────────────────────────────────

    /// <inheritdoc/>
    public IReadOnlyList<Book> Books => new List<Book>(_books).AsReadOnly();

    /// <inheritdoc/>
    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        var loaded = await _repository.LoadAsync(cancellationToken).ConfigureAwait(false);
        _books.Clear();
        _books.AddRange(loaded);
    }

    /// <inheritdoc/>
    public async Task SaveAsync(CancellationToken cancellationToken = default) =>
        await _repository.SaveAsync(_books, cancellationToken).ConfigureAwait(false);

    /// <inheritdoc/>
    public void Add(Book book)
    {
        ArgumentNullException.ThrowIfNull(book);
        _books.Add(book);
    }

    /// <inheritdoc/>
    public void Add(string title, string author, int pages) =>
        Add(new Book(title, author, pages));

    /// <inheritdoc/>
    public void Clear() => _books.Clear();

    /// <inheritdoc/>
    public IReadOnlyList<Book> GetSorted() => _sorter.Sort(_books);

    /// <inheritdoc/>
    public void Sort()
    {
        var sorted = _sorter.Sort(_books);
        _books.Clear();
        _books.AddRange(sorted);
    }

    /// <inheritdoc/>
    public IReadOnlyList<Book> SearchByTitle(string titlePart) =>
        _searcher.SearchByTitle(_books, titlePart);
}
