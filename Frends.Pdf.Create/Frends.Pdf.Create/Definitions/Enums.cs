namespace Frends.Pdf.Create.Definitions;

/// <summary>
/// Actions if the output document already exists.
/// </summary>
public enum FileExistsActionEnum
{
    /// <summary>
    /// Throw error.
    /// </summary>
    Error,

    /// <summary>
    /// Overwrite the existing document.
    /// </summary>
    Overwrite,

    /// <summary>
    /// Rename the new document.
    /// </summary>
    Rename,
}

#pragma warning disable CS1591 // Self-explanatory enums.
/// <summary>
/// Page size options.
/// </summary>
public enum PageSizeEnum
{
    /// <summary>A0 paper size.</summary>
    A0,

    /// <summary>A1 paper size.</summary>
    A1,

    /// <summary>A2 paper size.</summary>
    A2,

    /// <summary>A3 paper size.</summary>
    A3,

    /// <summary>A4 paper size.</summary>
    A4,

    /// <summary>A5 paper size.</summary>
    A5,

    /// <summary>A6 paper size.</summary>
    A6,

    /// <summary>B5 paper size.</summary>
    B5,

    /// <summary>Ledger paper size.</summary>
    Ledger,

    /// <summary>Legal paper size.</summary>
    Legal,

    /// <summary>Letter paper size.</summary>
    Letter,
}
#pragma warning restore CS1591

#pragma warning disable CS1591 // Self-explanatory enums.
/// <summary>
/// Options for page orientation.
/// </summary>
public enum PageOrientationEnum
{
    /// <summary>Portrait orientation.</summary>
    Portrait,

    /// <summary>Landscape orientation.</summary>
    Landscape,
}
#pragma warning restore CS1591

#pragma warning disable CS1591 // Self-explanatory enums.
/// <summary>
/// Element which will be added to the document.
/// </summary>
public enum ElementType
{
    /// <summary>Paragraph content.</summary>
    Paragraph,

    /// <summary>Image content.</summary>
    Image,

    /// <summary>Page break.</summary>
    PageBreak,

    /// <summary>Header content.</summary>
    Header,

    /// <summary>Footer content.</summary>
    Footer,

    /// <summary>Table content.</summary>
    Table,
}
#pragma warning restore CS1591

#pragma warning disable CS1591 // Self-explanatory enums.
/// <summary>
/// Alignment of paragraph.
/// </summary>
public enum ParagraphAlignmentEnum
{
    /// <summary>Align text to the left.</summary>
    Left,

    /// <summary>Center text.</summary>
    Center,

    /// <summary>Justify text.</summary>
    Justify,

    /// <summary>Align text to the right.</summary>
    Right,
}
#pragma warning restore CS1591

#pragma warning disable CS1591 // Self-explanatory enums.
/// <summary>
/// Alignment for image.
/// </summary>
public enum ImageAlignmentEnum
{
    /// <summary>Align the image to the left.</summary>
    Left,

    /// <summary>Center the image.</summary>
    Center,

    /// <summary>Align the image to the right.</summary>
    Right,
}
#pragma warning restore CS1591

#pragma warning disable CS1591 // Self-explanatory enums.
/// <summary>
/// Options for paragraph font.
/// </summary>
public enum FontStyleEnum
{
    /// <summary>Regular font style.</summary>
    Regular,

    /// <summary>Bold font style.</summary>
    Bold,

    /// <summary>Italic font style.</summary>
    Italic,

    /// <summary>Bold and italic font style.</summary>
    BoldItalic,

    /// <summary>Underlined font style.</summary>
    Underline,
}
#pragma warning restore CS1591

/// <summary>
/// Options for header/footer input.
/// </summary>
public enum HeaderFooterStyleEnum
{
    /// <summary>
    /// Add only text.
    /// </summary>
    Text,

    /// <summary>
    /// Add text and page numbers.
    /// </summary>
    TextPagenum,

    /// <summary>
    /// Add text and image.
    /// </summary>
    LogoText,

    /// <summary>
    /// Add text, image and page numbers.
    /// </summary>
    LogoTextPagenum,
}

/// <summary>
/// Options for table type.
/// </summary>
public enum TableTypeEnum
{
    /// <summary>
    /// Normal table in the document.
    /// </summary>
    Table,

    /// <summary>
    /// Add table to header.
    /// </summary>
    Header,

    /// <summary>
    /// Add table to footer.
    /// </summary>
    Footer,
}

/// <summary>
/// Type of data which will be added to the table columns.
/// </summary>
public enum TableColumnType
{
    /// <summary>
    /// Text data.
    /// </summary>
    Text,

    /// <summary>
    /// Images.
    /// </summary>
    Image,

    /// <summary>
    /// Page numbers.
    /// </summary>
    PageNum,
}

#pragma warning disable CS1591 // Self-explanatory enums.
/// <summary>
/// Table style selector for changing style to different borders.
/// </summary>
public enum TableBorderStyle
{
    /// <summary>No table border.</summary>
    None,

    /// <summary>Border along the top of the table.</summary>
    Top,

    /// <summary>Border along the bottom of the table.</summary>
    Bottom,

    /// <summary>Border around all sides of the table.</summary>
    All,
}
#pragma warning restore CS1591
