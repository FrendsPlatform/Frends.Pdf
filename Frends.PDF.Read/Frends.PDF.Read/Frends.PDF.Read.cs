using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Frends.PDF.Read.Definitions;
using Frends.PDF.Read.DTOs;
using Frends.PDF.Read.Helpers;
using Newtonsoft.Json;
using UglyToad.PdfPig;

namespace Frends.PDF.Read;

/// <summary>
/// Task Class for PDF operations.
/// </summary>
public static class PDF
{
    /// <summary>
    /// Frends task to read PDF documents
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends-PDF-Read)
    /// </summary>
    /// <param name="input">Essential parameters.</param>
    /// <param name="options">x.</param>
    /// <param name="cancellationToken">A cancellation token provided by Frends Platform.</param>
    /// <returns>object { bool Success, string Json, object Error { string Message, Exception AdditionalInfo } }</returns>
    public static Result Read(
    [PropertyTab] Input input,
    [PropertyTab] Options options,
    CancellationToken cancellationToken)
    {
        try
        {
            using var document = PdfDocument.Open(input.FilePath);

            var result = new PdfReadDocument
            {
                Metadata = new PdfMetadata
                {
                    Title = document.Information.Title,
                    Author = document.Information.Author,
                    PageCount = document.NumberOfPages,
                },
                Pages = new List<PdfReadPage>(),
            };

            foreach (var page in document.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var pageResult = new PdfReadPage
                {
                    Number = page.Number,
                    Text = page.Text,
                    Images = options.IncludeImages ? new List<PdfReadImage>() : null,
                };

                if (options.IncludeImages)
                {
                    foreach (var image in page.GetImages())
                    {
                        byte[] bytes;
                        string format;

                        if (image.TryGetPng(out bytes))
                        {
                            format = "PNG";
                        }
                        else
                        {
                            bytes = image.RawBytes.ToArray();
                            format = "RAW";
                        }

                        pageResult.Images.Add(new PdfReadImage
                        {
                            Format = format,
                            Bytes = bytes,
                            Width = image.Bounds.Width,
                            Height = image.Bounds.Height,
                        });
                    }
                }

                result.Pages.Add(pageResult);
            }

            var json = JsonConvert.SerializeObject(result, Newtonsoft.Json.Formatting.Indented);
            return new Result(true, json);
        }
        catch (Exception ex)
        {
            return ErrorHandler.Handle(ex, options.ThrowErrorOnFailure, options.ErrorMessageOnFailure);
        }
    }
}
