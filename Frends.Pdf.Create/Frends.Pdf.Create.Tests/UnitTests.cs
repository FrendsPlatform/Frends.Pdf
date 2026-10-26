using System;
using System.IO;
using System.Linq;
using Frends.Pdf.Create.Definitions;
using NUnit.Framework;
using UglyToad.PdfPig;

namespace Frends.Pdf.Create.Tests;

[TestFixture]
public class UnitTests
{
    private static readonly string FileName = "test_output.pdf";

    private static readonly string
        Folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../TestOutput");

    private static readonly string DestinationFullPath = Path.Combine(Folder, FileName);
    private Input input;

    private PageContentElement header;
    private PageContentElement footer;
    private PageContentElement title;
    private PageContentElement paragraphContent;
    private PageContentElement tableContent;

    private Options options;

    [SetUp]
    public void TestSetup()
    {
        if (!Directory.Exists(Folder))
        {
            Directory.CreateDirectory(Folder);
        }

        input = new Input
        {
            Directory = Folder,
            FileName = FileName,
            FileExistsAction = FileExistsActionEnum.Error,
        };

        var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../Files/logo.png");
        header = new PageContentElement
        {
            ContentType = ElementType.Header,
            FontFamily = "Times New Roman",
            FontSize = 8,
            FontStyle = FontStyleEnum.Regular,
            LineSpacingInPt = 11,
            ParagraphAlignment = ParagraphAlignmentEnum.Right,
            SpacingAfterInPt = 0,
            SpacingBeforeInPt = 8,
            ImagePath = logoPath,
            HeaderFooterStyle = HeaderFooterStyleEnum.LogoText,
            BorderWidthInPt = 0.5,
            ImageHeightInCm = 0.5,
        };
        footer = new PageContentElement
        {
            ContentType = ElementType.Footer,
            FontFamily = "Times New Roman",
            FontSize = 8,
            FontStyle = FontStyleEnum.Regular,
            LineSpacingInPt = 11,
            ParagraphAlignment = ParagraphAlignmentEnum.Center,
            SpacingAfterInPt = 0,
            SpacingBeforeInPt = 8,
            HeaderFooterStyle = HeaderFooterStyleEnum.TextPagenum,
            BorderWidthInPt = 0.0,
        };
        title = new PageContentElement
        {
            ContentType = ElementType.Paragraph,
            FontFamily = "Times New Roman",
            FontSize = 16,
            FontStyle = FontStyleEnum.Bold,
            LineSpacingInPt = 11,
            ParagraphAlignment = ParagraphAlignmentEnum.Left,
            SpacingAfterInPt = 0,
            SpacingBeforeInPt = 8,
        };
        paragraphContent = new PageContentElement
        {
            ContentType = ElementType.Paragraph,
            FontFamily = "Times New Roman",
            FontSize = 11,
            FontStyle = FontStyleEnum.Regular,
            LineSpacingInPt = 11,
            ParagraphAlignment = ParagraphAlignmentEnum.Left,
            SpacingAfterInPt = 0,
            SpacingBeforeInPt = 8,
        };

        var tablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../Files/ContentDefinition.json");
        var tableDefinition = File.ReadAllText(tablePath);
        tableContent = new PageContentElement
        {
            ContentType = ElementType.Table,
            Table = tableDefinition,
        };

        options = new Options
        {
            ThrowErrorOnFailure = true,
            CustomFontsLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Files"),
            MarginBottomInCm = 2,
            MarginLeftInCm = 2.5,
            MarginRightInCm = 2.5,
            MarginTopInCm = 5,
            Orientation = PageOrientationEnum.Portrait,
            Size = PageSizeEnum.A4,
        };
    }

    [TearDown]
    public void TestTearDown()
    {
        if (Directory.Exists(Folder))
            Directory.Delete(Folder, true);
    }

