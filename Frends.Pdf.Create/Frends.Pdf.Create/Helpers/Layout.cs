using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Frends.Pdf.Create.Definitions;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Writer;

#pragma warning disable SA1402
#pragma warning disable SA1204
namespace Frends.Pdf.Create.Helpers;

internal static class Units
{
    internal const double PointsPerCm = 72.0 / 2.54;

    internal static double CmToPt(double cm) => cm * PointsPerCm;

    internal static double PtToCm(double pt) => pt / PointsPerCm;
}

/// <summary>Everything needed to draw one page element. Y coordinates grow downwards.</summary>
internal sealed class DrawContext
{
    internal DrawContext(PdfPageBuilder page, double pageHeight, int pageNumber, int pageCount, DocumentResources resources)
    {
        Page = page;
        PageHeight = pageHeight;
        PageNumber = pageNumber;
        PageCount = pageCount;
        Resources = resources;
    }

    internal PdfPageBuilder Page { get; }

    internal double PageHeight { get; }

    internal int PageNumber { get; }

    internal int PageCount { get; }

    internal DocumentResources Resources { get; }

    internal PdfPoint Point(double x, double y) => new(x, PageHeight - y);

    internal void Line(double x1, double y1, double x2, double y2, double width)
    {
        Page.DrawLine(Point(x1, y1), Point(x2, y2), Math.Max(width, 0.01));
    }
}

/// <summary>Document level helpers shared by all layout elements.</summary>
internal sealed class DocumentResources
{
    private readonly Dictionary<string, PdfPageBuilder.AddedImage> addedImages = new();

    internal DocumentResources(FontProvider fonts)
    {
        Fonts = fonts;
    }

    internal FontProvider Fonts { get; }

    internal void DrawImage(DrawContext ctx, ImageData image, double x, double top, double width, double height)
    {
        var rect = new PdfRectangle(x, ctx.PageHeight - top - height, x + width, ctx.PageHeight - top);
        if (addedImages.TryGetValue(image.Path, out var added))
        {
            ctx.Page.AddImage(added, rect);
            return;
        }

        added = image.IsPng ? ctx.Page.AddPng(image.Bytes, rect) : ctx.Page.AddJpeg(image.Bytes, rect);
        addedImages[image.Path] = added;
    }
}

internal sealed class TextStyle
{
    internal string Family { get; init; }

    internal double Size { get; init; }

    internal bool Bold { get; init; }

    internal bool Italic { get; init; }

    internal bool Underline { get; init; }

    /// <summary>Line height in points. Zero means automatic.</summary>
    internal double LineSpacing { get; init; }

    /// <summary>When true the line spacing is a fixed height, otherwise it is a minimum.</summary>
    internal bool ExactSpacing { get; init; }

    internal double SpaceBefore { get; init; }

    internal double SpaceAfter { get; init; }

    internal ParagraphAlignmentEnum Alignment { get; init; }

    internal FontSpec Font => new(Family, Bold, Italic);

    internal double LineHeight
    {
        get
        {
            var auto = Size * 1.2;
            if (LineSpacing <= 0) return auto;

            return ExactSpacing ? LineSpacing : Math.Max(LineSpacing, auto);
        }
    }

    internal static TextStyle FromFontStyle(
        FontStyleEnum fontStyle,
        string family,
        double size,
        double lineSpacing,
        bool exact,
        double before,
        double after,
        ParagraphAlignmentEnum alignment)
    {
        return new TextStyle
        {
            Family = family,
            Size = size > 0 ? size : 11,
            Bold = fontStyle is FontStyleEnum.Bold or FontStyleEnum.BoldItalic,
            Italic = fontStyle is FontStyleEnum.Italic or FontStyleEnum.BoldItalic,
            Underline = fontStyle == FontStyleEnum.Underline,
            LineSpacing = lineSpacing,
            ExactSpacing = exact,
            SpaceBefore = before,
            SpaceAfter = after,
            Alignment = alignment,
        };
    }
}

