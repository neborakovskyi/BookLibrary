using BookLibrary.Exceptions;
using BookLibrary.Models;
using Xunit;

namespace BookLibrary.Tests;

public sealed class BookTests
{
    // ── Happy path ────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ValidArguments_SetsProperties()
    {
        var book = new Book("Moby Dick", "Herman Melville", 635);

        Assert.Equal("Moby Dick",       book.Name);
        Assert.Equal("Herman Melville", book.Author);
        Assert.Equal(635,               book.Pages);
    }

    [Fact]
    public void Constructor_TrimsWhitespaceFromStrings()
    {
        var book = new Book("  Moby Dick  ", "  Melville  ", 100);

        Assert.Equal("Moby Dick", book.Name);
        Assert.Equal("Melville",  book.Author);
    }

    // ── BookValidationException ───────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankTitle_ThrowsBookValidationException(string? bad)
    {
        var ex = Assert.Throws<BookValidationException>(() => new Book(bad!, "Author", 10));
        Assert.Equal("Name", ex.FieldName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankAuthor_ThrowsBookValidationException(string? bad)
    {
        var ex = Assert.Throws<BookValidationException>(() => new Book("Name", bad!, 10));
        Assert.Equal("Author", ex.FieldName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_NonPositivePages_ThrowsBookValidationException(int pages)
    {
        var ex = Assert.Throws<BookValidationException>(() => new Book("Name", "Author", pages));
        Assert.Equal("Pages", ex.FieldName);
    }

    // ── BookValidationException is a BookLibraryException ────────────

    [Fact]
    public void BookValidationException_IsBookLibraryException()
    {
        var ex = Assert.Throws<BookValidationException>(() => new Book("", "A", 1));
        Assert.IsAssignableFrom<BookLibraryException>(ex);
    }

    // ── With* helpers ─────────────────────────────────────────────────

    [Fact]
    public void WithName_ReturnsNewInstanceWithUpdatedName()
    {
        var original = new Book("Old", "Author", 100);
        var updated  = original.WithName("New");

        Assert.Equal("New",    updated.Name);
        Assert.Equal("Author", updated.Author);
        Assert.Equal(100,      updated.Pages);
        Assert.NotSame(original, updated);
    }

    [Fact]
    public void WithAuthor_ReturnsNewInstanceWithUpdatedAuthor()
    {
        var original = new Book("Name", "Old", 100);
        var updated  = original.WithAuthor("New");

        Assert.Equal("New", updated.Author);
        Assert.NotSame(original, updated);
    }

    [Fact]
    public void WithPages_ReturnsNewInstanceWithUpdatedPages()
    {
        var original = new Book("Name", "Author", 100);
        var updated  = original.WithPages(200);

        Assert.Equal(200, updated.Pages);
        Assert.NotSame(original, updated);
    }

    [Fact]
    public void WithName_InvalidValue_ThrowsBookValidationException()
    {
        var book = new Book("Name", "Author", 100);
        Assert.Throws<BookValidationException>(() => book.WithName(""));
    }

    // ── ToString ──────────────────────────────────────────────────────

    [Fact]
    public void ToString_ContainsAuthorNameAndPages()
    {
        var str = new Book("It", "King", 1138).ToString();
        Assert.Contains("King", str);
        Assert.Contains("It",   str);
        Assert.Contains("1138", str);
    }
}
