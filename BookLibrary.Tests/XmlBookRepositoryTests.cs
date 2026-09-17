using BookLibrary.Exceptions;
using BookLibrary.Models;
using BookLibrary.Services;
using Xunit;

namespace BookLibrary.Tests;

/// <summary>
/// Integration-level tests for <see cref="XmlBookRepository"/>:
/// uses real temp files to verify actual async file I/O.
/// </summary>
public sealed class XmlBookRepositoryTests : IDisposable
{
    private readonly string _dir  = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
    private string TempFile(string name = "books.xml") => Path.Combine(_dir, name);

    public XmlBookRepositoryTests() => Directory.CreateDirectory(_dir);
    public void Dispose()
    {
        if (Directory.Exists(_dir))
            Directory.Delete(_dir, recursive: true);
    }

    private static void Write(string path, string content) => File.WriteAllText(path, content);

    private const string ValidXml = """
        <?xml version="1.0" encoding="utf-8"?>
        <Books>
          <Book>
            <Name>The Little Mermaid</Name>
            <Author>Andersen</Author>
            <Pages>48</Pages>
          </Book>
          <Book>
            <Name>It</Name>
            <Author>King</Author>
            <Pages>1138</Pages>
          </Book>
        </Books>
        """;

    // ── Constructor ───────────────────────────────────────────────────

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_BlankPath_ThrowsArgumentException(string? bad)
    {
        Assert.Throws<ArgumentException>(() => new XmlBookRepository(bad!));
    }

    // ── LoadAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task LoadAsync_ValidFile_ReturnsBooksAsync()
    {
        var path = TempFile();
        Write(path, ValidXml);

        var repo  = new XmlBookRepository(path);
        var books = await repo.LoadAsync();

        Assert.Equal(2, books.Count);
    }

    [Fact]
    public async Task LoadAsync_ParsesAllFieldsCorrectly()
    {
        var path = TempFile();
        Write(path, ValidXml);
        var repo  = new XmlBookRepository(path);
        var books = await repo.LoadAsync();

        var andersen = books.First(b => b.Author == "Andersen");
        Assert.Equal("The Little Mermaid", andersen.Name);
        Assert.Equal(48,                   andersen.Pages);
    }

    [Fact]
    public async Task LoadAsync_EmptyBookList_ReturnsEmptyCollection()
    {
        var path = TempFile();
        Write(path, "<Books></Books>");
        var repo  = new XmlBookRepository(path);
        var books = await repo.LoadAsync();
        Assert.Empty(books);
    }

    [Fact]
    public async Task LoadAsync_FileNotFound_ThrowsBookPersistenceException()
    {
        var repo = new XmlBookRepository(Path.Combine(_dir, "missing.xml"));
        var ex   = await Assert.ThrowsAsync<BookPersistenceException>(
                       () => repo.LoadAsync());
        Assert.Contains("missing.xml", ex.FilePath);
    }

    [Fact]
    public async Task LoadAsync_WrongRootElement_ThrowsBookPersistenceException()
    {
        var path = TempFile();
        Write(path, "<Library><Book><Name>T</Name><Author>A</Author><Pages>1</Pages></Book></Library>");
        var repo = new XmlBookRepository(path);
        await Assert.ThrowsAsync<BookPersistenceException>(() => repo.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_MissingTitleElement_ThrowsBookPersistenceException()
    {
        var path = TempFile();
        Write(path, "<Books><Book><Author>King</Author><Pages>100</Pages></Book></Books>");
        var repo = new XmlBookRepository(path);
        await Assert.ThrowsAsync<BookPersistenceException>(() => repo.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_NonIntegerPages_ThrowsBookPersistenceException()
    {
        var path = TempFile();
        Write(path, "<Books><Book><Name>T</Name><Author>A</Author><Pages>abc</Pages></Book></Books>");
        var repo = new XmlBookRepository(path);
        await Assert.ThrowsAsync<BookPersistenceException>(() => repo.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_NegativePages_ThrowsBookPersistenceException()
    {
        var path = TempFile();
        Write(path, "<Books><Book><Name>T</Name><Author>A</Author><Pages>-1</Pages></Book></Books>");
        var repo = new XmlBookRepository(path);
        await Assert.ThrowsAsync<BookPersistenceException>(() => repo.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_MalformedXml_ThrowsBookPersistenceException()
    {
        var path = TempFile();
        Write(path, "<<not xml>>");
        var repo = new XmlBookRepository(path);
        await Assert.ThrowsAsync<BookPersistenceException>(() => repo.LoadAsync());
    }

    [Fact]
    public async Task LoadAsync_CancelledToken_ThrowsOperationCanceledException()
    {
        var path = TempFile();
        Write(path, ValidXml);
        var repo = new XmlBookRepository(path);
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => repo.LoadAsync(cts.Token));
    }

    // ── SaveAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_CreatesFile()
    {
        var path  = TempFile("save.xml");
        var repo  = new XmlBookRepository(path);
        await repo.SaveAsync(new[] { new Book("It", "King", 1138) });
        Assert.True(File.Exists(path));
    }

    [Fact]
    public async Task SaveAsync_ThenLoadAsync_RoundTrips()
    {
        var books = new[]
        {
            new Book("The Little Mermaid", "Andersen", 48),
            new Book("It",                 "King",     1138)
        };
        var path = TempFile("rt.xml");
        var repo = new XmlBookRepository(path);

        await repo.SaveAsync(books);
        var loaded = await repo.LoadAsync();

        Assert.Equal(2, loaded.Count);

        var a = loaded.Single(b => b.Author == "Andersen");
        Assert.Equal("The Little Mermaid", a.Name);
        Assert.Equal(48,                   a.Pages);
    }

    [Fact]
    public async Task SaveAsync_CreatesParentDirectory()
    {
        var nested = Path.Combine(_dir, "deep", "nested", "books.xml");
        var repo   = new XmlBookRepository(nested);
        await repo.SaveAsync(new[] { new Book("Carrie", "King", 253) });
        Assert.True(File.Exists(nested));
    }

    [Fact]
    public async Task SaveAsync_EmptyList_WritesValidXmlThatLoadsBack()
    {
        var path = TempFile("empty.xml");
        var repo = new XmlBookRepository(path);
        await repo.SaveAsync(Array.Empty<Book>());
        var loaded = await repo.LoadAsync();
        Assert.Empty(loaded);
    }

    [Fact]
    public async Task SaveAsync_NullBooks_ThrowsArgumentNullException()
    {
        var repo = new XmlBookRepository(TempFile());
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repo.SaveAsync(null!));
    }

    // ── BookPersistenceException is a BookLibraryException ────────────

    [Fact]
    public async Task BookPersistenceException_IsBookLibraryException()
    {
        var repo = new XmlBookRepository(Path.Combine(_dir, "missing.xml"));
        var ex   = await Assert.ThrowsAsync<BookPersistenceException>(
                       () => repo.LoadAsync());
        Assert.IsAssignableFrom<BookLibraryException>(ex);
    }
}