internal sealed class ImageData
{
    internal string Path { get; init; }

    internal byte[] Bytes { get; init; }

    internal bool IsPng { get; init; }

    /// <summary>Natural size in points, assuming 96 dpi.</summary>
    internal double WidthPt { get; init; }

    internal double HeightPt { get; init; }
}

/// <summary>One drawable unit that is never split between pages.</summary>
internal abstract class LayoutItem
{
    internal double Height { get; set; }

    internal abstract void Draw(DrawContext ctx, double x, double top);
}

internal static class TextLayout
{
    /// <summary>Splits text into lines. Whitespace characters become single spaces.</summary>
    /// <param name="fonts">Font provider used to measure text.</param>
    /// <param name="style">Style applied to the text.</param>
    /// <param name="text">Text to wrap.</param>
    /// <param name="width">Maximum width of each line.</param>
    /// <returns>The wrapped text lines.</returns>
    internal static List<string> Wrap(FontProvider fonts, TextStyle style, string text, double width)
    {
        var result = new List<string>();
        using var reader = new System.IO.StringReader(text ?? string.Empty);
        while (reader.ReadLine() is { } line)
            result.AddRange(WrapLine(fonts, style, Normalize(line), width));

        if (result.Count == 0) result.Add(string.Empty);

        return result;
    }

    internal static string Normalize(string line)
    {
        var sb = new StringBuilder(line.Length);
        foreach (var c in line) sb.Append(char.IsWhiteSpace(c) ? ' ' : c);

        return sb.ToString();
    }

#pragma warning disable SA1202
    private static IEnumerable<string> WrapLine(FontProvider fonts, TextStyle style, string line, double width)
    {
        var lines = new List<string>();
        var current = new StringBuilder();
        var currentWidth = 0.0;
        var font = style.Font;

        void Flush()
        {
            lines.Add(current.ToString().TrimEnd(' '));
            current.Clear();
            currentWidth = 0;
        }

        foreach (var token in Tokenize(line))
        {
            if (token[0] == ' ')
            {
                if (current.Length == 0 && lines.Count > 0) continue;
                current.Append(token);
                currentWidth += fonts.Measure(font, style.Size, token);
                continue;
            }

            var tokenWidth = fonts.Measure(font, style.Size, token);
            if (currentWidth + tokenWidth <= width)
            {
                current.Append(token);
                currentWidth += tokenWidth;
                continue;
            }

            if (current.ToString().Trim().Length > 0) Flush();
            else if (current.Length > 0 && lines.Count > 0) current.Clear();

            if (tokenWidth <= width)
            {
                current.Append(token);
                currentWidth = tokenWidth;
                continue;
            }

            foreach (var c in token)
            {
                var charWidth = fonts.Measure(font, style.Size, c.ToString());
                if (currentWidth + charWidth > width && current.Length > 0) Flush();
                current.Append(c);
                currentWidth += charWidth;
            }
        }

        if (current.Length > 0 || lines.Count == 0) Flush();

        return lines;
    }

    private static IEnumerable<string> Tokenize(string line)
    {
        var start = 0;
        for (var i = 1; i <= line.Length; i++)
        {
            if (i < line.Length && (line[i] == ' ') == (line[start] == ' ')) continue;
            yield return line.Substring(start, i - start);
            start = i;
        }
    }

