using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Frends.Pdf.Create.Definitions;
using MigraDoc;
using PdfSharp.Fonts;

namespace Frends.Pdf.Create.Helpers;

internal class FileFontResolver : IFontResolver
{
    private static readonly List<FontMetadata> Fonts;
    private static string defaultFamilyName;

    private const string BundledFont = "Victor Mono";

    static FileFontResolver()
    {
        Fonts = [];
    }

    internal static void Setup(string defaultName = BundledFont, string customFontsLocation = null)
    {
        defaultFamilyName = string.IsNullOrWhiteSpace(defaultName) ? BundledFont : defaultName;
        PredefinedFontsAndChars.ErrorFontName = defaultFamilyName;

        var fontsLocations = GetFontsLocations(customFontsLocation);
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
                var alreadyExists = Fonts.Any(f =>
                    f.Name.Equals(newFont.Name, StringComparison.CurrentCultureIgnoreCase) &&
                    f.IsBold == newFont.IsBold && f.IsItalic == newFont.IsItalic);
                if (!alreadyExists) Fonts.Add(newFont);
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
            Fonts.FirstOrDefault(f =>
                f.Name.Equals(familyName, StringComparison.CurrentCultureIgnoreCase)
                && f.IsBold == isBold
                && f.IsItalic == isItalic)
            // try to get a regular font from family
            ?? Fonts.FirstOrDefault(f =>
                f.Name.Equals(familyName, StringComparison.CurrentCultureIgnoreCase)
                && !f.IsBold
                && !f.IsItalic)
            // try to get any font from the family
            ?? Fonts.FirstOrDefault(f => f.Name.Equals(familyName, StringComparison.CurrentCultureIgnoreCase))
            // try to get a regular fallback font
            ?? Fonts.FirstOrDefault(f =>
                f.Name.Equals(defaultFamilyName, StringComparison.CurrentCultureIgnoreCase)
                && !f.IsBold
                && !f.IsItalic)
            // try to get any font from the fallback family
            ?? Fonts.FirstOrDefault(f =>
                f.Name.Equals(defaultFamilyName, StringComparison.CurrentCultureIgnoreCase))
            ?? new FontMetadata(BundledFont);

        return new FontResolverInfo(font.FullPath);
    }

    public byte[] GetFont(string path)
    {
        try
        {
            return File.Exists(path) ? File.ReadAllBytes(path) : throw new Exception("Could not find font file");
        }
        catch (Exception e)
        {
            var assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "Frends.Pdf.Create.VictorMono-Regular.ttf";
            var fs = assembly.GetManifestResourceStream(resourceName);
            using var ms = new MemoryStream();

            if (fs != null) fs.CopyTo(ms);
            else throw new Exception("Could not resolve bundled font file.", e);

            return ms.ToArray();
        }
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
