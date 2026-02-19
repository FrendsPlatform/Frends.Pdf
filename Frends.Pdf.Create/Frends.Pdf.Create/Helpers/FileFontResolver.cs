using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using PdfSharp.Fonts;

namespace Frends.Pdf.Create.Helpers;
internal class FileFontResolver : IFontResolver
{
// Dictionary to map "familyname_bold_italic" to a physical file path
    private static readonly Dictionary<string, string> _fontMap = new(StringComparer.OrdinalIgnoreCase);
    private static readonly string WindowsFontsPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
    //TODO check for getting linux folder
    static FileFontResolver()
    {
        //todo try to get info about font by some additional package
        // add option to specify font folder
        // maybe add option to map font name to font file etc.

        // One-time scan of the fonts folder to see what's actually installed
        // This avoids hardcoding and is much faster than GDI+
        foreach (var file in Directory.GetFiles(WindowsFontsPath, "*.ttf"))
        {
            try
            {
                // We use the filename as the key.
                // Note: Most Windows fonts follow standard naming (arial, arialbd, etc.)
                _fontMap[Path.GetFileName(file)] = file;
            }
            catch { /* Skip corrupted files */ }
        }
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        string name = familyName.ToLower();

        // Basic mapping logic for common naming conventions
        string suffix = "";
        if (isBold && isItalic) suffix = "bi";
        else if (isBold) suffix = "bd";
        else if (isItalic) suffix = "i";

        string expectedFile = $"{name}{suffix}.ttf";

        // Check if the file exists in our scanned map
        if (_fontMap.ContainsKey(expectedFile))
        {
            return new FontResolverInfo(expectedFile);
        }

        // Fallback to the regular version if the specific style (bold/italic) isn't found
        if (_fontMap.ContainsKey($"{name}.ttf"))
        {
            return new FontResolverInfo($"{name}.ttf");
        }

        // Final safety fallback so the app never crashes
        return new FontResolverInfo("arial.ttf");
    }

    public byte[] GetFont(string faceName)
    {
        // faceName is the string we put in FontResolverInfo above
        string fullPath = Path.Combine(WindowsFontsPath, faceName);

        return File.Exists(fullPath) ? File.ReadAllBytes(fullPath) : null;
    }
}
