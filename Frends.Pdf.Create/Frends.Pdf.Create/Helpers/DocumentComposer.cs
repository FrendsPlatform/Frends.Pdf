using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Frends.Pdf.Create.Definitions;
using Newtonsoft.Json;
using UglyToad.PdfPig.Writer;

namespace Frends.Pdf.Create.Helpers;

/// <summary>Converts the task content definition into pages of a PdfPig document.</summary>
internal sealed class DocumentComposer
{
    private const double HeaderFooterDistance = 1.25 * Units.PointsPerCm;

    private readonly Options options;
    private readonly PdfDocumentBuilder builder = new();
    private readonly DocumentResources resources;
    private readonly Dictionary<string, ImageData> images = new();
    private readonly double pageWidth;
    private readonly double pageHeight;
    private readonly double left;
    private readonly double top;
    private readonly double contentWidth;
    private readonly double bottom;
    private readonly List<ComposedSection> sections = [];

    private static (double Width, double Height) GetPageSize(PageSizeEnum size)
    {
        return size switch
        {
            PageSizeEnum.A0 => (2383.94, 3370.39),
            PageSizeEnum.A1 => (1683.78, 2383.94),
            PageSizeEnum.A2 => (1190.55, 1683.78),
            PageSizeEnum.A3 => (841.89, 1190.55),
            PageSizeEnum.A4 => (595.275, 841.89),
            PageSizeEnum.A5 => (419.53, 595.275),
            PageSizeEnum.A6 => (297.64, 419.53),
            PageSizeEnum.B5 => (498.9, 708.66),
            PageSizeEnum.Ledger => (792, 1224),
            PageSizeEnum.Legal => (612, 1008),
            _ => (612, 792),
        };
    }

    private static TextStyle ElementStyle(PageContentElement element)
    {
        return TextStyle.FromFontStyle(
            element.FontStyle,
            element.FontFamily,
            element.FontSize,
            element.LineSpacingInPt,
            true,
            element.SpacingBeforeInPt,
            element.SpacingAfterInPt,
            element.ParagraphAlignment);
    }

    private static string Format(double value) =>
        Math.Round(value, 2).ToString(CultureInfo.InvariantCulture);

#pragma warning disable SA1201
    internal DocumentComposer(Options options)
    {
        this.options = options;
        resources = new DocumentResources(new FontProvider(builder));

        var (w, h) = GetPageSize(options.Size);
        if (options.Orientation == PageOrientationEnum.Landscape) (w, h) = (Math.Max(w, h), Math.Min(w, h));
        else (w, h) = (Math.Min(w, h), Math.Max(w, h));

        pageWidth = w;
        pageHeight = h;
        left = Units.CmToPt(options.MarginLeftInCm);
        top = Units.CmToPt(options.MarginTopInCm);
        bottom = Units.CmToPt(options.MarginBottomInCm);
        contentWidth = pageWidth - left - Units.CmToPt(options.MarginRightInCm);

        if (!string.IsNullOrWhiteSpace(options.Title)) builder.DocumentInformation.Title = options.Title;
        if (!string.IsNullOrWhiteSpace(options.Author)) builder.DocumentInformation.Author = options.Author;
    }
#pragma warning restore SA1201

    internal byte[] Compose(DocumentContent content)
    {
        sections.Add(new ComposedSection());

        foreach (var element in content.Contents)
        {
            var section = sections[^1];
            switch (element.ContentType)
            {
                case ElementType.Image:
                    section.Body.Add(CreateImage(element));
                    break;
                case ElementType.PageBreak:
                    sections.Add(new ComposedSection());
                    break;
                case ElementType.Header:
                    section.Header.AddRange(CreateHeaderFooter(element));
                    break;
                case ElementType.Footer:
                    section.Footer.AddRange(CreateHeaderFooter(element));
                    break;
                case ElementType.Table:
                    AddTable(section, element);
                    break;
                default:
                    section.Body.AddRange(CreateParagraph(element));
                    break;
            }
        }

        var pages = Paginate();
        Render(pages);

        return builder.Build();
    }

    private ImageData GetImage(string path)
    {
        if (!images.TryGetValue(path, out var image))
        {
            image = ImageLoader.Load(path);
            images[path] = image;
        }

        return image;
    }

    private ImageItem CreateImage(PageContentElement element)
    {
        if (!File.Exists(element.ImagePath))
            throw new FileNotFoundException("Image not found from path: " + element.ImagePath);

        var maxHeight = Math.Max(pageHeight - top - bottom, 1);

        return new ImageItem(GetImage(element.ImagePath), contentWidth, maxHeight, element.ImageAlignment);
    }

