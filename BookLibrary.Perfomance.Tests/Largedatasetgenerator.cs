using System.Text;
using System.Xml;

namespace BookLibrary.PerformanceTests;

/// <summary>
/// Generates a deterministic XML file with exactly <see cref="DefaultCount"/>
/// book records, using a streaming <see cref="XmlWriter"/> so the full DOM
/// is never held in memory at once.
///
/// Authors are drawn from a pool of 50 realistic names; names from 200
/// templates. This produces a realistic skewed distribution (multiple books
/// per author) rather than 1 M unique strings, which exercises sort and
/// search more faithfully.
/// </summary>
public static class LargeDatasetGenerator
{
    public const int DefaultCount = 1_000_000;

    // ── Author / name pools ──────────────────────────────────────────

    private static readonly string[] Authors =
    {
        "Andersen", "Asimov", "Atwood", "Austen", "Bradbury",
        "Bronte", "Bulgakov", "Camus", "Chandler", "Christie",
        "Clarke", "Conan Doyle", "Conrad", "Dickens", "Dostoevsky",
        "Dumas", "Faulkner", "Fitzgerald", "Flaubert", "Gaiman",
        "Garcia Marquez", "Gibson", "Gogol", "Golding", "Greene",
        "Hardy", "Hemingway", "Herbert", "Hugo", "Huxley",
        "Joyce", "Kafka", "King", "Kipling", "Le Guin",
        "London", "Lovecraft", "Mann", "Maugham", "Melville",
        "Nabokov", "Orwell", "Poe", "Proust", "Pushkin",
        "Remarque", "Steinbeck", "Stoker", "Tolstoy", "Woolf"
    };

    private static readonly string[] NameTemplates =
    {
        "The {0} Chronicles",    "A {0} in the Dark",      "Beyond the {0}",
        "The Last {0}",          "Return of the {0}",       "Shadow of {0}",
        "Children of {0}",       "Rise of the {0}",         "The {0} Enigma",
        "Under {0} Skies",       "The {0} Legacy",          "Edge of {0}",
        "Night of the {0}",      "The {0} Protocol",        "Heart of {0}",
        "The {0} Paradox",       "Dawn of {0}",             "The {0} Prophecy",
        "Echoes of {0}",         "The {0} Covenant",        "War of {0}",
        "The {0} Labyrinth",     "Secrets of {0}",          "The {0} Frontier",
        "Land of {0}",           "The {0} Gambit",          "Voice of {0}",
        "The {0} Rebellion",     "Throne of {0}",           "The {0} Manifesto",
        "Storm of {0}",          "The {0} Archive",         "Song of {0}",
        "The {0} Dilemma",       "Fires of {0}",            "The {0} Expedition",
        "World of {0}",          "The {0} Conspiracy",      "Wrath of {0}",
        "The {0} Inheritance",   "Fall of {0}",             "The {0} Testament",
        "Winds of {0}",          "The {0} Discovery",       "Ghost of {0}",
        "The {0} Revelation",    "Age of {0}",              "The {0} Encounter",
        "Ruins of {0}",          "The {0} Experiment",      "Birth of {0}",
        "The {0} Manuscript",    "Eye of {0}",              "The {0} Resistance",
        "Time of {0}",           "The {0} Breach",          "Gates of {0}",
        "The {0} Convergence",   "Path of {0}",             "The {0} Dominion",
        "Blood of {0}",          "The {0} Horizon",         "Crown of {0}",
        "The {0} Reckoning",     "Order of {0}",            "The {0} Ascension",
        "Mind of {0}",           "The {0} Initiative",      "Empire of {0}",
        "The {0} Divergence",    "End of {0}",              "The {0} Theory",
        "Kingdom of {0}",        "The {0} Collapse",        "Soul of {0}",
        "The {0} Emergence",     "Cry of {0}",              "The {0} Threshold",
        "Light of {0}",          "The {0} Doctrine",        "Rise and {0}",
        "The {0} Ultimatum",     "Power of {0}",            "The {0} Voyage",
        "Myth of {0}",           "The {0} Uprising",        "Lair of {0}",
        "The {0} Syndrome",      "Days of {0}",             "The {0} Equation",
        "Depths of {0}",         "The {0} Anomaly",         "Hunt for {0}",
        "The {0} Conquest",      "Siege of {0}",            "The {0} Transition",
        "Legend of {0}",         "The {0} Accord",          "Curse of {0}",
        "The {0} Revolution",    "Fragments of {0}",        "The {0} Paradox II",
        "Birth of the {0}",      "The {0} Sequence",        "Eyes of {0}",
        "The {0} Prophecy II",   "Children of the {0}",     "The {0} Factor",
        "Daughters of {0}",      "The {0} Saga",            "Borders of {0}",
        "The {0} Stratagem",     "Masters of {0}",          "The {0} Dominance",
        "Lords of {0}",          "The {0} Directive",       "Echoes from {0}",
        "The {0} Veil",          "Watchers of {0}",         "The {0} Spectrum",
        "Harbingers of {0}",     "The {0} Signal",          "Shadows of {0}",
        "The {0} Riddle",        "Keepers of {0}",          "The {0} Recurrence",
        "Harbors of {0}",        "The {0} Pact",            "Winds from {0}",
        "The {0} Configuration", "Pillars of {0}",          "The {0} Rift",
        "Prophecy of {0}",       "The {0} Decree",          "Realm of {0}",
        "The {0} Lament",        "Heralds of {0}",          "The {0} Imperative",
        "Crusade of {0}",        "The {0} Pursuit",         "Architects of {0}",
        "The {0} Descent",       "Keepers of the {0}",      "The {0} Mandate",
        "Progenitors of {0}",    "The {0} Obsession",       "Bearers of {0}",
        "The {0} Design",        "Seekers of {0}",          "The {0} Annihilation",
        "Disciples of {0}",      "The {0} Schism",          "Daughters of the {0}",
        "The {0} Relic",         "Champions of {0}",        "The {0} Delusion",
        "Legends of {0}",        "The {0} Mechanism",       "Prophets of {0}",
        "The {0} Contingency",   "Wanderers of {0}",        "The {0} Transformation",
        "Survivors of {0}",      "The {0} Resolution",      "Sons of {0}",
        "The {0} Intervention",  "Fragments from {0}",      "The {0} Foundation",
        "Inheritors of {0}",     "The {0} Compendium",      "Avatars of {0}",
        "The {0} Transcendence", "Remnants of {0}",         "The {0} Oscillation",
        "Watchers from {0}",     "The {0} Consequence",     "Guardians of {0}",
        "The {0} Declaration",   "Emissaries of {0}",       "The {0} Dissolution",
        "Heralds from {0}",      "The {0} Trajectory",      "Architects of the {0}",
        "The {0} Commission",    "Echoes of the {0}",       "Caretakers of {0}",
        "The {0} Inception",     "Outlanders of {0}",       "The {0} Unraveling",
        "Pioneers of {0}",       "The {0} Convergence II",  "Outcasts of {0}",
        "The {0} Prelude",       "Builders of {0}",         "The {0} Conclusion"
    };

