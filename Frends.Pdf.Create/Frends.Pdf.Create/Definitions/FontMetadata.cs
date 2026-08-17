using System.IO;
using OpenFontSharp;

namespace Frends.Pdf.Create.Definitions;

internal class FontMetadata
{
    /// <summary>Font family name of the font file.</summary>
    /// <example>Arial</example>
    public string Name { get; }

    /// <summary>Absolute path to the font file on disk.</summary>
    /// <example>C:\Windows\Fonts\arial.ttf</example>
    public string FullPath { get; }

    /// <summary>Whether the font is a bold variant.</summary>
    /// <example>false</example>
    public bool IsBold { get; }

    /// <summary>Whether the font is an italic variant.</summary>
    /// <example>false</example>
    public bool IsItalic { get; }

    public FontMetadata(string fontFilePath)
    {
        FullPath = fontFilePath;

        if (!File.Exists(fontFilePath))
        {
            Name = Path.GetFileNameWithoutExtension(fontFilePath);
            return;
        }

        using var fs = new FileStream(fontFilePath, FileMode.Open, FileAccess.Read);
        var reader = new OpenFontReader();
        var typeface = reader.Read(fs);

        var (bold, italic) = GetStyleFromFlags(typeface);

        Name = typeface.Name;

        IsBold = bold;
        IsItalic = italic;
    }

    private static (bool, bool) GetStyleFromFlags(Typeface tf)
    {
        bool italic = (tf.OS2Table.fsSelection & 0x01) != 0;
        bool bold = (tf.OS2Table.fsSelection & 0x20) != 0;

        return (bold, italic);
    }
}