    private List<LayoutItem> CreateParagraph(PageContentElement element)
    {
        if (string.IsNullOrWhiteSpace(element.Text)) return [];

        var style = ElementStyle(element);
        var lines = TextLayout.Wrap(resources.Fonts, style, element.Text, contentWidth);
        var items = new List<LayoutItem>();
        for (var i = 0; i < lines.Count; i++)
        {
            var first = i == 0;
            var last = i == lines.Count - 1;
            items.Add(new TextLineItem(
                style,
                lines[i],
                contentWidth,
                first ? style.SpaceBefore : 0,
                last ? style.SpaceAfter : 0,
                !last));
        }

        return items;
    }

    private IEnumerable<LayoutItem> CreateHeaderFooter(PageContentElement element)
    {
        if (string.IsNullOrWhiteSpace(element.Text)) return [];

        double[] widths;
        bool logo;
        bool pageNumber;
        switch (element.HeaderFooterStyle)
        {
            case HeaderFooterStyleEnum.Text:
                widths = [16];
                logo = false;
                pageNumber = false;
                break;
            case HeaderFooterStyleEnum.TextPagenum:
                widths = [12, 4];
                logo = false;
                pageNumber = true;
                break;
            case HeaderFooterStyleEnum.LogoText:
                widths = [5, 11];
                logo = true;
                pageNumber = false;
                break;
            case HeaderFooterStyleEnum.LogoTextPagenum:
                widths = [5, 7, 4];
                logo = true;
                pageNumber = true;
                break;
            default:
                throw new Exception("Cannot insert header without proper style choice.");
        }

        var style = ElementStyle(element);
        var cells = new List<Cell>();
        var index = 0;

        if (logo)
        {
            if (string.IsNullOrWhiteSpace(element.ImagePath) || !File.Exists(element.ImagePath))
            {
                throw new FileNotFoundException(
                    "Path to header graphics was empty or the file does not exist: " + element.ImagePath);
            }

            cells.Add(new ImageCell(
                GetImage(element.ImagePath),
                Units.CmToPt(element.ImageHeightInCm),
                Units.CmToPt(widths[index++])));
        }

        cells.Add(new TextCell(resources.Fonts, style, element.Text, Units.CmToPt(widths[index++])));

        if (pageNumber)
        {
            var numberStyle = TextStyle.FromFontStyle(
                element.FontStyle,
                element.FontFamily,
                element.FontSize,
                element.LineSpacingInPt,
                true,
                element.SpacingBeforeInPt,
                element.SpacingAfterInPt,
                ParagraphAlignmentEnum.Right);
            cells.Add(new PageNumberCell(numberStyle, Units.CmToPt(widths[index])));
        }

        var isHeader = element.ContentType == ElementType.Header;
        var border = isHeader ? TableBorderStyle.Bottom : TableBorderStyle.Top;

        return [new RowItem(cells, border, element.BorderWidthInPt, true, true)];
    }

    private void AddTable(ComposedSection section, PageContentElement element)
    {
        var tableData = JsonConvert.DeserializeObject<TableDefinition>(element.Table);
        var style = tableData.StyleSettings ?? new TableStyle { FontFamily = "Times New Roman", FontSizeInPt = 9 };

        var tableWidth = 0.0;
        foreach (var column in tableData.Columns)
        {
            tableWidth += Units.CmToPt(column.WidthInCm);
            if (tableWidth > contentWidth)
            {
                throw new Exception(
                    $"Page allows table to be {Format(Units.PtToCm(contentWidth))} cm wide. Provided table's width is larger than that, {Format(Units.PtToCm(tableWidth))} cm.");
            }
        }

        var rows = new List<(List<TableColumnDefinition> Columns, List<string> Data)>();
        if (tableData.HasHeaderRow)
        {
            var headerColumns = tableData.Columns.Select(_ => new TableColumnDefinition { Type = TableColumnType.Text })
                .ToList();
            rows.Add((headerColumns, tableData.Columns.Select(c => c.Name).ToList()));
        }

        foreach (var dataRow in tableData.RowData)
            rows.Add((tableData.Columns, dataRow.Select(r => r.Value).ToList()));

        var items = new List<LayoutItem>();
        for (var r = 0; r < rows.Count; r++)
        {
            var cells = CreateCells(rows[r].Columns, rows[r].Data, tableData.Columns, style);
            items.Add(new RowItem(cells, style.BorderStyle, style.BorderWidthInPt, r == 0, r == rows.Count - 1));
        }

        var target = tableData.TableType switch
        {
            TableTypeEnum.Header => section.Header,
            TableTypeEnum.Footer => section.Footer,
            _ => section.Body,
        };
        target.AddRange(items);
    }

