using BookLibrary.Models;
using BookLibrary.Services;
using Xunit;

namespace BookLibrary.Tests;

public sealed class BookSearchServiceTests
{
    private readonly BookSearchService _sut = new();

    private static readonly Book[] Library =
    {
        new("The Little Mermaid", "Andersen", 48),
        new("The Ugly Duckling",  "Andersen", 32),
        new("It",                 "King",     1138)
    };

    [Fact]
    public void SearchByTitle_ExactMatch_ReturnsBook()
    {
        var result = _sut.SearchByTitle(Library, "It");
        Assert.Single(result);
        Assert.Equal("It", result[0].Name);
    }

    [Fact]
    public void SearchByTitle_SubstringMatch_ReturnsBook()
    {
        var result = _sut.SearchByTitle(Library, "Little");
        Assert.Single(result);
        Assert.Equal("The Little Mermaid", result[0].Name);
    }

    [Fact]
    public void SearchByTitle_IsCaseInsensitive()
    {
        var result = _sut.SearchByTitle(Library, "LITTLE");
        Assert.Single(result);
    }

    [Fact]
    public void SearchByTitle_MultipleMatches_ReturnsAll()
    {
        var result = _sut.SearchByTitle(Library, "The");
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void SearchByTitle_NoMatch_ReturnsEmptyList()
    {
        var result = _sut.SearchByTitle(Library, "Dracula");
        Assert.Empty(result);
    }

    [Fact]
    public void SearchByTitle_TrimsSearchTerm()
    {
        var result = _sut.SearchByTitle(Library, "  Little  ");
        Assert.Single(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SearchByTitle_BlankTerm_ThrowsArgumentException(string? bad)
    {
        Assert.Throws<ArgumentException>(() => _sut.SearchByTitle(Library, bad!));
    }

    [Fact]
    public void SearchByTitle_NullBooks_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.SearchByTitle(null!, "term"));
    }

    [Fact]
    public void SearchByTitle_EmptyCollection_ReturnsEmpty()
    {
        var result = _sut.SearchByTitle(Array.Empty<Book>(), "term");
        Assert.Empty(result);
    }
}
