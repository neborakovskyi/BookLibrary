using BookLibrary.Exceptions;
using BookLibrary.Interfaces;
using BookLibrary.Models;
using BookLibrary.Services;
using NSubstitute;
using Xunit;

namespace BookLibrary.Tests;

/// <summary>
/// Tests for <see cref="BookCollectionService"/>.
/// All collaborators are mocked (NSubstitute) — verifies the facade's
/// orchestration logic in complete isolation (DIP in action).
/// </summary>
public sealed class BookCollectionServiceTests
{
    // ── Fixtures ──────────────────────────────────────────────────────

    private readonly IBookRepository    _repo     = Substitute.For<IBookRepository>();
    private readonly IBookSortService   _sorter   = Substitute.For<IBookSortService>();
    private readonly IBookSearchService _searcher = Substitute.For<IBookSearchService>();

    private BookCollectionService Build() =>
        new(_repo, _sorter, _searcher);

    private static readonly Book BookA = new("The Little Mermaid", "Andersen", 48);
    private static readonly Book BookB = new("It", "King", 1138);

    // ── Constructor guards ────────────────────────────────────────────

    [Fact]
    public void Constructor_NullRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new BookCollectionService(null!, _sorter, _searcher));
    }

    [Fact]
    public void Constructor_NullSorter_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new BookCollectionService(_repo, null!, _searcher));
    }

    [Fact]
    public void Constructor_NullSearcher_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new BookCollectionService(_repo, _sorter, null!));
    }

    // ── LoadAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task LoadAsync_CallsRepositoryAndPopulatesBooks()
    {
        _repo.LoadAsync(Arg.Any<CancellationToken>())
             .Returns(new[] { BookA, BookB });

        var svc = Build();
        await svc.LoadAsync();

        Assert.Equal(2, svc.Books.Count);
        await _repo.Received(1).LoadAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoadAsync_ReplacesExistingBooks()
    {
        var svc = Build();
        svc.Add(new Book("Old", "Old", 1));

        _repo.LoadAsync(Arg.Any<CancellationToken>())
             .Returns(new[] { BookA });

        await svc.LoadAsync();

        Assert.Single(svc.Books);
        Assert.Equal("The Little Mermaid", svc.Books[0].Name);
    }

    [Fact]
    public async Task LoadAsync_RepositoryThrows_PropagatesException()
    {
        _repo.LoadAsync(Arg.Any<CancellationToken>())
             .Returns<IReadOnlyList<Book>>(_ =>
                 throw new BookPersistenceException("/x.xml", "boom"));

        var svc = Build();
        await Assert.ThrowsAsync<BookPersistenceException>(() => svc.LoadAsync());
    }

    // ── SaveAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_CallsRepositoryWithCurrentBooks()
    {
        var svc = Build();
        svc.Add(BookA);
        svc.Add(BookB);

        await svc.SaveAsync();

        await _repo.Received(1)
                   .SaveAsync(
                       Arg.Is<IEnumerable<Book>>(b => b.Count() == 2),
                       Arg.Any<CancellationToken>());
    }

    // ── Add ───────────────────────────────────────────────────────────

    [Fact]
    public void Add_Book_AppearsInBooks()
    {
        var svc = Build();
        svc.Add(BookA);
        Assert.Single(svc.Books);
        Assert.Same(BookA, svc.Books[0]);
    }

    [Fact]
    public void Add_StringOverload_CreatesAndAddsBook()
    {
        var svc = Build();
        svc.Add("Carrie", "King", 253);
        Assert.Single(svc.Books);
        Assert.Equal("Carrie", svc.Books[0].Name);
    }

    [Fact]
    public void Add_NullBook_ThrowsArgumentNullException()
    {
        var svc = Build();
        Assert.Throws<ArgumentNullException>(() => svc.Add(null!));
    }

    [Fact]
    public void Add_InvalidBook_ThrowsBookValidationException()
    {
        var svc = Build();
        Assert.Throws<BookValidationException>(() => svc.Add(new Book("", "Author", 10)));
    }

    // ── Clear ─────────────────────────────────────────────────────────

    [Fact]
    public void Clear_RemovesAllBooks()
    {
        var svc = Build();
        svc.Add(BookA);
        svc.Clear();
        Assert.Empty(svc.Books);
    }

    // ── Sort / GetSorted ──────────────────────────────────────────────

    [Fact]
    public void Sort_DelegatesToSorterAndUpdatesInPlace()
    {
        var svc = Build();
        svc.Add(BookB);   // King
        svc.Add(BookA);   // Andersen

        var expected = new[] { BookA, BookB };
        _sorter.Sort(Arg.Any<IEnumerable<Book>>()).Returns(expected);

        svc.Sort();

        _sorter.Received(1).Sort(Arg.Any<IEnumerable<Book>>());
        Assert.Equal("Andersen", svc.Books[0].Author);
    }

    [Fact]
    public void GetSorted_DelegatesToSorterWithoutChangingInternalList()
    {
        var svc = Build();
        svc.Add(BookB); // King — added first
        svc.Add(BookA); // Andersen

        var sortedResult = new[] { BookA, BookB };
        _sorter.Sort(Arg.Any<IEnumerable<Book>>()).Returns(sortedResult);

        var result = svc.GetSorted();

        // Result from sorter is returned
        Assert.Equal(2, result.Count);
        Assert.Equal("Andersen", result[0].Author);

        // Internal list order is unchanged (King still first)
        Assert.Equal("King", svc.Books[0].Author);
    }

    // ── SearchByName ─────────────────────────────────────────────────

    [Fact]
    public void SearchByName_DelegatesToSearcher()
    {
        var svc = Build();
        svc.Add(BookA);

        _searcher.SearchByName(Arg.Any<IEnumerable<Book>>(), "mermaid")
                 .Returns(new[] { BookA });

        var result = svc.SearchByName("mermaid");

        _searcher.Received(1)
                 .SearchByName(Arg.Any<IEnumerable<Book>>(), "mermaid");
        Assert.Single(result);
    }

    [Fact]
    public void SearchByName_SearcherThrows_PropagatesException()
    {
        var svc = Build();
        _searcher.SearchByName(Arg.Any<IEnumerable<Book>>(), Arg.Any<string>())
                 .Returns<IReadOnlyList<Book>>(_ =>
                     throw new ArgumentException("blank term"));

        Assert.Throws<ArgumentException>(() => svc.SearchByName("   "));
    }

    // ── Books snapshot ────────────────────────────────────────────────

    [Fact]
    public void Books_ReturnsSnapshot_NotLiveReference()
    {
        var svc      = Build();
        svc.Add(BookA);
        var snapshot = svc.Books;

        svc.Add(BookB);

        Assert.Single(snapshot); // snapshot captured before second Add
    }
}
