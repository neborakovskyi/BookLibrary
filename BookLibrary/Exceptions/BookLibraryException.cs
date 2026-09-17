namespace BookLibrary.Exceptions;

/// <summary>
/// Base class for all domain exceptions thrown by BookLibrary.
/// Catching this type intercepts any library-specific error.
/// </summary>
public class BookLibraryException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BookLibraryException"/> class with a specified error message.
    /// </summary>
    /// <param name="message"></param>
    public BookLibraryException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="BookLibraryException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public BookLibraryException(string message, Exception innerException)
        : base(message, innerException) { }
}
