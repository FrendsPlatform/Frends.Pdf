using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Frends.Pdf.MergeDocuments.Definitions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Actions;
using UglyToad.PdfPig.Outline.Destinations;
using UglyToad.PdfPig.Writer;

namespace Frends.Pdf.MergeDocuments;

/// <summary>
/// Task class.
/// </summary>
public static class Pdf
{
    /// <summary>
    /// Task to merge multiple Pdf documents into one.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-Pdf-MergeDocuments)
    /// </summary>
    /// <param name="input">Information about input and output paths</param>
    /// <param name="options">Exception settings</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, object Error { string Message, exception AdditionalInfo } }</returns>
    public static Result MergeDocuments(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            if (File.Exists(input.DestinationFilePath)) throw new Exception("Destination file already exists");
            if (Path.GetExtension(input.DestinationFilePath) != ".pdf")
                throw new Exception("Destination file must have .pdf extension");

            foreach (var filePath in input.InputFilePaths)
            {
                if (!File.Exists(filePath)) throw new FileNotFoundException("Input file not found", filePath);
            }

            var dir = Path.GetDirectoryName(input.DestinationFilePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            using var outputDocument = new PdfDocumentBuilder();
            var basePageNumber = 0;
            foreach (var filePath in input.InputFilePaths)
            {
                using var inputDocument = PdfDocument.Open(filePath);
                for (var pageNumber = 1; pageNumber <= inputDocument.NumberOfPages; pageNumber++)
                {
                    outputDocument.AddPage(
                        inputDocument,
                        pageNumber,
                        new PdfDocumentBuilder.AddPageOptions
                        {
                            CopyLinkFunc = action => CopyLink(action, basePageNumber),
                        });
                }

                basePageNumber += inputDocument.NumberOfPages;
            }

            File.WriteAllBytes(input.DestinationFilePath, outputDocument.Build());

            return new Result { Success = true, Error = null, };
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return Helpers.ErrorHandler.Handle(e, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }

    private static PdfAction CopyLink(PdfAction action, int basePageNumber)
    {
        if (action is GoToRAction) return action;
        if (action is not GoToAction goToAction) return action;

        var destination = goToAction.Destination;
        var mergedDestination = new ExplicitDestination(
            basePageNumber + destination.PageNumber,
            destination.Type,
            destination.Coordinates);

        return new GoToAction(mergedDestination);
    }
}