    /// <summary>Draws one line of text inside a box that starts at x with the given width.</summary>
    /// <param name="ctx">Drawing context for the current page.</param>
    /// <param name="style">Style applied to the text.</param>
    /// <param name="text">Text to draw.</param>
    /// <param name="x">Left edge of the text box.</param>
    /// <param name="width">Width of the text box.</param>
    /// <param name="top">Top edge of the text box.</param>
    /// <param name="lineHeight">Height of the line.</param>
    /// <param name="justify">Whether to justify the line.</param>
    internal static void DrawLine(
        DrawContext ctx,
        TextStyle style,
        string text,
        double x,
        double width,
        double top,
        double lineHeight,
        bool justify)
    {
        if (string.IsNullOrEmpty(text)) return;

        var fonts = ctx.Resources.Fonts;
        var font = fonts.GetFont(style.Font);
        var textWidth = fonts.Measure(style.Font, style.Size, text);
        var baseline = top + (lineHeight / 2) + (style.Size * 0.3);

        if (justify && style.Alignment == ParagraphAlignmentEnum.Justify && text.Contains(' ') && textWidth < width)
        {
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length > 1)
            {
                var wordsWidth = words.Sum(w => fonts.Measure(style.Font, style.Size, w));
                var gap = (width - wordsWidth) / (words.Length - 1);
                var wx = x;
                foreach (var word in words)
                {
                    ctx.Page.AddText(word, style.Size, ctx.Point(wx, baseline), font);
                    wx += fonts.Measure(style.Font, style.Size, word) + gap;
                }

                DrawUnderline(ctx, style, x, baseline, width);
                return;
            }
        }

        var startX = style.Alignment switch
        {
            ParagraphAlignmentEnum.Center => x + ((width - textWidth) / 2),
            ParagraphAlignmentEnum.Right => x + width - textWidth,
            _ => x,
        };

        ctx.Page.AddText(text, style.Size, ctx.Point(startX, baseline), font);
        DrawUnderline(ctx, style, startX, baseline, textWidth);
    }
#pragma warning restore SA1202

    private static void DrawUnderline(DrawContext ctx, TextStyle style, double x, double baseline, double width)
    {
        if (!style.Underline) return;

        var y = baseline + (style.Size * 0.12);
        ctx.Line(x, y, x + width, y, 0.5 + (style.Size / 24));
    }
}

/// <summary>One wrapped line of a body paragraph.</summary>
internal sealed class TextLineItem : LayoutItem
{
    private readonly TextStyle style;
    private readonly string text;
    private readonly double width;
    private readonly double offset;
    private readonly bool justify;

    internal TextLineItem(TextStyle style, string text, double width, double offset, double after, bool justify)
    {
        this.style = style;
        this.text = text;
        this.width = width;
        this.offset = offset;
        this.justify = justify;
        Height = offset + style.LineHeight + after;
    }

    internal override void Draw(DrawContext ctx, double x, double top)
    {
        TextLayout.DrawLine(ctx, style, text, x, width, top + offset, style.LineHeight, justify);
    }
}

internal sealed class ImageItem : LayoutItem
{
    private readonly ImageData image;
    private readonly double contentWidth;
    private readonly double width;
    private readonly ImageAlignmentEnum alignment;

    internal ImageItem(ImageData image, double contentWidth, double maxHeight, ImageAlignmentEnum alignment)
    {
        this.image = image;
        this.contentWidth = contentWidth;
        this.alignment = alignment;

        var scale = 1.0;
        if (image.WidthPt > contentWidth) scale = contentWidth / image.WidthPt;
        if (image.HeightPt * scale > maxHeight) scale = maxHeight / image.HeightPt;

        width = image.WidthPt * scale;
        Height = image.HeightPt * scale;
    }

    internal override void Draw(DrawContext ctx, double x, double top)
    {
        var offset = alignment switch
        {
            ImageAlignmentEnum.Center => (contentWidth - width) / 2,
            ImageAlignmentEnum.Right => contentWidth - width,
            _ => 0,
        };

        ctx.Resources.DrawImage(ctx, image, x + offset, top, width, Height);
    }
}

internal abstract class Cell
{
    internal double Width { get; init; }

    internal double ContentHeight { get; init; }

    internal abstract void Draw(DrawContext ctx, double x, double top);
}

internal sealed class TextCell : Cell
{
    internal const double Padding = 1.2 * 72.0 / 25.4;

    private readonly TextStyle style;
    private readonly List<string> lines;

