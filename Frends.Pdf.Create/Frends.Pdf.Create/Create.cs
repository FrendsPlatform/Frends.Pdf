using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using Frends.Pdf.Create.Definitions;
using Frends.Pdf.Create.Helpers;

namespace Frends.Pdf.Create;

/// <summary>
/// Task class.
/// </summary>
public static class Pdf
{
    /// <summary>
    /// Create PDF document from given content.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.PDF.Create)
    /// </summary>
    /// <param name="input">Input parameters: output file properties, document settings, and content.</param>
    /// <param name="options">Additional task options.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>Object { bool Success, string FileName, Error Error }</returns>
    public static Result Create(
        [PropertyTab] Input input,
        [PropertyTab] Options options,
        CancellationToken cancellationToken)
    {
        try
        {
            FileFontResolver.Setup(options.FallbackFontName, options.CustomFontsLocation);

            var bytes = new DocumentComposer(options).Compose(input.Content);
            var fileName = DetermineFileName(input);
            File.WriteAllBytes(fileName, bytes);

            return new Result
            {
                Success = true,
                FileName = fileName,
            };
        }
        catch (Exception ex)
        {
            return ex.Handle(options);
        }
    }

    private static string DetermineFileName(Input input)
    {
        var fileName = Path.Combine(input.Directory, input.FileName);
        var fileNameIndex = 1;

        if (File.Exists(fileName) && input.FileExistsAction == FileExistsActionEnum.Error)
            throw new Exception("Output file already exists: " + fileName);

        while (File.Exists(fileName) && input.FileExistsAction != FileExistsActionEnum.Overwrite)
        {
            switch (input.FileExistsAction)
            {
                case FileExistsActionEnum.Error:
                    throw new Exception($"File {fileName} already exists.");
                case FileExistsActionEnum.Rename:
                    fileName = Path.Combine(
                        input.Directory,
                        $"{Path.GetFileNameWithoutExtension(input.FileName)}_({fileNameIndex}){Path.GetExtension(input.FileName)}");

                    break;
            }

            fileNameIndex++;
        }

        return fileName;
    }
}
