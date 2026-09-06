using System;
using System.Buffers.Binary;
using System.Collections.Generic;

namespace Abstractions.Extractors.IFS;

/// <summary>S3TC block decompression (DXT1 / DXT5) for IFS textures.</summary>
internal static class Dxt
{
    private static (byte R, byte G, byte B) Color565(int value) => (
        (byte)Math.Round((value >> 11 & 31) * 255.0 / 31),
        (byte)Math.Round((value >> 5 & 63) * 255.0 / 63),
        (byte)Math.Round((value & 31) * 255.0 / 31));

    private static (byte R, byte G, byte B, byte A)[] DecodeColors(ReadOnlySpan<byte> data, int offset, bool forceFour)
    {
        int first = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset, 2));
        int second = BinaryPrimitives.ReadUInt16LittleEndian(data.Slice(offset + 2, 2));
        var a = Color565(first);
        var b = Color565(second);
        var colors = new (byte R, byte G, byte B, byte A)[4];
        colors[0] = (a.R, a.G, a.B, 255);
        colors[1] = (b.R, b.G, b.B, 255);
        if (first > second || forceFour)
        {
            colors[2] = ((byte)Math.Round((2 * a.R + b.R) / 3.0), (byte)Math.Round((2 * a.G + b.G) / 3.0), (byte)Math.Round((2 * a.B + b.B) / 3.0), 255);
            colors[3] = ((byte)Math.Round((a.R + 2 * b.R) / 3.0), (byte)Math.Round((a.G + 2 * b.G) / 3.0), (byte)Math.Round((a.B + 2 * b.B) / 3.0), 255);
        }
        else
        {
            colors[2] = ((byte)Math.Round((a.R + b.R) / 2.0), (byte)Math.Round((a.G + b.G) / 2.0), (byte)Math.Round((a.B + b.B) / 2.0), 255);
            colors[3] = (0, 0, 0, 0);
        }
        return colors;
    }

    internal static byte[] Decode(byte[] raw, int width, int height, bool dxt5)
    {
        var data = (byte[])raw.Clone();
        for (var index = 0; index + 1 < data.Length; index += 2)
            (data[index], data[index + 1]) = (data[index + 1], data[index]);

        var rgba = new byte[width * height * 4];
        var blockSize = dxt5 ? 16 : 8;
        var offset = 0;
        for (var by = 0; by < height; by += 4)
        {
            for (var bx = 0; bx < width; bx += 4)
            {
                if (offset + blockSize > data.Length) return rgba;
                var alpha = new int[16];
                Array.Fill(alpha, 255);
                var colorOffset = offset;
                if (dxt5)
                {
                    byte a0 = data[offset], a1 = data[offset + 1];
                    var palette = new List<int> { a0, a1 };
                    if (a0 > a1)
                        for (var i = 1; i <= 6; i++) palette.Add((int)Math.Round(((7 - i) * a0 + i * a1) / 7.0));
                    else
                    {
                        for (var i = 1; i <= 4; i++) palette.Add((int)Math.Round(((5 - i) * a0 + i * a1) / 5.0));
                        palette.Add(0);
                        palette.Add(255);
                    }
                    ulong bits = 0;
                    for (var i = 0; i < 6; i++) bits |= (ulong)data[offset + 2 + i] << (i * 8);
                    for (var i = 0; i < 16; i++) alpha[i] = palette[(int)(bits >> (i * 3) & 7)];
                    colorOffset += 8;
                }
                var colors = DecodeColors(data, colorOffset, dxt5);
                var indices = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(colorOffset + 4, 4));
                for (var py = 0; py < 4; py++)
                {
                    for (var px = 0; px < 4; px++)
                    {
                        int x = bx + px, y = by + py;
                        if (x >= width || y >= height) continue;
                        var pixel = py * 4 + px;
                        var color = colors[(int)(indices >> (pixel * 2) & 3)];
                        var target = (y * width + x) * 4;
                        rgba[target] = color.R;
                        rgba[target + 1] = color.G;
                        rgba[target + 2] = color.B;
                        rgba[target + 3] = dxt5 ? (byte)alpha[pixel] : color.A;
                    }
                }
                offset += blockSize;
            }
        }
        return rgba;
    }
}
