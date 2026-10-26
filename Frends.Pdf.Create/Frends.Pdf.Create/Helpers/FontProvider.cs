using System;
using System.Collections.Generic;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;

namespace Frends.Pdf.Create.Helpers;

/// <summary>Font selection for a piece of text.</summary>
internal sealed record FontSpec(string Family, bool Bold, bool Italic);

/// <summary>
/// Loads fonts into the output document and measures text widths.
/// Measurement uses a separate scratch document, so the output document contains only real content.
/// </summary>
internal sealed class FontProvider
{
    private const double MeasureSize = 1000;

    private readonly PdfDocumentBuilder document;
    private readonly PdfDocumentBuilder scratch = new();
    private readonly PdfPageBuilder scratchPage;
    private readonly Dictionary<string, PdfDocumentBuilder.AddedFont> documentFonts = new();
    private readonly Dictionary<string, PdfDocumentBuilder.AddedFont> scratchFonts = new();
    private readonly Dictionary<(string, char), double> widths = new();

    internal FontProvider(PdfDocumentBuilder document)
    {
        this.document = document;
        scratchPage = scratch.AddPage(100, 100);
    }

    internal PdfDocumentBuilder.AddedFont GetFont(FontSpec spec)
    {
        var metadata = FileFontResolver.Resolve(spec.Family, spec.Bold, spec.Italic);
        if (!documentFonts.TryGetValue(metadata.FullPath, out var font))
        {
            font = document.AddTrueTypeFont(FileFontResolver.GetFont(metadata.FullPath));
            documentFonts[metadata.FullPath] = font;
        }

        return font;
    }

    internal double Measure(FontSpec spec, double size, string text)
    {
        var metadata = FileFontResolver.Resolve(spec.Family, spec.Bold, spec.Italic);
        var total = 0.0;
        foreach (var c in text) total += CharWidth(metadata.FullPath, c);

        return total * size / MeasureSize;
    }

    private double CharWidth(string fontKey, char c)
    {
        if (widths.TryGetValue((fontKey, c), out var cached)) return cached;

        if (!scratchFonts.TryGetValue(fontKey, out var font))
        {
            font = scratch.AddTrueTypeFont(FileFontResolver.GetFont(fontKey));
            scratchFonts[fontKey] = font;
        }

        double width;
        try
        {
            var letters = scratchPage.MeasureText(c.ToString(), MeasureSize, new PdfPoint(0, 0), font);
            width = letters.Count > 0 ? letters[^1].EndBaseLine.X - letters[0].StartBaseLine.X : MeasureSize * 0.5;
        }
        catch (Exception)
        {
            width = MeasureSize * 0.5;
        }

        widths[(fontKey, c)] = width;

        return width;
    }
}
