using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Frends.Pdf.Create.Definitions;
using PdfSharp.Fonts;

namespace Frends.Pdf.Create.Helpers;

internal class FileFontResolver : IFontResolver
{
    private readonly List<FontMetadata> _fonts = [];
    private readonly string[] _fontsLocations;
    private readonly string _defaultFamilyName;
    private readonly string _customFontsLocation;

    public FileFontResolver(string defaultFamilyName = "Arial", string customFontsLocation = null)
    {
        _fontsLocations = GetFontsLocations();
        _defaultFamilyName = string.IsNullOrWhiteSpace(defaultFamilyName) ? "Arial" : defaultFamilyName;
        _customFontsLocation = string.IsNullOrWhiteSpace(customFontsLocation) ? null : defaultFamilyName;
        List<string> fontsPaths = [];
        foreach (var location in _fontsLocations)
        {
            fontsPaths.AddRange(Directory.GetFiles(location, "*.ttf", SearchOption.AllDirectories));
        }

        foreach (var file in fontsPaths)
        {
            try
            {
                _fonts.Add(new FontMetadata(file));
            }
            catch
            {
                /* Skip corrupted files */
            }
        }
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        var font =
            // try to get font we are looking for
            _fonts.FirstOrDefault(f =>
                f.Name.Equals(familyName, StringComparison.CurrentCultureIgnoreCase)
                && f.IsBold == isBold
                && f.IsItalic == isItalic)
            // try to get regular font from family
            ?? _fonts.FirstOrDefault(f =>
                f.Name.Equals(familyName, StringComparison.CurrentCultureIgnoreCase)
                && !f.IsBold
                && !f.IsItalic)
            // try to get any font from family
            ?? _fonts.FirstOrDefault(f => f.Name.Equals(familyName, StringComparison.CurrentCultureIgnoreCase))
            // try to get regular fallback font
            ?? _fonts.FirstOrDefault(f =>
                f.Name.Equals(_defaultFamilyName, StringComparison.CurrentCultureIgnoreCase)
                && !f.IsBold
                && !f.IsItalic)
            // try to get any font from fallback family
            ?? _fonts.FirstOrDefault(f =>
                f.Name.Equals(_defaultFamilyName, StringComparison.CurrentCultureIgnoreCase))
            ?? throw new Exception(
                $"Font: {familyName} {(!isBold && !isItalic ? "regular" : string.Empty)} {(isBold ? "bold" : string.Empty)} {(isItalic ? "italic" : string.Empty)}, couldn't be resolved");
        return new FontResolverInfo(font.FileName);
    }

    public byte[] GetFont(string faceName)
    {
        foreach (var location in _fontsLocations)
        {
            string fullPath = Path.Combine(location, faceName);
            if (File.Exists(fullPath))
            {
                return File.ReadAllBytes(fullPath);
            }
        }

        throw new Exception("Could not find font file");
    }

    private string[] GetFontsLocations()
    {
        List<string> result = [];
        if (OperatingSystem.IsWindows())
        {
            result.Add(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));
        }
        else if (OperatingSystem.IsLinux())
        {
            result.AddRange([
                "/usr/share/fonts",
                "/usr/local/share/fonts",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/fonts"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts")
            ]);
        }
        else throw new Exception("Unsupported operating system");

        if (_customFontsLocation != null) result.Add(_customFontsLocation);

        return result.ToArray();
    }
}