    private List<Cell> CreateCells(
        List<TableColumnDefinition> columns,
        List<string> data,
        List<TableColumnDefinition> widthSource,
        TableStyle style)
    {
        var textStyle = TextStyle.FromFontStyle(
            style.FontStyle,
            style.FontFamily,
            style.FontSizeInPt,
            style.LineSpacingInPt,
            false,
            style.SpacingBeforeInPt,
            style.SpacingAfterInPt,
            ParagraphAlignmentEnum.Left);
        var cells = new List<Cell>();

        for (var i = 0; i < Math.Min(data.Count, columns.Count); i++)
        {
            var width = Units.CmToPt(widthSource[i].WidthInCm);
            switch (columns[i].Type)
            {
                case TableColumnType.Image:
                    if (string.IsNullOrWhiteSpace(data[i]) || !File.Exists(data[i]))
                    {
                        throw new FileNotFoundException(
                            "Path to header graphics was empty or the file does not exist.");
                    }

                    cells.Add(new ImageCell(GetImage(data[i]), Units.CmToPt(columns[i].HeightInCm), width));
                    break;
                case TableColumnType.PageNum:
                    cells.Add(new PageNumberCell(textStyle, width));
                    break;
                default:
                    cells.Add(new TextCell(resources.Fonts, textStyle, data[i], width));
                    break;
            }
        }

        // Keep the column grid intact even when a row has less data than there are columns.
        for (var i = cells.Count; i < widthSource.Count; i++)
            cells.Add(new TextCell(resources.Fonts, textStyle, string.Empty, Units.CmToPt(widthSource[i].WidthInCm)));

        return cells;
    }

    private List<ComposedPage> Paginate()
    {
        var pages = new List<ComposedPage>();
        var header = new List<LayoutItem>();
        var footer = new List<LayoutItem>();

        foreach (var section in sections)
        {
            // A section without its own header or footer inherits the previous one.
            if (section.Header.Count > 0) header = section.Header;
            if (section.Footer.Count > 0) footer = section.Footer;

            var headerHeight = header.Sum(i => i.Height);
            var footerHeight = footer.Sum(i => i.Height);
            var bodyTop = header.Count > 0 ? Math.Max(top, HeaderFooterDistance + headerHeight) : top;
            var bodyBottom = pageHeight -
                             (footer.Count > 0 ? Math.Max(bottom, HeaderFooterDistance + footerHeight) : bottom);

            var page = new ComposedPage(header, footer, footerHeight);
            pages.Add(page);
            var y = bodyTop;
            foreach (var item in section.Body)
            {
                if (y + item.Height > bodyBottom && page.Items.Count > 0)
                {
                    page = new ComposedPage(header, footer, footerHeight);
                    pages.Add(page);
                    y = bodyTop;
                }

                page.Items.Add((item, y));
                y += item.Height;
            }
        }

        return pages;
    }

    private void Render(List<ComposedPage> pages)
    {
        for (var i = 0; i < pages.Count; i++)
        {
            var composed = pages[i];
            var page = builder.AddPage(pageWidth, pageHeight);
            var ctx = new DrawContext(page, pageHeight, i + 1, pages.Count, resources);

            foreach (var (item, y) in composed.Items) item.Draw(ctx, left, y);

            var headerY = HeaderFooterDistance;
            foreach (var item in composed.Header)
            {
                item.Draw(ctx, left, headerY);
                headerY += item.Height;
            }

            var footerY = pageHeight - HeaderFooterDistance - composed.FooterHeight;
            foreach (var item in composed.Footer)
            {
                item.Draw(ctx, left, footerY);
                footerY += item.Height;
            }
        }
    }

    private sealed class ComposedSection
    {
        internal List<LayoutItem> Body { get; } = [];

        internal List<LayoutItem> Header { get; } = [];

        internal List<LayoutItem> Footer { get; } = [];
    }

    private sealed class ComposedPage
    {
        internal ComposedPage(List<LayoutItem> header, List<LayoutItem> footer, double footerHeight)
        {
            Header = header;
            Footer = footer;
            FooterHeight = footerHeight;
        }

        internal List<(LayoutItem Item, double Y)> Items { get; } = [];

        internal List<LayoutItem> Header { get; }

        internal List<LayoutItem> Footer { get; }

        internal double FooterHeight { get; }
    }
}
