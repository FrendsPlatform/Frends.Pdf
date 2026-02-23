using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Frends.Pdf.Create.Definitions;
using PdfSharp.Fonts;

namespace Frends.Pdf.Create.Helpers;

internal class FileFontResolver : IFontResolver
{
    private static AsyncLocal<List<FontMetadata>> _fonts;
    private static AsyncLocal<string> _defaultFamilyName;

    public FileFontResolver()
    {
        _fonts = new AsyncLocal<List<FontMetadata>>();
        _defaultFamilyName = new AsyncLocal<string>();
        _fonts.Value = [];
    }

    public static void Setup(string defaultFamilyName = "Arial", string customFontsLocation = null)
    {
        var fontsLocations = GetFontsLocations(customFontsLocation);
        _defaultFamilyName.Value = string.IsNullOrWhiteSpace(defaultFamilyName) ? "Arial" : defaultFamilyName;
        List<string> fontsPaths = [];

        foreach (var location in fontsLocations)
        {
            fontsPaths.AddRange(Directory.GetFiles(location, "*.ttf", SearchOption.AllDirectories));
        }

        foreach (var file in fontsPaths)
        {
            try
            {
                var newFont = new FontMetadata(file);
                var alreadyExists = _fonts.Value.Any(f =>
                    f.Name.Equals(newFont.Name, StringComparison.CurrentCultureIgnoreCase) &&
                    f.IsBold == newFont.IsBold && f.IsItalic == newFont.IsItalic);
                if (!alreadyExists) _fonts.Value.Add(newFont);
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
            // try to get the font we are looking for
            _fonts.Value.FirstOrDefault(f =>
                f.Name.Equals(familyName, StringComparison.CurrentCultureIgnoreCase)
                && f.IsBold == isBold
                && f.IsItalic == isItalic)
            // try to get a regular font from family
            ?? _fonts.Value.FirstOrDefault(f =>
                f.Name.Equals(familyName, StringComparison.CurrentCultureIgnoreCase)
                && !f.IsBold
                && !f.IsItalic)
            // try to get any font from family
            ?? _fonts.Value.FirstOrDefault(f => f.Name.Equals(familyName, StringComparison.CurrentCultureIgnoreCase))
            // try to get a regular fallback font
            ?? _fonts.Value.FirstOrDefault(f =>
                f.Name.Equals(_defaultFamilyName.Value, StringComparison.CurrentCultureIgnoreCase)
                && !f.IsBold
                && !f.IsItalic)
            // try to get any font from the fallback family
            ?? _fonts.Value.FirstOrDefault(f =>
                f.Name.Equals(_defaultFamilyName.Value, StringComparison.CurrentCultureIgnoreCase))
            ?? throw new Exception(
                $"Font: {familyName} {(!isBold && !isItalic ? "regular," : string.Empty)}{(isBold ? "bold," : string.Empty)}{(isItalic ? "italic," : string.Empty)} couldn't be resolved");

        return new FontResolverInfo(font.FullPath);
    }

    public byte[] GetFont(string path)
    {
        return File.Exists(path) ? File.ReadAllBytes(path) : throw new Exception("Could not find font file");
    }

    private static string[] GetFontsLocations(string customFontsLocation)
    {
        var customLocation = string.IsNullOrWhiteSpace(customFontsLocation) ? null : customFontsLocation;
        List<string> result = [];
        List<string> potentialPaths = [customLocation];

        if (OperatingSystem.IsWindows())
        {
            potentialPaths.Add(Environment.GetFolderPath(Environment.SpecialFolder.Fonts));
        }
        else if (OperatingSystem.IsLinux())
        {
            potentialPaths.AddRange(
            [
                "/usr/share/fonts",
                "/usr/local/share/fonts",
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local/share/fonts"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".fonts"),
            ]);
        }
        else throw new Exception("Unsupported operating system");

        foreach (var potentialPath in potentialPaths.Where(potentialPath =>
                     Directory.Exists(potentialPath) && !result.Contains(potentialPath)))
            result.Add(potentialPath);

        return result.ToArray();
    }
}