    [Test]
    public void CreatePDFTest()
    {
        input.FileExistsAction = FileExistsActionEnum.Overwrite;
        paragraphContent.Text = @"Some text           for testing
with some tab
    one
        two
            three. Then end with scandic letter ö and russian word код.";

        var result = CallCreatePdf(new PageContentElement[]
        {
            header, footer, title, paragraphContent, new PageContentElement
            {
                ContentType = ElementType.PageBreak,
            },
            tableContent,
        });

        Assert.IsTrue(File.Exists(DestinationFullPath));
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void Create_LandscapeOrientationProducesLandscapePdfTest()
    {
        input.FileExistsAction = FileExistsActionEnum.Overwrite;
        options.Orientation = PageOrientationEnum.Landscape;
        paragraphContent.Text = "Landscape orientation test";

        var result = CallCreatePdf([paragraphContent]);

        Assert.IsTrue(result.Success);

        using var pdfDocument = PdfDocument.Open(DestinationFullPath);
        var page = pdfDocument.GetPage(1);

        Assert.Greater(
            page.Width,
            page.Height,
            "Expected a landscape page (wider than tall) when Orientation is set to Landscape.");
    }

    [Test]
    public void Create_RendersNonLatin1CharactersTest()
    {
        input.FileExistsAction = FileExistsActionEnum.Overwrite;
        const string text = "Abc \u043a\u043e\u0434 \u00f6 \u0105\u0119 \u03a9\u03c0 \u00e5\u00e4\u00f6\u00c5\u00c4\u00d6 \u0161\u017e\u0160\u017d \u0401\u0451\u042f\u044b";
        paragraphContent.Text = text;

        var result = CallCreatePdf([paragraphContent]);

        Assert.IsTrue(result.Success);

        using var pdfDocument = PdfDocument.Open(DestinationFullPath);
        var extracted = string.Concat(pdfDocument.GetPage(1).Letters.Select(l => l.Value));
        Assert.AreEqual(text, extracted);
    }

    [Test]
    public void Create_WrapsNonLatin1TextInsideContentAreaTest()
    {
        input.FileExistsAction = FileExistsActionEnum.Overwrite;
        const string word = "\u041f\u0440\u0438\u0432\u0435\u0442";
        paragraphContent.Text = string.Join(" ", Enumerable.Repeat(word, 80));

        var result = CallCreatePdf([paragraphContent]);

        Assert.IsTrue(result.Success);

        using var pdfDocument = PdfDocument.Open(DestinationFullPath);
        var page = pdfDocument.GetPage(1);
        var left = 2.5 / 2.54 * 72;
        var right = page.Width - left;
        var letters = page.Letters.Where(l => l.Value != " ").ToList();

        Assert.AreEqual(80 * word.Length, letters.Count);
        Assert.GreaterOrEqual(letters.Min(l => l.GlyphRectangle.Left), left - 1);
        Assert.LessOrEqual(letters.Max(l => l.GlyphRectangle.Right), right + 1);
        Assert.Greater(letters.Select(l => Math.Round(l.StartBaseLine.Y)).Distinct().Count(), 1);
    }

    [Test]
    public void Create_DoesNotFailIfContentIsEmptyTest()
    {
        paragraphContent.Text = string.Empty;
        var result = CallCreatePdf(new PageContentElement[]
        {
            paragraphContent,
        });

        Assert.IsTrue(File.Exists(DestinationFullPath));
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void Create_ThrowsExceptionIfFileExistsTest()
    {
        options.ThrowErrorOnFailure = true;
        input.FileExistsAction = FileExistsActionEnum.Error;
        var errorMessage = "Output file already exists: " + DestinationFullPath;

        // Run once so file exists.
        CallCreatePdf(new PageContentElement[]
        {
            paragraphContent,
        });

        var result = Assert.Throws<Exception>(() => CallCreatePdf(new PageContentElement[]
        {
            paragraphContent,
        }));
        Assert.AreEqual(errorMessage, result.Message);
    }

    [Test]
    public void Create_OverwriteFileIfItExistsTest()
    {
        options.ThrowErrorOnFailure = true;
        input.FileExistsAction = FileExistsActionEnum.Overwrite;

        // Run once so file exists.
        CallCreatePdf(new PageContentElement[]
        {
            paragraphContent,
        });
        Assert.IsTrue(File.Exists(DestinationFullPath));

        // Run second time to overwrite the previous file.
        CallCreatePdf(new PageContentElement[]
        {
            paragraphContent,
        });
        Assert.IsTrue(File.Exists(DestinationFullPath));
    }

    [Test]
    public void Create_RenamesFilesIfAlreadyExistsTest()
    {
        input.FileExistsAction = FileExistsActionEnum.Rename;

        // Create 3 files.
        var result1 = CallCreatePdf(new PageContentElement[]
        {
            paragraphContent,
        });
        var result2 = CallCreatePdf(new PageContentElement[]
        {
            paragraphContent,
        });
        var result3 = CallCreatePdf(new PageContentElement[]
        {
            paragraphContent,
        });

        Assert.IsTrue(File.Exists(result1.FileName));
        Assert.AreEqual(DestinationFullPath, result1.FileName);
        Assert.IsTrue(File.Exists(result2.FileName));
        Assert.IsTrue(result2.FileName.Contains("_(1)"));
        Assert.IsTrue(File.Exists(result3.FileName));
        Assert.IsTrue(result3.FileName.Contains("_(2)"));
    }

    [Test]
    public void Create_LogoNotFoundTest()
    {
        input.FileExistsAction = FileExistsActionEnum.Overwrite;

        var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Files\no_such_logo.png");
        var header = new PageContentElement
        {
            ContentType = ElementType.Header,
            FontFamily = "Times New Roman",
            FontSize = 8,
            FontStyle = FontStyleEnum.Regular,
            LineSpacingInPt = 11,
            ParagraphAlignment = ParagraphAlignmentEnum.Right,
            SpacingAfterInPt = 0,
            SpacingBeforeInPt = 8,
            ImagePath = logoPath,
            HeaderFooterStyle = HeaderFooterStyleEnum.LogoText,
            BorderWidthInPt = 0.5,
            ImageHeightInCm = 0.5,
        };
        header.Text = @"This is a header";
        var errorMessage = "Path to header graphics was empty or the file does not exist: " + logoPath;

        var result = Assert.Throws<Exception>(() => CallCreatePdf(new PageContentElement[]
        {
            header,
        }));
        Assert.AreEqual(errorMessage, result.Message);
        Assert.IsFalse(File.Exists(DestinationFullPath));
    }

    [Test]
    public void Create_TableWidthTooLargeTest()
    {
        var tooWideTable =
            @"{ ""HasHeaderRow"": true, ""TableType"": ""Table"", ""Columns"": [ { ""Name"": ""Sarake 1"", ""WidthInCm"": 21, ""HeightInCm"": 0, ""Type"": ""Text"" } ], ""RowData"": [] }";
        var table1 = new PageContentElement
        {
            ContentType = ElementType.Table,
            Table = tooWideTable,
        };
        var errorMessage = "Page allows table to be 16 cm wide. Provided table's width is larger than that";
        var result1 = Assert.Throws<Exception>(() => CallCreatePdf(new PageContentElement[]
        {
            table1,
        }));

        // Tables width is calculated during runtime, so exact check of the error message is not possible.
        Assert.IsTrue(result1.Message.Contains(errorMessage));
        Assert.IsFalse(File.Exists(DestinationFullPath));
    }

    [Test]
    public void Create_TableWidthTooLargeTestShouldNotThrowWhenThrowOnErrorFalse()
    {
        var tooWideTable =
            @"{ ""HasHeaderRow"": true, ""TableType"": ""Table"", ""Columns"": [ { ""Name"": ""Sarake 1"", ""WidthInCm"": 21, ""HeightInCm"": 0, ""Type"": ""Text"" } ], ""RowData"": [] }";
        var table1 = new PageContentElement
        {
            ContentType = ElementType.Table,
            Table = tooWideTable,
        };
        var options = new Options
        {
            ThrowErrorOnFailure = false,
            Size = PageSizeEnum.A4,
        };

        Pdf.Create(
            new Input
            {
                Directory = Folder,
                FileName = FileName,
                FileExistsAction = FileExistsActionEnum.Error,
                Content = new DocumentContent
                {
                    Contents = new PageContentElement[]
                    {
                        table1,
                    },
                },
            },
            options,
            default);

        Assert.IsFalse(File.Exists(DestinationFullPath));
    }

    [Test]
    public void CreatePDF_withNullTitleAndHeader()
    {
        paragraphContent.Text = string.Empty;

        var options = new Options
        {
            ThrowErrorOnFailure = true,
            CustomFontsLocation = this.options.CustomFontsLocation,
            Title = null,
            Author = null,
            MarginBottomInCm = 0.5,
            MarginLeftInCm = 0.5,
            MarginRightInCm = 0.5,
            MarginTopInCm = 0.5,
            Orientation = PageOrientationEnum.Portrait,
            Size = PageSizeEnum.A4,
        };

        var input = new Input
        {
            Directory = Folder,
            FileName = FileName,
            FileExistsAction = FileExistsActionEnum.Overwrite,
            Content = new DocumentContent
            {
                Contents = new PageContentElement[]
                {
                    paragraphContent,
                },
            },
        };

        var result = Pdf.Create(input, options, default);

        Assert.IsTrue(File.Exists(DestinationFullPath));
        Assert.IsTrue(result.Success);

        options.Title = string.Empty;
        options.Author = string.Empty;

        result = Pdf.Create(input, options, default);

        Assert.IsTrue(File.Exists(DestinationFullPath));
        Assert.IsTrue(result.Success);

        options.Title = "Title";
        options.Author = "Tester";

        result = Pdf.Create(input, options, default);

        Assert.IsTrue(File.Exists(DestinationFullPath));
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void Create_TaskShouldThrowIfImageNotFound()
    {
        var imagePath = @"c:\file\that\dont\exist\logo.png";
        header = new PageContentElement
        {
            ContentType = ElementType.Image,
            FontFamily = "Times New Roman",
            FontSize = 8,
            FontStyle = FontStyleEnum.Regular,
            LineSpacingInPt = 11,
            ParagraphAlignment = ParagraphAlignmentEnum.Right,
            SpacingAfterInPt = 0,
            SpacingBeforeInPt = 8,
            ImagePath = imagePath,
            HeaderFooterStyle = HeaderFooterStyleEnum.LogoText,
            BorderWidthInPt = 0.5,
            ImageHeightInCm = 0.5,
        };
        var ex = Assert.Throws<Exception>(() => CallCreatePdf(new PageContentElement[]
        {
            header, footer, title, paragraphContent, new PageContentElement
            {
                ContentType = ElementType.PageBreak,
            },
            tableContent,
        }));
        Assert.AreEqual($"Image not found from path: {imagePath}", ex.Message);
    }

    [Test]
    public void Create_LargeImage()
    {
        input.FileExistsAction = FileExistsActionEnum.Overwrite;

        var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../Files/LargeImage.png");
        header = new PageContentElement
        {
            ContentType = ElementType.Image,
            FontFamily = "Times New Roman",
            FontSize = 8,
            FontStyle = FontStyleEnum.Regular,
            LineSpacingInPt = 11,
            ParagraphAlignment = ParagraphAlignmentEnum.Right,
            SpacingAfterInPt = 0,
            SpacingBeforeInPt = 8,
            ImagePath = imagePath,
            HeaderFooterStyle = HeaderFooterStyleEnum.Text,
            BorderWidthInPt = 0.5,
            ImageHeightInCm = 0.5,
        };
        var result =
            CallCreatePdf(
                new PageContentElement[]
                {
                    header, footer, title, paragraphContent, new PageContentElement
                    {
                        ContentType = ElementType.PageBreak,
                    },
                    tableContent,
                });
        Assert.IsTrue(File.Exists(DestinationFullPath));
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void Create_TestHeaderAddStyles()
    {
        var logoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../Files/logo.png");
        input.FileExistsAction = FileExistsActionEnum.Overwrite;

        header = new PageContentElement
        {
            ContentType = ElementType.Header,
            FontFamily = "Times New Roman",
            FontSize = 8,
            FontStyle = FontStyleEnum.Regular,
            LineSpacingInPt = 11,
            ParagraphAlignment = ParagraphAlignmentEnum.Right,
            SpacingAfterInPt = 0,
            SpacingBeforeInPt = 8,
            ImagePath = logoPath,
            HeaderFooterStyle = HeaderFooterStyleEnum.Text,
            BorderWidthInPt = 0.5,
            ImageHeightInCm = 0.5,
        };
        var result =
            CallCreatePdf(
                new PageContentElement[]
                {
                    header, footer, title, paragraphContent, new PageContentElement
                    {
                        ContentType = ElementType.PageBreak,
                    },
                    tableContent,
                });
        Assert.IsTrue(File.Exists(DestinationFullPath));
        Assert.IsTrue(result.Success);

        header = new PageContentElement
        {
            ContentType = ElementType.Header,
            Text = "This is a test",
            FontFamily = "Times New Roman",
            FontSize = 8,
            FontStyle = FontStyleEnum.Regular,
            LineSpacingInPt = 11,
            ParagraphAlignment = ParagraphAlignmentEnum.Right,
            SpacingAfterInPt = 0,
            SpacingBeforeInPt = 8,
            ImagePath = logoPath,
            HeaderFooterStyle = HeaderFooterStyleEnum.Text,
            BorderWidthInPt = 0.5,
            ImageHeightInCm = 0.5,
        };
        result = CallCreatePdf(
            new PageContentElement[]
            {
                header, footer, title, paragraphContent, new PageContentElement
                {
                    ContentType = ElementType.PageBreak,
                },
                tableContent,
            });
        Assert.IsTrue(File.Exists(DestinationFullPath));
        Assert.IsTrue(result.Success);

        header = new PageContentElement
        {
            ContentType = ElementType.Header,
            Text = "This is a test",
            FontFamily = "Times New Roman",
            FontSize = 8,
            FontStyle = FontStyleEnum.Regular,
            LineSpacingInPt = 11,
            ParagraphAlignment = ParagraphAlignmentEnum.Right,
            SpacingAfterInPt = 0,
            SpacingBeforeInPt = 8,
            ImagePath = logoPath,
            HeaderFooterStyle = HeaderFooterStyleEnum.TextPagenum,
            BorderWidthInPt = 0.5,
            ImageHeightInCm = 0.5,
        };
        result = CallCreatePdf(
            new PageContentElement[]
            {
                header, footer, title, paragraphContent, new PageContentElement
                {
                    ContentType = ElementType.PageBreak,
                },
                tableContent,
            });
        Assert.IsTrue(File.Exists(DestinationFullPath));
        Assert.IsTrue(result.Success);

        header = new PageContentElement
        {
            ContentType = ElementType.Header,
            Text = "This is a test",
            FontFamily = "Times New Roman",
            FontSize = 8,
            FontStyle = FontStyleEnum.Regular,
            LineSpacingInPt = 11,
            ParagraphAlignment = ParagraphAlignmentEnum.Right,
            SpacingAfterInPt = 0,
            SpacingBeforeInPt = 8,
            ImagePath = logoPath,
            HeaderFooterStyle = HeaderFooterStyleEnum.LogoText,
            BorderWidthInPt = 0.5,
            ImageHeightInCm = 0.5,
        };
        result = CallCreatePdf(
            new PageContentElement[]
            {
                header, footer, title, paragraphContent, new PageContentElement
                {
                    ContentType = ElementType.PageBreak,
                },
                tableContent,
            });
        Assert.IsTrue(File.Exists(DestinationFullPath));
        Assert.IsTrue(result.Success);

        header = new PageContentElement
        {
            ContentType = ElementType.Header,
            Text = "This is a test",
            FontFamily = "Times New Roman",
            FontSize = 8,
            FontStyle = FontStyleEnum.Regular,
            LineSpacingInPt = 11,
            ParagraphAlignment = ParagraphAlignmentEnum.Right,
            SpacingAfterInPt = 0,
            SpacingBeforeInPt = 8,
            ImagePath = logoPath,
            HeaderFooterStyle = HeaderFooterStyleEnum.LogoTextPagenum,
            BorderWidthInPt = 0.5,
            ImageHeightInCm = 0.5,
        };
        result = CallCreatePdf(
            new PageContentElement[]
            {
                header, footer, title, paragraphContent, new PageContentElement
                {
                    ContentType = ElementType.PageBreak,
                },
                tableContent,
            });
        Assert.IsTrue(File.Exists(DestinationFullPath));
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void UsingDefaultFontWithoutFailures()
    {
        paragraphContent.Text = "some text";
        paragraphContent.FontFamily = "NonExistingFont";

        var result = CallCreatePdf([paragraphContent]);

        Assert.IsTrue(File.Exists(DestinationFullPath));
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void SettingUpDefaultFontWithoutFailures()
    {
        paragraphContent.Text = "some text";
        paragraphContent.FontFamily = "NonExistingFont";
        options.FallbackFontName = "Arial";

        var result = CallCreatePdf([paragraphContent]);

        Assert.IsTrue(File.Exists(DestinationFullPath));
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void SettingUpNonExistingDefaultFontFallbackToBundledFont()
    {
        paragraphContent.Text = "some text";
        paragraphContent.FontFamily = "NonExisting";
        options.FallbackFontName = "OtherNonExisting";

        var result = CallCreatePdf([paragraphContent]);

        Assert.IsTrue(File.Exists(DestinationFullPath));
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void NonExistingCustomFontDirectoryIsSkipped()
    {
        paragraphContent.Text = "some text";
        options.CustomFontsLocation = "InvalidPath";

        var result = CallCreatePdf([paragraphContent]);

        Assert.IsTrue(File.Exists(DestinationFullPath));
        Assert.IsTrue(result.Success);
    }

    private Result CallCreatePdf(PageContentElement[] contents)
    {
        input.Content = new DocumentContent
        {
            Contents = contents,
        };

        return Pdf.Create(input, options, default);
    }
}
