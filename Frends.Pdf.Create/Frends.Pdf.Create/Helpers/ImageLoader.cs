using System;
using System.IO;

namespace Frends.Pdf.Create.Helpers;

internal static class ImageLoader
{
    private const double PointsPerPixel = 72.0 / 96.0;

    internal static ImageData Load(string path)
    {
        var bytes = File.ReadAllBytes(path);

        if (bytes.Length > 24 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
        {
            var width = ReadInt32BigEndian(bytes, 16);
            var height = ReadInt32BigEndian(bytes, 20);

            return Create(path, TrimPng(bytes), true, width, height);
        }

        if (bytes.Length > 4 && bytes[0] == 0xFF && bytes[1] == 0xD8)
        {
            var (width, height) = ReadJpegSize(bytes);

            return Create(path, bytes, false, width, height);
        }

        throw new Exception($"Unsupported image format: {path}. Only PNG and JPEG images are supported.");
    }

    private static ImageData Create(string path, byte[] bytes, bool isPng, int width, int height)
    {
        if (width <= 0 || height <= 0) throw new Exception($"Could not read the image size: {path}");

        return new ImageData
        {
            Path = path,
            Bytes = bytes,
            IsPng = isPng,
            WidthPt = width * PointsPerPixel,
            HeightPt = height * PointsPerPixel,
        };
    }

    // Some encoders append extra data after the IEND chunk, which the PNG decoder can not handle.
    private static byte[] TrimPng(byte[] bytes)
    {
        var offset = 8;
        while (offset + 12 <= bytes.Length)
        {
            var length = ReadInt32BigEndian(bytes, offset);
            if (length < 0) break;

            var isEnd = bytes[offset + 4] == 'I' && bytes[offset + 5] == 'E' && bytes[offset + 6] == 'N' &&
                        bytes[offset + 7] == 'D';
            offset += 12 + length;
            if (!isEnd) continue;

            return offset < bytes.Length ? bytes[..offset] : bytes;
        }

        return bytes;
    }

    private static int ReadInt32BigEndian(
byte[] bytes, int offset)
    {
        return (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
    }

    private static (int Width, int Height) ReadJpegSize(byte[] bytes)
    {
        var i = 2;
        while (i + 9 < bytes.Length)
        {
            if (bytes[i] != 0xFF)
            {
                i++;
                continue;
            }

            var marker = bytes[i + 1];
            if (marker == 0xFF)
            {
                i++;
                continue;
            }

            // Start-of-frame markers carry the image size.
            if (marker is >= 0xC0 and <= 0xCF && marker != 0xC4 && marker != 0xC8 && marker != 0xCC)
                return ((bytes[i + 7] << 8) | bytes[i + 8], (bytes[i + 5] << 8) | bytes[i + 6]);

            i += 2 + ((bytes[i + 2] << 8) | bytes[i + 3]);
        }

        return (0, 0);
    }
}