    internal TextCell(FontProvider fonts, TextStyle style, string text, double width)
    {
        this.style = style;
        Width = width;
        lines = TextLayout.Wrap(fonts, style, text, Math.Max(width - (2 * Padding), 1));
        ContentHeight = style.SpaceBefore + (lines.Count * style.LineHeight) + style.SpaceAfter;
    }

    internal override void Draw(DrawContext ctx, double x, double top)
    {
        var y = top + style.SpaceBefore;
        for (var i = 0; i < lines.Count; i++)
        {
            var last = i == lines.Count - 1;
            TextLayout.DrawLine(ctx, style, lines[i], x + Padding, Width - (2 * Padding), y, style.LineHeight, !last);
            y += style.LineHeight;
        }
    }
}

internal sealed class PageNumberCell : Cell
{
    private readonly TextStyle style;

    internal PageNumberCell(TextStyle style, double width)
    {
        this.style = style;
        Width = width;
        ContentHeight = style.SpaceBefore + style.LineHeight + style.SpaceAfter;
    }

    internal override void Draw(DrawContext ctx, double x, double top)
    {
        TextLayout.DrawLine(
            ctx,
            style,
            $"{ctx.PageNumber} ({ctx.PageCount})",
            x + TextCell.Padding,
            Width - (2 * TextCell.Padding),
            top + style.SpaceBefore,
            style.LineHeight,
            false);
    }
}

internal sealed class ImageCell : Cell
{
    private readonly ImageData image;
    private readonly double width;
    private readonly double height;

    internal ImageCell(ImageData image, double heightPt, double cellWidth)
    {
        this.image = image;
        Width = cellWidth;

        var h = heightPt > 0 ? heightPt : image.HeightPt;
        var w = image.WidthPt * h / image.HeightPt;
        var available = Math.Max(cellWidth - (2 * TextCell.Padding), 1);
        if (w > available)
        {
            h *= available / w;
            w = available;
        }

        width = w;
        height = h;
        ContentHeight = h;
    }

    internal override void Draw(DrawContext ctx, double x, double top)
    {
        ctx.Resources.DrawImage(ctx, image, x + TextCell.Padding, top, width, height);
    }
}

internal sealed class RowItem : LayoutItem
{
    private readonly List<Cell> cells;
    private readonly TableBorderStyle border;
    private readonly double borderWidth;
    private readonly bool isFirst;
    private readonly bool isLast;

    internal RowItem(List<Cell> cells, TableBorderStyle border, double borderWidth, bool isFirst, bool isLast)
    {
        this.cells = cells;
        this.border = border;
        this.borderWidth = borderWidth;
        this.isFirst = isFirst;
        this.isLast = isLast;
        Height = cells.Count == 0 ? 0 : cells.Max(c => c.ContentHeight);
    }

    internal override void Draw(DrawContext ctx, double x, double top)
    {
        var cellX = x;
        foreach (var cell in cells)
        {
            cell.Draw(ctx, cellX, top + ((Height - cell.ContentHeight) / 2));
            if (border == TableBorderStyle.All && borderWidth > 0)
            {
                ctx.Line(cellX, top, cellX + cell.Width, top, borderWidth);
                ctx.Line(cellX, top + Height, cellX + cell.Width, top + Height, borderWidth);
                ctx.Line(cellX, top, cellX, top + Height, borderWidth);
                ctx.Line(cellX + cell.Width, top, cellX + cell.Width, top + Height, borderWidth);
            }

            cellX += cell.Width;
        }

        if (borderWidth <= 0) return;

        var totalWidth = cells.Sum(c => c.Width);
        if (border == TableBorderStyle.Top && isFirst) ctx.Line(x, top, x + totalWidth, top, borderWidth);
        if (border == TableBorderStyle.Bottom && isLast)
            ctx.Line(x, top + Height, x + totalWidth, top + Height, borderWidth);
    }
}

#pragma warning restore SA1402
#pragma warning restore SA1204
