using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Frends.Pdf.MergeDocuments.Definitions;
using NUnit.Framework;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Actions;
using UglyToad.PdfPig.Annotations;
using UglyToad.PdfPig.Tokens;

namespace Frends.Pdf.MergeDocuments.Tests;

[TestFixture]
public class UnitTests
{
    private static readonly string TestDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");
    private static readonly string ResultDir = Path.Combine(TestDataDir, "result");
    private static readonly string ExpectedOutputPath = Path.Combine(TestDataDir, "expectedOutput.pdf");

    private static Input DefaultInput => new Input()
    {
        InputFilePaths =
        [
            Path.Combine(TestDataDir, "first.pdf"),
            Path.Combine(TestDataDir, "second.pdf"),
            Path.Combine(TestDataDir, "third.pdf")
        ],
        DestinationFilePath = Path.Combine(ResultDir, "output.pdf"),
    };

    private static Options DefaultOptions => new Options()
    {
        ThrowErrorOnFailure = true,
        ErrorMessageOnFailure = string.Empty,
    };

    [SetUp]
    public void ClearResults()
    {
        if (Path.Exists(ResultDir))
        {
            if (File.Exists(ResultDir)) File.Delete(ResultDir);
            else
                Directory.Delete(ResultDir, recursive: true);
        }
    }

    [Test]
    public void ShouldMergeFiles()
    {
        var input = DefaultInput;
        var result = Pdf.MergeDocuments(input, DefaultOptions, CancellationToken.None);
        Assert.That(FilesHaveSamePageCount(ExpectedOutputPath, input.DestinationFilePath), Is.True);
        Assert.That(result.Success, Is.True);
    }

    [Test]
    public void ShouldPreserveRemoteLinksAndSignatureWidgets()
    {
        Directory.CreateDirectory(ResultDir);
        var firstInput = Path.Combine(ResultDir, "first-with-annotations.pdf");
        var secondInput = Path.Combine(ResultDir, "second-with-annotations.pdf");
        var output = Path.Combine(ResultDir, "merged-with-annotations.pdf");
        File.WriteAllBytes(firstInput, BuildPdfWithRemoteLinkAndSignatureWidget());
        File.WriteAllBytes(secondInput, BuildPdfWithRemoteLinkAndSignatureWidget("Signature2"));
        int remoteDestinationPageNumber;
        using (var sourceDocument = PdfDocument.Open(firstInput))
        {
            remoteDestinationPageNumber = sourceDocument.GetPages()
                .SelectMany(page => page.GetAnnotations())
                .Select(annotation => annotation.Action)
                .OfType<GoToRAction>()
                .Single()
                .Destination.PageNumber;
        }

        var input = new Input
        {
            InputFilePaths = new[] { firstInput, secondInput },
            DestinationFilePath = output,
        };
        var result = Pdf.MergeDocuments(input, DefaultOptions, CancellationToken.None);

        Assert.That(result.Success, Is.True);
        using var document = PdfDocument.Open(output);
        var annotations = document.GetPages().SelectMany(page => page.GetAnnotations()).ToList();
        var signatureWidgets = annotations.Where(annotation => annotation.Type == AnnotationType.Widget).ToList();
        var signatureNames = signatureWidgets
            .Select(annotation => annotation.AnnotationDictionary
                .Get<StringToken>(NameToken.T, document.Structure.TokenScanner).Data)
            .ToList();
        var actions = annotations.Select(annotation => annotation.Action).ToList();
        var remoteLinks = actions.OfType<GoToRAction>().ToList();
        var localLinks = actions.OfType<GoToAction>().ToList();

        Assert.That(signatureNames, Has.Count.EqualTo(2));
        Assert.That(signatureNames[0], Is.Not.EqualTo(signatureNames[1]));
        Assert.That(signatureWidgets.All(widget => widget.HasNormalAppearance), Is.True);
        Assert.That(remoteLinks, Has.Count.EqualTo(2));
        Assert.That(remoteLinks.Select(link => link.Filename), Is.All.EqualTo("external.pdf"));
        Assert.That(remoteLinks.Select(link => link.Destination.PageNumber), Is.All.EqualTo(remoteDestinationPageNumber));
        Assert.That(remoteLinks.All(link => link.Destination.PageNumber > document.NumberOfPages), Is.True);
        Assert.That(localLinks.Select(link => link.Destination.PageNumber), Is.EqualTo(new[] { 1, 2 }));
    }

