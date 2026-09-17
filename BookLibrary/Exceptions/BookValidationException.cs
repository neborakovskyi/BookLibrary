namespace BookLibrary.Exceptions;

/// <summary>
/// Thrown when a <see cref="Models.Book"/> is constructed or modified with
/// invalid field values (blank title, non-positive page count, etc.).
/// </summary>
public sealed class BookValidationException : BookLibraryException
{
    /// <summary>Name of the invalid field, e.g. "Name" or "Pages".</summary>
    public string FieldName { get; }
    /// <summary>
    /// Initializes a new instance of the <see cref="BookValidationException"/> class with a specified field name and error message.
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="message"></param>
    public BookValidationException(string fieldName, string message)
        : base(message)
    {
        FieldName = fieldName;
    }
}
