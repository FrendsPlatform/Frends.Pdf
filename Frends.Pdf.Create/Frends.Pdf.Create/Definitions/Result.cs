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
    public bool Success { get; init; }

    /// <summary>
    /// Name of the file which was created.
    /// </summary>
    /// <example>C:\tmp\example_file.pdf</example>
    public string FileName { get; init; }

    /// <summary>
    /// Error details. Null when Success is true.
    /// </summary>
    /// <example>null</example>
    public Error Error { get; init; }
}
