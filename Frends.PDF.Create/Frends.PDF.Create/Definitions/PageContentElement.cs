using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.PDF.Create.Definitions;

/// <summary>
/// Class for single content element.
/// </summary>
public class PageContentElement
{
    /// <summary>
    /// Specifies the type of content for this element (e.g., Paragraph, Image, Header).
    /// </summary>
    /// <example>Paragraph</example>
    [DefaultValue(ElementType.Paragraph)]
    public ElementType ContentType { get; set; }

    /// <summary>
    /// Full path to image.
    /// </summary>
    /// <example>F:\imagedir\myimage.jpg</example>
    [UIHint(nameof(ContentType), "", ElementType.Image, ElementType.Header, ElementType.Footer)]
    public string ImagePath { get; set; }

    /// <summary>
    /// Text content for Paragraph, Header, or Footer elements.
    /// </summary>
    /// <example>This is an example text.</example>
    [UIHint(nameof(ContentType), "", ElementType.Paragraph, ElementType.Header, ElementType.Footer)]
    [DisplayFormat(DataFormatString = "Text")]
    public string Text { get; set; }

    /// <summary>
    /// Font family for the text. Applies to Paragraph, Header, and Footer elements.
    /// </summary>
    /// <example>Times New Roman</example>
    [UIHint(nameof(ContentType), "", ElementType.Paragraph, ElementType.Header, ElementType.Footer)]
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("Times New Roman")]
    public string FontFamily { get; set; }

    /// <summary>
    /// Font size in points for the text content.
    /// </summary>
    /// <example>11</example>
    [UIHint(nameof(ContentType), "", ElementType.Paragraph, ElementType.Header, ElementType.Footer)]
    [DefaultValue(11)]
    public int FontSize { get; set; }

    /// <summary>
    /// Font style, such as Regular, Bold, or Italic.
    /// </summary>
    /// <example>Regular</example>
    [UIHint(nameof(ContentType), "", ElementType.Paragraph, ElementType.Header, ElementType.Footer)]
    [DefaultValue(FontStyleEnum.Regular)]
    public FontStyleEnum FontStyle { get; set; }

    /// <summary>
    /// Space between lines in points.
    /// </summary>
    /// <example>14</example>
    [UIHint(nameof(ContentType), "", ElementType.Paragraph, ElementType.Header, ElementType.Footer)]
    [DefaultValue(14)]
    public int LineSpacingInPt { get; set; }


    /// <summary>
    /// Specifies the alignment of the text (e.g., Left, Center, Right).
    /// </summary>
    /// <example>Left</example>
    [UIHint(nameof(ContentType), "", ElementType.Paragraph, ElementType.Header, ElementType.Footer)]
    [DefaultValue(ParagraphAlignmentEnum.Left)]
    [DisplayName("Alignment")]
    public ParagraphAlignmentEnum ParagraphAlignment { get; set; }


    /// <summary>
    /// Image alignment.
    /// </summary>
    /// <example>Left</example>
    [UIHint(nameof(ContentType), "", ElementType.Image)]
    [DefaultValue(ImageAlignmentEnum.Left)]
    [DisplayName("Alignment")]
    public ImageAlignmentEnum ImageAlignment { get; set; }

    /// <summary>
    /// Amount of space added above this element in points.
    /// </summary>
    /// <example>8</example>
    [UIHint(nameof(ContentType), "", ElementType.Image, ElementType.Paragraph, ElementType.Header, ElementType.Footer)]
    [DefaultValue(8)]
    public int SpacingBeforeInPt { get; set; }

    /// <summary>
    /// Amount of space added after this element in points.
    /// </summary>
    /// <example>0</example>
    [UIHint(nameof(ContentType), "", ElementType.Image, ElementType.Paragraph, ElementType.Header, ElementType.Footer)]
    [DefaultValue(0)]
    public int SpacingAfterInPt { get; set; }

    /// <summary>
    /// Header or footer type: only text, or additional graphics and/or pagenumbers.
    /// </summary>
    /// <example>Text</example>
    [UIHint(nameof(ContentType), "", ElementType.Header, ElementType.Footer)]
    [DefaultValue(HeaderFooterStyleEnum.Text)]
    public HeaderFooterStyleEnum HeaderFooterStyle { get; set; }

    /// <summary>
    /// Width of header's lower (or footer's upper) border line in points.
    /// </summary>
    /// <example>0.0</example>
    [UIHint(nameof(ContentType), "", ElementType.Header, ElementType.Footer)]
    [DefaultValue("0.0")]
    public double BorderWidthInPt { get; set; }

    /// <summary>
    /// Height of the header/footer graphics in centimeters.
    /// Image's aspect ratio is preserved when scaling.
    /// </summary>
    /// <example>2.5</example>
    [UIHint(nameof(ContentType), "", ElementType.Header, ElementType.Footer)]
    [DefaultValue(2.5)]
    public double ImageHeightInCm { get; set; }

    /// <summary>
    /// Table data in JSON. For detailed example of this JSON,
    /// please refer to the README documentation.
    /// </summary>
    /// <example>{"HasHeaderRow":true,"TableType":"Table","StyleSettings":{"FontFamily":"Times New Roman","FontSizeInPt":9.0,"FontStyle":"Regular","BorderWidthInPt":0.5,"BorderStyle":"All"},"Columns":[{"Name":"Name","WidthInCm":6},{"Name":"Age","WidthInCm":3}],"RowData":[{"Name":"Alice","Age":"30"}]}</example>
    [UIHint(nameof(ContentType), "", ElementType.Table)]
    [DisplayFormat(DataFormatString = "Json")]
    [DefaultValue("{}")]
    public string Table { get; set; }
}
