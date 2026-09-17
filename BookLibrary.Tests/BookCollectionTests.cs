using BookLibrary;
using Xunit;

namespace BookLibrary.Tests;

public sealed class BookCollectionTests : IDisposable
{
    // ------------------------------------------------------------------
    // Helpers / fixtures
    // ------------------------------------------------------------------

    private readonly BookCollection _collection = new();
    private readonly string _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

    /// <summary>Creates a temporary XML file with the given content.</summary>
    private string WriteXml(string xml)
    {
        Directory.CreateDirectory(_tempDir);
        var path = Path.Combine(_tempDir, $"{Guid.NewGuid()}.xml");
        File.WriteAllText(path, xml);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // ==================================================================
    // 1. Add
    // ==================================================================

    [Fact]
    public void Add_Book_IncreasesCount()
    {
        _collection.Add("It", "King", 1138);
        Assert.Equal(1, _collection.Count);
    }

    [Fact]
    public void Add_NullBook_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _collection.Add(null!));
    }

    [Fact]
    public void Add_MultipleBooks_AllPresent()
    {
        _collection.Add("It",      "King",     1138);
        _collection.Add("Carrie",  "King",      253);
        _collection.Add("Dracula", "Stoker",    418);

        Assert.Equal(3, _collection.Count);
    }

    // ==================================================================
    // 2. Sort — primary by Author, secondary by Name
    // ==================================================================

    [Fact]
    public void Sort_OrdersByAuthorThenTitle()
    {
        _collection.Add("The Ugly Duckling",  "Andersen", 32);
        _collection.Add("The Little Mermaid", "Andersen", 48);
        _collection.Add("It",                 "King",     1138);
        _collection.Add("Carrie",             "King",     253);

        _collection.Sort();
        var books = _collection.Books;

        // Authors in order
        Assert.Equal("Andersen", books[0].Author);
        Assert.Equal("Andersen", books[1].Author);
        Assert.Equal("King",     books[2].Author);
        Assert.Equal("King",     books[3].Author);

        // Andersen: alphabetical by title
        Assert.Equal("The Little Mermaid", books[0].Name);
        Assert.Equal("The Ugly Duckling",  books[1].Name);

        // King: alphabetical by title
        Assert.Equal("Carrie", books[2].Name);
        Assert.Equal("It",     books[3].Name);
    }

    [Fact]
    public void Sort_IsCaseInsensitive()
    {
        _collection.Add("Zebra", "andersen", 100);  // lowercase author
        _collection.Add("Apple", "Andersen", 50);

        _collection.Sort();
        var books = _collection.Books;

        Assert.Equal("Apple", books[0].Name);
        Assert.Equal("Zebra", books[1].Name);
    }

    [Fact]
    public void Sort_EmptyCollection_DoesNotThrow()
    {
        var ex = Record.Exception(() => _collection.Sort());
        Assert.Null(ex);
    }

    [Fact]
    public void GetSorted_DoesNotModifyOriginalOrder()
    {
        _collection.Add("Zebra Book", "Z-Author", 100);
        _collection.Add("Apple Book", "A-Author",  50);

        var sorted = _collection.GetSorted();

        // Sorted view: A-Author first
        Assert.Equal("A-Author", sorted[0].Author);

        // Original order unchanged: Z-Author was added first
        Assert.Equal("Z-Author", _collection.Books[0].Author);
    }

    // ==================================================================
    // 3. Search by title substring
    // ==================================================================

    [Fact]
    public void SearchByTitle_MatchesSubstring()
    {
        _collection.Add("The Little Mermaid", "Andersen", 48);
        _collection.Add("The Ugly Duckling",  "Andersen", 32);
        _collection.Add("It",                 "King",     1138);

        var results = _collection.SearchByTitle("Little");

        Assert.Single(results);
        Assert.Equal("The Little Mermaid", results[0].Name);
    }

    [Fact]
    public void SearchByTitle_IsCaseInsensitive()
    {
        _collection.Add("The Little Mermaid", "Andersen", 48);

        var results = _collection.SearchByTitle("LITTLE");
        Assert.Single(results);
    }

    [Fact]
    public void SearchByTitle_NoMatch_ReturnsEmptyList()
    {
        _collection.Add("It", "King", 1138);
        var results = _collection.SearchByTitle("Andersen");
        Assert.Empty(results);
    }

    [Fact]
    public void SearchByTitle_MultipleMatches_ReturnsAll()
    {
        _collection.Add("The Little Mermaid", "Andersen", 48);
        _collection.Add("The Ugly Duckling",  "Andersen", 32);
        _collection.Add("It",                 "King",     1138);

        var results = _collection.SearchByTitle("The");
        Assert.Equal(2, results.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SearchByTitle_NullOrWhitespaceTerm_Throws(string? bad)
    {
        Assert.Throws<ArgumentException>(() => _collection.SearchByTitle(bad!));
    }

    // ==================================================================
    // 4. LoadFromXml
    // ==================================================================

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

    [Fact]
    public void LoadFromXml_ValidFile_LoadsBooks()
    {
        var path = WriteXml(ValidXml);
        _collection.LoadFromXml(path);

        Assert.Equal(2, _collection.Count);
    }

    [Fact]
    public void LoadFromXml_ValidFile_ParsesFieldsCorrectly()
    {
        var path = WriteXml(ValidXml);
        _collection.LoadFromXml(path);

        var book = _collection.Books.First(b => b.Author == "Andersen");
        Assert.Equal("The Little Mermaid", book.Name);
        Assert.Equal(48, book.Pages);
    }

    [Fact]
    public void LoadFromXml_ReplacesExistingBooks()
    {
        _collection.Add("Old Book", "Old Author", 10);
        var path = WriteXml(ValidXml);
        _collection.LoadFromXml(path);

        Assert.Equal(2, _collection.Count);
        Assert.DoesNotContain(_collection.Books, b => b.Name == "Old Book");
    }

    [Fact]
    public void LoadFromXml_EmptyBookList_LoadsZeroBooks()
    {
        var path = WriteXml("<Books></Books>");
        _collection.LoadFromXml(path);
        Assert.Equal(0, _collection.Count);
    }

    [Fact]
    public void LoadFromXml_FileNotFound_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(
            () => _collection.LoadFromXml("/nonexistent/path/books.xml"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void LoadFromXml_NullOrWhitespacePath_ThrowsArgumentException(string? bad)
    {
        Assert.Throws<ArgumentException>(() => _collection.LoadFromXml(bad!));
    }

    [Fact]
    public void LoadFromXml_WrongRootElement_ThrowsInvalidDataException()
    {
        var path = WriteXml("<Library><Book><Name>T</Name><Author>A</Author><Pages>1</Pages></Book></Library>");
        Assert.Throws<InvalidDataException>(() => _collection.LoadFromXml(path));
    }

    [Fact]
    public void LoadFromXml_MissingTitleElement_ThrowsInvalidDataException()
    {
        var path = WriteXml("""
            <Books>
              <Book>
                <Author>King</Author>
                <Pages>100</Pages>
              </Book>
            </Books>
            """);
        Assert.Throws<InvalidDataException>(() => _collection.LoadFromXml(path));
    }

    [Fact]
    public void LoadFromXml_InvalidPages_ThrowsInvalidDataException()
    {
        var path = WriteXml("""
            <Books>
              <Book>
                <Name>It</Name>
                <Author>King</Author>
                <Pages>not-a-number</Pages>
              </Book>
            </Books>
            """);
        Assert.Throws<InvalidDataException>(() => _collection.LoadFromXml(path));
    }

    [Fact]
    public void LoadFromXml_NegativePages_ThrowsInvalidDataException()
    {
        var path = WriteXml("""
            <Books>
              <Book>
                <Name>It</Name>
                <Author>King</Author>
                <Pages>-5</Pages>
              </Book>
            </Books>
            """);
        Assert.Throws<InvalidDataException>(() => _collection.LoadFromXml(path));
    }

    // ==================================================================
    // 5. SaveToXml
    // ==================================================================

    [Fact]
    public void SaveToXml_CreatesFile()
    {
        _collection.Add("It", "King", 1138);
        var path = Path.Combine(_tempDir, "out.xml");

        _collection.SaveToXml(path);

        Assert.True(File.Exists(path));
    }

    [Fact]
    public void SaveToXml_ThenLoadFromXml_RoundTrips()
    {
        _collection.Add("The Little Mermaid", "Andersen", 48);
        _collection.Add("It",                 "King",     1138);

        var path = Path.Combine(_tempDir, "roundtrip.xml");
        _collection.SaveToXml(path);

        var loaded = new BookCollection();
        loaded.LoadFromXml(path);

        Assert.Equal(2, loaded.Count);

        var andersen = loaded.Books.First(b => b.Author == "Andersen");
        Assert.Equal("The Little Mermaid", andersen.Name);
        Assert.Equal(48,                   andersen.Pages);

        var king = loaded.Books.First(b => b.Author == "King");
        Assert.Equal("It",  king.Name);
        Assert.Equal(1138,  king.Pages);
    }

    [Fact]
    public void SaveToXml_CreatesParentDirectory()
    {
        var nested = Path.Combine(_tempDir, "nested", "deep", "books.xml");
        _collection.Add("Carrie", "King", 253);

        _collection.SaveToXml(nested);

        Assert.True(File.Exists(nested));
    }

    [Fact]
    public void SaveToXml_EmptyCollection_WritesValidXml()
    {
        var path = Path.Combine(_tempDir, "empty.xml");
        _collection.SaveToXml(path);

        var loaded = new BookCollection();
        loaded.LoadFromXml(path);  // should not throw
        Assert.Equal(0, loaded.Count);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SaveToXml_NullOrWhitespacePath_ThrowsArgumentException(string? bad)
    {
        Assert.Throws<ArgumentException>(() => _collection.SaveToXml(bad!));
    }

    // ==================================================================
    // 6. Clear / Count helpers
    // ==================================================================

    [Fact]
    public void Clear_RemovesAllBooks()
    {
        _collection.Add("It", "King", 1138);
        _collection.Clear();
        Assert.Equal(0, _collection.Count);
    }

    [Fact]
    public void Books_ReturnsSnapshot_NotLiveReference()
    {
        _collection.Add("It", "King", 1138);
        var snapshot = _collection.Books;
        _collection.Add("Carrie", "King", 253);

        // snapshot must still show only the book that existed when it was taken
        Assert.Single(snapshot);
    }
}