    private static readonly string[] NameWords =
    {
        "Darkness", "Light", "Fire", "Ice", "Storm", "Earth", "Sky",
        "Sea", "Star", "Moon", "Sun", "Wind", "Rain", "Thunder",
        "Shadow", "Dream", "Fate", "Time", "Space", "Power",
        "Chaos", "Order", "Truth", "Lies", "Hope", "Fear",
        "War", "Peace", "Death", "Life", "Blood", "Gold",
        "Silver", "Iron", "Stone", "Glass", "Sand", "Snow",
        "Mist", "Smoke", "Ash", "Ember", "Flame", "Wave",
        "Tide", "Current", "Void", "Abyss", "Zenith", "Nadir"
    };

    // ── Public API ────────────────────────────────────────────────────

    /// <summary>
    /// Generates an XML file with <paramref name="count"/> book records
    /// using a streaming writer (constant memory regardless of count).
    /// </summary>
    /// <param name="filePath">Destination path (parent dirs are created).</param>
    /// <param name="count">Number of records to generate.</param>
    /// <param name="cancellationToken">Optional cancellation.</param>
    public static async Task GenerateAsync(
        string            filePath,
        int               count             = DefaultCount,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Path must not be blank.", nameof(filePath));
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");

        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir))
            Directory.CreateDirectory(dir);

        var settings = new XmlWriterSettings
        {
            Async    = true,
            Indent   = false,          // no indentation → ~30 % smaller file
            Encoding = Encoding.UTF8
        };

        await using var stream = new FileStream(
            filePath, FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 1 << 16,       // 64 KB write buffer
            useAsync: true);

        await using var writer = XmlWriter.Create(stream, settings);

        await writer.WriteStartDocumentAsync().ConfigureAwait(false);
        await writer.WriteStartElementAsync(null, "Books", null).ConfigureAwait(false);

        for (int i = 0; i < count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var author = Authors[i % Authors.Length];
            var name   = BuildName(i);
            var pages  = 50 + (i % 950);   // 50 … 999

            await writer.WriteStartElementAsync(null, "Book",   null).ConfigureAwait(false);
            await writer.WriteElementStringAsync(null, "Name",  null, name) .ConfigureAwait(false);
            await writer.WriteElementStringAsync(null, "Author", null, author).ConfigureAwait(false);
            await writer.WriteElementStringAsync(null, "Pages",  null, pages.ToString()).ConfigureAwait(false);
            await writer.WriteEndElementAsync().ConfigureAwait(false);   // </Book>
        }

        await writer.WriteEndElementAsync().ConfigureAwait(false);       // </Books>
        await writer.WriteEndDocumentAsync().ConfigureAwait(false);
        await writer.FlushAsync().ConfigureAwait(false);
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private static string BuildName(int i)
    {
        var template = NameTemplates[i % NameTemplates.Length];
        var word     = NameWords[(i / NameTemplates.Length) % NameWords.Length];
        return string.Format(template, word);
    }
}