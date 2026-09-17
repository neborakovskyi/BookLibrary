using System.Diagnostics;
using BookLibrary.Services;
using Xunit;
using Xunit.Abstractions;

namespace BookLibrary.PerformanceTests;

/// <summary>
/// Measures end-to-end performance against a 1 000 000-record XML file.
///
/// Run with:
///   dotnet test --filter "FullyQualifiedName~PerformanceTests" -c Release
///
/// Time budgets are intentionally generous so tests are green on any
/// developer machine or CI agent.  The output line shows the real elapsed
/// time, making regressions visible in the test log even when the budget
/// is not exceeded.
/// </summary>
[Trait("Category", "Performance")]
public sealed class PerformanceTests : IAsyncLifetime
{
    // ── Tunables ──────────────────────────────────────────────────────

    private const int    RecordCount   = 1_000_000;

    // Generous ceilings — adjust down once you know your baseline.
    private static readonly TimeSpan GenerateBudget = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan LoadBudget     = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan SaveBudget     = TimeSpan.FromMinutes(3);
    private static readonly TimeSpan SortBudget     = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan SearchBudget   = TimeSpan.FromSeconds(5);

    // ── Infrastructure ────────────────────────────────────────────────

    private readonly ITestOutputHelper _out;
    private readonly string _dir  = Path.Combine(Path.GetTempPath(), $"BookLibPerf_{Guid.NewGuid():N}");
    private string DataFile  => Path.Combine(_dir, "large-dataset.xml");
    private string SaveFile  => Path.Combine(_dir, "large-dataset-saved.xml");

    public PerformanceTests(ITestOutputHelper output) => _out = output;

    // Generate the dataset once before any test in this class runs.
    public async Task InitializeAsync()
    {
        Directory.CreateDirectory(_dir);
        _out.WriteLine($"[Setup] Generating {RecordCount:N0} records → {DataFile}");

        var sw = Stopwatch.StartNew();
        await LargeDatasetGenerator.GenerateAsync(DataFile, RecordCount);
        sw.Stop();

        var size = new FileInfo(DataFile).Length;
        _out.WriteLine($"[Setup] Done in {sw.Elapsed:g}  |  file size: {size / 1_048_576.0:F1} MB");

        Assert.True(sw.Elapsed < GenerateBudget,
            $"Generation took {sw.Elapsed:g}, exceeded budget {GenerateBudget:g}");
    }

