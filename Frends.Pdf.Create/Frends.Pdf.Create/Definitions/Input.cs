using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Pdf.Create.Definitions;

/// <summary>
/// Input parameters for the Create task.
/// </summary>
public class Input
{
    /// <summary>
    /// PDF document destination Directory.
    /// </summary>
    /// <example>F:\outputfiles</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue(@"C:\Output")]
    public string Directory { get; set; }

    /// <summary>
    /// Filename for created PDF file.
    /// </summary>
    /// <example>output.pdf</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("example_file.pdf")]
    public string FileName { get; set; }

    /// <summary>
    /// What to do if destination file already exists.
    /// </summary>
    /// <example>Error</example>
    [DefaultValue(FileExistsActionEnum.Error)]
    public FileExistsActionEnum FileExistsAction { get; set; }

    /// <summary>
    /// Content elements to render in the PDF document.
    /// </summary>
    /// <example>null</example>
    [Required]
    public DocumentContent Content { get; set; }
}