    [Test]
    public void ShouldFailIfAnyInputFilesDoesNotExist()
    {
        var input = DefaultInput;
        input.InputFilePaths[0] = Path.Combine(TestDataDir, "nonexistent.pdf");
        Assert.Throws<Exception>(() => Pdf.MergeDocuments(input, DefaultOptions, CancellationToken.None));
    }

    [Test]
    public void ShouldFailIfAnyInputFilesIsNotPdf()
    {
        var input = DefaultInput;
        input.InputFilePaths[0] = Path.Combine(TestDataDir, "invalid.txt");
        Assert.Throws<Exception>(() => Pdf.MergeDocuments(input, DefaultOptions, CancellationToken.None));
    }

    [Test]
    public void ShouldFailIfOutputPathAlreadyOccupied()
    {
        Pdf.MergeDocuments(DefaultInput, DefaultOptions, CancellationToken.None);
        Assert.Throws<Exception>(() => Pdf.MergeDocuments(DefaultInput, DefaultOptions, CancellationToken.None));
    }

    [Test]
    public void ShouldFailIfOutputPathIsInvalid()
    {
        var input = DefaultInput;
        input.DestinationFilePath = Path.Combine(TestDataDir, "result");
        Assert.Throws<Exception>(() => Pdf.MergeDocuments(input, DefaultOptions, CancellationToken.None));
    }

    [Test]
    public void ReturnFailedResultIfThrowErrorFlagIsDisabled()
    {
        var input = DefaultInput;
        input.InputFilePaths[0] = Path.Combine(TestDataDir, "invalid.txt");
        var options = DefaultOptions;
        options.ThrowErrorOnFailure = false;
        var result = Pdf.MergeDocuments(input, options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void DefaultErrorMessageOnFailureIsUsed()
    {
        var input = DefaultInput;
        input.InputFilePaths[0] = Path.Combine(TestDataDir, "invalid.txt");
        var options = DefaultOptions;
        options.ErrorMessageOnFailure = "Test message";
        var ex = Assert.Throws<Exception>(() => Pdf.MergeDocuments(input, options, CancellationToken.None));
        Assert.That(ex!.Message, Is.EqualTo("Test message"));
    }

    private static bool FilesHaveSamePageCount(string path1, string path2)
    {
        return PageCount(path1) == PageCount(path2);
    }

    private static int PageCount(string path)
    {
        using var document = PdfDocument.Open(path);
        return document.NumberOfPages;
    }

    private static byte[] BuildPdfWithRemoteLinkAndSignatureWidget(string signatureName = "Signature1")
    {
        return BuildRawPdf(
            "<< /Type /Catalog /Pages 2 0 R /AcroForm << /Fields [4 0 R] /SigFlags 3 >> >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 /MediaBox [0 0 612 792] /Resources << >> >>",
            "<< /Type /Page /Parent 2 0 R /Annots [4 0 R 5 0 R 7 0 R] >>",
            $"<< /Type /Annot /Subtype /Widget /FT /Sig /T ({signatureName}) /Rect [10 10 50 30] /F 4 /AP << /N 6 0 R >> /V << /Type /Sig /Filter /Adobe.PPKLite /SubFilter /adbe.pkcs7.detached /ByteRange [0 0 0 0] /Contents <00> >> >>",
            "<< /Type /Annot /Subtype /Link /Rect [60 10 100 30] /Border [0 0 0] /A << /S /GoToR /F (external.pdf) /D [42 /Fit] >> >>",
            "<< /Type /XObject /Subtype /Form /BBox [0 0 40 20] /Resources << >> /Length 19 >>\nstream\nq 0 0 40 20 re S Q\nendstream",
            "<< /Type /Annot /Subtype /Link /Rect [110 10 150 30] /Border [0 0 0] /A << /S /GoTo /D [3 0 R /Fit] >> >>");
    }

    private static byte[] BuildRawPdf(params string[] objects)
    {
        var pdf = new StringBuilder("%PDF-1.7\n");
        var offsets = new List<int>();
        foreach (var obj in objects)
        {
            offsets.Add(Encoding.Latin1.GetByteCount(pdf.ToString()));
            pdf.Append($"{offsets.Count} 0 obj\n{obj}\nendobj\n");
        }

        var xrefOffset = Encoding.Latin1.GetByteCount(pdf.ToString());
        pdf.Append($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
        foreach (var offset in offsets) pdf.Append($"{offset:D10} 00000 n \n");
        pdf.Append($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xrefOffset}\n%%EOF");

        return Encoding.Latin1.GetBytes(pdf.ToString());
    }
}
