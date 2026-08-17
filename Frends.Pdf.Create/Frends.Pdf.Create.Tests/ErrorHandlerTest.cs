using System;
using System.IO;
using System.Threading;
using Frends.Pdf.Create.Definitions;
using NUnit.Framework;

namespace Frends.Pdf.Create.Tests;

[TestFixture]
internal class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    private static readonly string _folder =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"../../../TestOutput");

    private Input DefaultInput()
    {
        return new Input
        {
            OutputFile = new FileProperties
            {
                Directory = "/nonexistent/path/that/does/not/exist",
                FileName = "test.pdf",
                FileExistsAction = FileExistsActionEnum.Error
            },
            DocumentSettings = new DocumentSettings
            {
                MarginBottomInCm = 2,
                MarginLeftInCm = 2.5,
                MarginRightInCm = 2.5,
                MarginTopInCm = 2,
                Orientation = PageOrientationEnum.Portrait,
                Size = PageSizeEnum.A4
            },
            Content = new DocumentContent
            {
                Contents = new[]
                {
                    new PageContentElement
                    {
                        ContentType = ElementType.Paragraph,
                        FontFamily = "Times New Roman",
                        FontSize = 11,
                        FontStyle = FontStyleEnum.Regular,
                        LineSpacingInPt = 11,
                        ParagraphAlignment = ParagraphAlignmentEnum.Left,
                        SpacingAfterInPt = 0,
                        SpacingBeforeInPt = 8,
                        Text = "Hello"
                    }
                }
            }
        };
    }

    private Options DefaultOptions()
    {
        return new Options
        {
            ThrowErrorOnFailure = true,
            ErrorMessageOnFailure = string.Empty,
            CustomFontsLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../Files")
        };
    }

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.Throws<Exception>(() =>
            Pdf.Create(DefaultInput(), DefaultOptions(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result = Pdf.Create(DefaultInput(), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;
        var ex = Assert.Throws<Exception>(() =>
            Pdf.Create(DefaultInput(), options, CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
    }

    [Test]
    public void Should_Return_Failed_Result_With_Custom_Message_When_ThrowErrorOnFailure_Is_False()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        options.ErrorMessageOnFailure = CustomErrorMessage;
        var result = Pdf.Create(DefaultInput(), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
        Assert.That(result.Error.Message, Contains.Substring(CustomErrorMessage));
    }
}
