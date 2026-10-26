using System;
using System.IO;
using System.Threading;
using Frends.Pdf.MergeDocuments.Definitions;
using Frends.Pdf.MergeDocuments.Helpers;
using NUnit.Framework;

namespace Frends.Pdf.MergeDocuments.Tests;

[TestFixture]
public class UnitTests
{
    private static readonly string TestDataDir = Path.Combine(AppContext.BaseDirectory, "TestData");
    private static readonly string ResultDir = Path.Combine(TestDataDir, "result");
    private static readonly string ExpectedOutputPath = Path.Combine(TestDataDir, "expectedOutput.pdf");

    private static Input DefaultInput => new()
    {
        InputFilePaths =
        [
            Path.Combine(TestDataDir, "first.pdf"),
            Path.Combine(TestDataDir, "second.pdf"),
            Path.Combine(TestDataDir, "third.pdf")
        ],
        DestinationFilePath = Path.Combine(ResultDir, "output.pdf"),
    };

    private static Options DefaultOptions => new()
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
        var jsonPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            QpdfNative.Run(["--json", "--json-key=pages", path, jsonPath]);
            return System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(jsonPath), "\"pageposfrom1\"").Count;
        }
        finally
        {
            File.Delete(jsonPath);
        }
    }
}