    public Task DisposeAsync()
    {
        try { Directory.Delete(_dir, recursive: true); } catch { /* best-effort */ }
        return Task.CompletedTask;
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private XmlBookRepository Repo(string path) => new(path);

    private TimeSpan Measure(string label, Action action)
    {
        var sw = Stopwatch.StartNew();
        action();
        sw.Stop();
        _out.WriteLine($"[{label}] {sw.Elapsed:g}");
        return sw.Elapsed;
    }

    private async Task<TimeSpan> MeasureAsync(string label, Func<Task> action)
    {
        var sw = Stopwatch.StartNew();
        await action().ConfigureAwait(false);
        sw.Stop();
        _out.WriteLine($"[{label}] {sw.Elapsed:g}");
        return sw.Elapsed;
    }

    // ── Tests ─────────────────────────────────────────────────────────

    /// <summary>Loads 1 000 000 books from XML and checks timing + count.</summary>
    [Fact]
    public async Task Load_OneMillion_Books_WithinBudget()
    {
        var repo = Repo(DataFile);

        var elapsed = await MeasureAsync("Load 1M", async () =>
        {
            var books = await repo.LoadAsync();
            Assert.Equal(RecordCount, books.Count);
        });

        Assert.True(elapsed < LoadBudget,
            $"Load took {elapsed:g}, exceeded budget {LoadBudget:g}");
    }

    /// <summary>Saves 1 000 000 books to XML and checks timing.</summary>
    [Fact]
    public async Task Save_OneMillion_Books_WithinBudget()
    {
        // Load first so we have real data.
        var books = await Repo(DataFile).LoadAsync();
        Assert.Equal(RecordCount, books.Count);

        var repo    = Repo(SaveFile);
        var elapsed = await MeasureAsync("Save 1M", () => repo.SaveAsync(books));

        Assert.True(File.Exists(SaveFile), "Output file must exist after save.");
        Assert.True(elapsed < SaveBudget,
            $"Save took {elapsed:g}, exceeded budget {SaveBudget:g}");
    }

    /// <summary>
    /// Verifies round-trip fidelity: saves then reloads 1M records and
    /// checks the first and last book are preserved exactly.
    /// </summary>
    [Fact]
    public async Task RoundTrip_OneMillion_Books_DataIntact()
    {
        var original = await Repo(DataFile).LoadAsync();

        var roundTripFile = Path.Combine(_dir, "roundtrip.xml");
        await Repo(roundTripFile).SaveAsync(original);
        var reloaded = await Repo(roundTripFile).LoadAsync();

        Assert.Equal(RecordCount, reloaded.Count);

        // Spot-check first and last records.
        Assert.Equal(original[0].Name,  reloaded[0].Name);
        Assert.Equal(original[0].Author, reloaded[0].Author);
        Assert.Equal(original[0].Pages,  reloaded[0].Pages);

        Assert.Equal(original[^1].Name,  reloaded[^1].Name);
        Assert.Equal(original[^1].Author, reloaded[^1].Author);
        Assert.Equal(original[^1].Pages,  reloaded[^1].Pages);
    }

    /// <summary>Sorts 1 000 000 books in-memory and checks timing + order.</summary>
    [Fact]
    public async Task Sort_OneMillion_Books_WithinBudget()
    {
        var books = await Repo(DataFile).LoadAsync();

        var sorter  = new BookSortService();
        var elapsed = Measure("Sort 1M", () =>
        {
            var sorted = sorter.Sort(books);

            // Verify ordering at the boundary between two adjacent authors.
            for (int i = 1; i < sorted.Count; i++)
            {
                var prev = sorted[i - 1];
                var curr = sorted[i];
                int cmp  = StringComparer.OrdinalIgnoreCase.Compare(prev.Author, curr.Author);
                Assert.True(cmp <= 0, $"Author order violated at index {i}");
                if (cmp == 0)
                    Assert.True(
                        StringComparer.OrdinalIgnoreCase.Compare(prev.Name, curr.Name) <= 0,
                        $"Name order violated at index {i} (same author)");
            }
        });

        Assert.True(elapsed < SortBudget,
            $"Sort took {elapsed:g}, exceeded budget {SortBudget:g}");
    }

    /// <summary>
    /// Searches 1 000 000 books by a substring that matches a known fraction
    /// and checks timing + non-empty result.
    /// </summary>
    [Fact]
    public async Task Search_OneMillion_Books_WithinBudget()
    {
        var books = await Repo(DataFile).LoadAsync();

        var searcher = new BookSearchService();

        // "Chronicles" appears in every 200th name template slot → ~5 000 hits
        int    hits    = 0;
        var    elapsed = Measure("Search 1M ('Chronicles')", () =>
        {
            var results = searcher.SearchByName(books, "Chronicles");
            hits = results.Count;
            Assert.All(results, b =>
                Assert.Contains("Chronicles", b.Name, StringComparison.OrdinalIgnoreCase));
        });

        _out.WriteLine($"[Search] {hits:N0} hits for 'Chronicles'");
        Assert.True(hits > 0, "Expected at least one match for 'Chronicles'.");
        Assert.True(elapsed < SearchBudget,
            $"Search took {elapsed:g}, exceeded budget {SearchBudget:g}");
    }

    /// <summary>
    /// Memory smoke test: loads 1M records, forces a GC collect, then
    /// checks that the live heap is below a conservative ceiling.
    /// This is not a precise leak detector — see the stress tests for that.
    /// </summary>
    [Fact]
    public async Task Load_OneMillion_Books_MemoryWithinReasonableBounds()
    {
        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        long before = GC.GetTotalMemory(forceFullCollection: true);

        var books = await Repo(DataFile).LoadAsync();
        Assert.Equal(RecordCount, books.Count);

        GC.Collect(2, GCCollectionMode.Forced, blocking: true);
        long after = GC.GetTotalMemory(forceFullCollection: true);

        long deltaMb = (after - before) / 1_048_576;
        _out.WriteLine($"[Memory] Δ {deltaMb} MB  (before={before/1_048_576} MB  after={after/1_048_576} MB)");

        // 1M books × ~200 bytes of managed data each ≈ 200 MB ceiling.
        Assert.True(deltaMb < 500,
            $"Memory delta {deltaMb} MB exceeds 500 MB ceiling for 1M records.");
    }
}