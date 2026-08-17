namespace Frends.Pdf.Create.Definitions;

/// <summary>
/// Result-class for the task.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates if the operation was successful.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Name of the file which was created.
    /// </summary>
    /// <example>C:\tmp\example_file.pdf</example>
    public string FileName { get; private set; }

    /// <summary>
    /// Error details. Null when Success is true.
    /// </summary>
    /// <example>null</example>
    public Error Error { get; private set; }

    internal Result(bool success, string fileName, Error error = null)
    {
        Success = success;
        FileName = fileName;
        Error = error;
    }
}
