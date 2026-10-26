using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;using Frends.Pdf.MergeDocuments.Definitions;
using Frends.Pdf.MergeDocuments.Helpers;

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

            var args = new List<string> { "--empty", "--pages" };
            foreach (var filePath in input.InputFilePaths)
            {
                args.Add(filePath);
                args.Add("1-z");
            }

            args.Add("--");
            args.Add(input.DestinationFilePath);

            // QPDF repairs damaged files and reports that as a warning, but still writes the output.
            var exitCode = QpdfNative.Run(args);
            if (exitCode != QpdfNative.ExitSuccess && exitCode != QpdfNative.ExitWarnings)
            {
                if (File.Exists(input.DestinationFilePath)) File.Delete(input.DestinationFilePath);
                throw new Exception($"QPDF failed to merge the documents (exit code {exitCode}). See the process error output for details.");
            }

            return new Result { Success = true, Error = null, };
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            return Helpers.ErrorHandler.Handle(e, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}