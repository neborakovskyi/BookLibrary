using BookLibrary.Models;
using BookLibrary.Services;
using Xunit;

namespace BookLibrary.Tests;

public sealed class BookSortServiceTests
{
    private readonly BookSortService _sut = new();

    [Fact]
    public void Sort_OrdersByAuthorThenName()
    {
        var input = new[]
        {
            new Book("The Ugly Duckling",  "Andersen", 32),
            new Book("The Little Mermaid", "Andersen", 48),
            new Book("It",                 "King",     1138),
            new Book("Carrie",             "King",     253)
        };

        var result = _sut.Sort(input);

        Assert.Equal("Andersen", result[0].Author);
        Assert.Equal("The Little Mermaid", result[0].Name);   // A before U
        Assert.Equal("Andersen", result[1].Author);
        Assert.Equal("The Ugly Duckling",  result[1].Name);
        Assert.Equal("King",     result[2].Author);
        Assert.Equal("Carrie",   result[2].Name);              // C before I
        Assert.Equal("King",     result[3].Author);
        Assert.Equal("It",       result[3].Name);
    }

    [Fact]
    public void Sort_IsCaseInsensitiveOnAuthor()
    {
        var input = new[]
        {
            new Book("Zebra", "king",     100),
            new Book("Apple", "Andersen", 50)
        };

        var result = _sut.Sort(input);
        Assert.Equal("Andersen", result[0].Author);
    }

    [Fact]
    public void Sort_IsCaseInsensitiveOnName()
    {
        var input = new[]
        {
            new Book("zebra", "King", 100),
            new Book("Apple", "King",  50)
        };

        var result = _sut.Sort(input);
        Assert.Equal("Apple", result[0].Name);
    }

    [Fact]
    public void Sort_DoesNotModifySourceSequence()
    {
        var input = new[]
        {
            new Book("Zebra", "Z-Author", 100),
            new Book("Apple", "A-Author",  50)
        };
        var original = input.ToArray(); // snapshot

        _sut.Sort(input);

        Assert.Equal(original[0].Name, input[0].Name);
        Assert.Equal(original[1].Name, input[1].Name);
    }

    [Fact]
    public void Sort_EmptySequence_ReturnsEmpty()
    {
        var result = _sut.Sort(Array.Empty<Book>());
        Assert.Empty(result);
    }

    [Fact]
    public void Sort_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _sut.Sort(null!));
    }

    [Fact]
    public void Sort_SingleBook_ReturnsThatBook()
    {
        var book   = new Book("It", "King", 1138);
        var result = _sut.Sort(new[] { book });
        Assert.Single(result);
        Assert.Same(book, result[0]);
    }
}
