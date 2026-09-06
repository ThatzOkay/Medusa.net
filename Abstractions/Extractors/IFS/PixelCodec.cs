using System;
using System.Buffers.Binary;

namespace Abstractions.Extractors.IFS;

/// <summary>Decodes raw IFS texture payloads into RGBA, and re-encodes RGBA back into the writable format.</summary>
internal static class PixelCodec
{
    internal static byte[] Decode(string? format, byte[] data, int width, int height)
    {
        var pixels = width * height;
        switch (format)
        {
            case "argb8888rev":
            {
                var rgba = new byte[pixels * 4];
                for (var index = 0; index < pixels; index++)
                {
                    var source = index * 4;
                    rgba[source] = source + 2 < data.Length ? data[source + 2] : (byte)0;
                    rgba[source + 1] = source + 1 < data.Length ? data[source + 1] : (byte)0;
                    rgba[source + 2] = source < data.Length ? data[source] : (byte)0;
                    rgba[source + 3] = source + 3 < data.Length ? data[source + 3] : (byte)0;
                }
                return rgba;
            }
            case "argb4444":
            {
                var rgba = new byte[pixels * 4];
                for (var index = 0; index < pixels; index++)
                {
                    var wordIndex = index * 2;
                    var word = wordIndex + 1 < data.Length
                        ? BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(wordIndex, 2))
                        : (ushort)0;
                    var target = index * 4;
                    rgba[target] = (byte)((word & 15) * 17);
                    rgba[target + 1] = (byte)((word >> 8 & 15) * 17);
                    rgba[target + 2] = (byte)((word >> 12 & 15) * 17);
                    rgba[target + 3] = (byte)((word >> 4 & 15) * 17);
                }
                return rgba;
            }
            case "dxt1": return Dxt.Decode(data, width, height, dxt5: false);
            case "dxt5": return Dxt.Decode(data, width, height, dxt5: true);
            default: throw new NotSupportedException($"Unsupported IFS texture format {format ?? "undefined"}");
        }
    }

    internal static byte[] EncodeArgb8888Rev(byte[] rgba)
    {
        var output = new byte[rgba.Length];
        for (var offset = 0; offset < rgba.Length; offset += 4)
        {
            output[offset] = rgba[offset + 2];
            output[offset + 1] = rgba[offset + 1];
            output[offset + 2] = rgba[offset];
            output[offset + 3] = rgba[offset + 3];
        }
        return output;
    }
}
