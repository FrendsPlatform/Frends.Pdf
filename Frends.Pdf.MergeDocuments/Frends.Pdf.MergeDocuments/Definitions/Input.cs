using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Pdf.MergeDocuments.Definitions;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Paths to files to merge.
    /// </summary>
    /// <example>["C:/files/foo.pdf", "C:/files/bar.pdf"]</example>
    public string[] InputFilePaths { get; set; }

    /// <summary>
    /// path where to save merged PDF.
    /// </summary>
    /// <example>2</example>
    [DefaultValue("C:/files/merged.pdf")]
    public string DestinationFilePath { get; set; }
}
