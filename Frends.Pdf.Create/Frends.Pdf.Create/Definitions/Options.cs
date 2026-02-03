using System.ComponentModel;

namespace Frends.Pdf.Create.Definitions;

/// <summary>
/// Class for task options.
/// </summary>
public class Options
{
    /// <summary>
    /// True: Throws error on failure
    /// False: Returns object{ Success = false }
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;
}
