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
    public void SearchByName_ExactMatch_ReturnsBook()
    {
        var result = _sut.SearchByName(Library, "It");
        Assert.Single(result);
        Assert.Equal("It", result[0].Name);
    }

    [Fact]
    public void SearchByName_SubstringMatch_ReturnsBook()
    {
        var result = _sut.SearchByName(Library, "Little");
        Assert.Single(result);
        Assert.Equal("The Little Mermaid", result[0].Name);
    }

    [Fact]
    public void SearchByName_IsCaseInsensitive()
    {
        var result = _sut.SearchByName(Library, "LITTLE");
        Assert.Single(result);
    }

    [Fact]
    public void SearchByName_MultipleMatches_ReturnsAll()
    {
        var result = _sut.SearchByName(Library, "The");
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void SearchByName_NoMatch_ReturnsEmptyList()
    {
        var result = _sut.SearchByName(Library, "Dracula");
        Assert.Empty(result);
    }

    [Fact]
    public void SearchByName_TrimsSearchTerm()
    {
        var result = _sut.SearchByName(Library, "  Little  ");
        Assert.Single(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SearchByName_BlankTerm_ThrowsArgumentException(string? bad)
    {
        Assert.Throws<ArgumentException>(() => _sut.SearchByName(Library, bad!));
    }

    [Fact]
    public void SearchByName_NullBooks_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.SearchByName(null!, "term"));
    }

    [Fact]
    public void SearchByName_EmptyCollection_ReturnsEmpty()
    {
        var result = _sut.SearchByName(Array.Empty<Book>(), "term");
        Assert.Empty(result);
    }
}
