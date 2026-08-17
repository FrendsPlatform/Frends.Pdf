using System;

namespace Frends.Pdf.Create.Definitions;

/// <summary>
/// Error information returned when the task fails and ThrowErrorOnFailure is false.
/// </summary>
public class Error
{
    /// <summary>
    /// Human-readable description of the error.
    /// </summary>
    /// <example>Output file already exists: C:\Output\example_file.pdf</example>
    public string Message { get; set; }

    /// <summary>
    /// The exception that caused the failure.
    /// </summary>
    /// <example>System.IO.FileNotFoundException: Could not find file 'example.pdf'</example>
    public Exception AdditionalInfo { get; set; }
}
