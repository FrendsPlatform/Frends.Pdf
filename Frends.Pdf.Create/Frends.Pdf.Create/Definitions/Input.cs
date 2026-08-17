using System.ComponentModel.DataAnnotations;

namespace Frends.Pdf.Create.Definitions;

/// <summary>
/// Input parameters for the Create task.
/// </summary>
public class Input
{
    /// <summary>
    /// Output file properties (directory, filename, and action if file already exists).
    /// </summary>
    /// <example>null</example>
    [Required]
    public FileProperties OutputFile { get; set; }

    /// <summary>
    /// Document settings such as page size, orientation and margins.
    /// </summary>
    /// <example>null</example>
    [Required]
    public DocumentSettings DocumentSettings { get; set; }

    /// <summary>
    /// Content elements to render in the PDF document.
    /// </summary>
    /// <example>null</example>
    [Required]
    public DocumentContent Content { get; set; }
}
