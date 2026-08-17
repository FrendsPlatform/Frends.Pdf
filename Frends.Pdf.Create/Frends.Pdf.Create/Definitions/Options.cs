using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.Pdf.Create.Definitions;

/// <summary>
/// Class for task options.
/// </summary>
public class Options
{
    /// <summary>
    /// True: Throws error on failure
    /// False: Returns an object{ Success = false }
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure.
    /// </summary>
    /// <example>PDF creation failed: check input parameters</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;

    /// <summary>
    /// Optional PDF document title.
    /// </summary>
    /// <example>Very important document.</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Title { get; set; }

    /// <summary>
    /// Optional PDF document Author.
    /// </summary>
    /// <example>Erik Example</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Author { get; set; }

    /// <summary>
    /// Document page size.
    /// </summary>
    /// <example>A4</example>
    [DefaultValue(PageSizeEnum.A4)]
    public PageSizeEnum Size { get; set; }

    /// <summary>
    /// Page orientation.
    /// </summary>
    /// <example>Portrait</example>
    [DefaultValue(PageOrientationEnum.Portrait)]
    public PageOrientationEnum Orientation { get; set; }

    /// <summary>
    /// Page margin left in CM.
    /// </summary>
    /// <example>2.5</example>
    [DefaultValue(2.5)]
    public double MarginLeftInCm { get; set; }

    /// <summary>
    /// Page margin top in CM.
    /// </summary>
    /// <example>2</example>
    [DefaultValue(2)]
    public double MarginTopInCm { get; set; }

    /// <summary>
    /// Page margin right in CM.
    /// </summary>
    /// <example>2.5</example>
    [DefaultValue(2.5)]
    public double MarginRightInCm { get; set; }

    /// <summary>
    /// Page margin bottom in CM.
    /// </summary>
    /// <example>2</example>
    [DefaultValue(2)]
    public double MarginBottomInCm { get; set; }

    /// <summary>
    /// Path to a directory with fonts to use.
    /// If empty, a task will use default system locations
    /// Otherwise, CustomFontsLocation will be used as well as default system locations, unless it does not exist.
    /// </summary>
    /// <example>C:\MyDir\fonts</example>
    [DefaultValue("")]
    public string CustomFontsLocation { get; set; } = string.Empty;

    /// <summary>
    /// Font family name that will be used if a specific font couldn't be resolved.
    /// This font will be used as an ErrorFontName as well.
    /// Using bundled Victor Mono font if nothing is provided.
    /// </summary>
    /// <example>Arial</example>
    [DefaultValue("")]
    public string FallbackFontName { get; set; } = string.Empty;
}
