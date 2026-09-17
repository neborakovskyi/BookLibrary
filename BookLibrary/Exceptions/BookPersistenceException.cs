namespace BookLibrary.Exceptions;

/// <summary>
/// Thrown when loading from or saving to an XML file fails —
/// covers both I/O errors and malformed XML / missing elements.
/// </summary>
public sealed class BookPersistenceException : BookLibraryException
{
    /// <summary>File path involved in the failed operation.</summary>
    public string FilePath { get; }
    /// <summary>
    /// Initializes a new instance of the <see cref="BookPersistenceException"/> class with a specified file path and error message.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="message"></param>
    public BookPersistenceException(string filePath, string message)
        : base(message)
    {
        FilePath = filePath;
    }
    /// <summary>
    /// Initializes a new instance of the <see cref="BookPersistenceException"/> class with a specified file path, error message, and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public BookPersistenceException(string filePath, string message, Exception innerException)
        : base(message, innerException)
    {
        FilePath = filePath;
    }
}
