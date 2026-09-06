using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Abstractions.Extractors.IFS;

/// <summary>Minimal PNG encode/decode for 8-bit RGBA, non-interlaced images (matches what the IFS format stores).</summary>
internal static class Png
{
    private static readonly byte[] Signature = [137, 80, 78, 71, 13, 10, 26, 10];
    private static readonly uint[] CrcTable = BuildCrcTable();

    private static uint[] BuildCrcTable()
    {
        var table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            var c = n;
            for (var k = 0; k < 8; k++)
                c = (c & 1) != 0 ? 0xEDB88320 ^ (c >> 1) : c >> 1;
            table[n] = c;
        }
        return table;
    }

    private static uint Crc32Update(uint crc, ReadOnlySpan<byte> data)
    {
        foreach (var b in data)
            crc = CrcTable[(crc ^ b) & 0xFF] ^ (crc >> 8);
        return crc;
    }

    private static byte[] Chunk(string name, byte[] data)
    {
        var type = Encoding.ASCII.GetBytes(name);
        using var stream = new MemoryStream(8 + data.Length + 4);
        Span<byte> lengthBuffer = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(lengthBuffer, (uint)data.Length);
        stream.Write(lengthBuffer);
        stream.Write(type);
        stream.Write(data);
        var crc = Crc32Update(Crc32Update(0xFFFFFFFF, type), data) ^ 0xFFFFFFFF;
        Span<byte> crcBuffer = stackalloc byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(crcBuffer, crc);
        stream.Write(crcBuffer);
        return stream.ToArray();
    }

    private static byte[] ZlibDeflate(byte[] data)
    {
        using var output = new MemoryStream();
        using (var zlib = new ZLibStream(output, CompressionLevel.Optimal, leaveOpen: true))
            zlib.Write(data);
        return output.ToArray();
    }

    private static byte[] ZlibInflate(byte[] data, int expectedLength)
    {
        using var input = new MemoryStream(data);
        using var zlib = new ZLibStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream(expectedLength);
        zlib.CopyTo(output);
        return output.ToArray();
    }

    internal static byte[] Encode(int width, int height, byte[] rgba)
    {
        var stride = width * 4;
        var scanLines = new byte[height * (stride + 1)];
        for (var y = 0; y < height; y++)
            Buffer.BlockCopy(rgba, y * stride, scanLines, y * (stride + 1) + 1, stride);

        var ihdr = new byte[13];
        BinaryPrimitives.WriteUInt32BigEndian(ihdr.AsSpan(0, 4), (uint)width);
        BinaryPrimitives.WriteUInt32BigEndian(ihdr.AsSpan(4, 4), (uint)height);
        ihdr[8] = 8; // bit depth
        ihdr[9] = 6; // color type: RGBA

        using var output = new MemoryStream();
        output.Write(Signature);
        output.Write(Chunk("IHDR", ihdr));
        output.Write(Chunk("IDAT", ZlibDeflate(scanLines)));
        output.Write(Chunk("IEND", []));
        return output.ToArray();
    }

    internal static (int Width, int Height, byte[] Rgba) Decode(byte[] input, int expectedWidth, int expectedHeight)
    {
        if (input.Length < 33 || !input.AsSpan(0, 8).SequenceEqual(Signature))
            throw new InvalidDataException("The cropped image is not a valid PNG.");

        int width = 0, height = 0;
        var ended = false;
        using var idatStream = new MemoryStream();
        var offset = 8;
        while (offset + 12 <= input.Length)
        {
            var size = (int)BinaryPrimitives.ReadUInt32BigEndian(input.AsSpan(offset, 4));
            var type = Encoding.ASCII.GetString(input, offset + 4, 4);
            var dataStart = offset + 8;
            var dataEnd = dataStart + size;
            if (dataEnd + 4 > input.Length) throw new InvalidDataException("The cropped PNG is truncated.");
            var chunkData = input.AsSpan(dataStart, size);
            var expectedCrc = BinaryPrimitives.ReadUInt32BigEndian(input.AsSpan(dataEnd, 4));
            var crc = Crc32Update(Crc32Update(0xFFFFFFFF, Encoding.ASCII.GetBytes(type)), chunkData) ^ 0xFFFFFFFF;
            if (crc != expectedCrc) throw new InvalidDataException("The cropped PNG checksum is invalid.");

            switch (type)
            {
                case "IHDR" when size != 13 || width != 0 || height != 0:
                    throw new InvalidDataException("The cropped PNG header is invalid.");
                case "IHDR":
                {
                    width = (int)BinaryPrimitives.ReadUInt32BigEndian(chunkData[..4]);
                    height = (int)BinaryPrimitives.ReadUInt32BigEndian(chunkData.Slice(4, 4));
                    if (width != expectedWidth || height != expectedHeight)
                        throw new InvalidDataException("The cropped image has an invalid pixel size.");
                    if (chunkData[8] != 8 || chunkData[9] != 6 || chunkData[10] != 0 || chunkData[11] != 0 || chunkData[12] != 0)
                        throw new InvalidDataException("The cropped PNG must use 8-bit RGBA pixels without interlacing.");
                    break;
                }
                case "IDAT" when width == 0 || height == 0:
                    throw new InvalidDataException("The cropped PNG chunk order is invalid.");
                case "IDAT":
                    idatStream.Write(chunkData);
                    break;
                case "IEND" when size != 0 || dataEnd + 4 != input.Length:
                    throw new InvalidDataException("The cropped PNG ending is invalid.");
                case "IEND":
                    ended = true;
                    break;
            }
            offset = dataEnd + 4;
            if (type == "IEND") break;
        }
        if (!ended || width <= 0 || height <= 0 || idatStream.Length == 0)
            throw new InvalidDataException("The cropped PNG has no image data.");

        var stride = width * 4;
        var expectedSize = height * (stride + 1);
        var filtered = ZlibInflate(idatStream.ToArray(), expectedSize);
        if (filtered.Length != expectedSize) throw new InvalidDataException("The cropped PNG pixel data is invalid.");

        var rgba = new byte[width * height * 4];
        for (var y = 0; y < height; y++)
        {
            var filter = filtered[y * (stride + 1)];
            var source = y * (stride + 1) + 1;
            var target = y * stride;
            for (var x = 0; x < stride; x++)
            {
                var value = filtered[source + x];
                var left = x >= 4 ? rgba[target + x - 4] : (byte)0;
                var above = y > 0 ? rgba[target + x - stride] : (byte)0;
                var upperLeft = y > 0 && x >= 4 ? rgba[target + x - stride - 4] : (byte)0;
                rgba[target + x] = filter switch
                {
                    0 => value,
                    1 => (byte)(value + left),
                    2 => (byte)(value + above),
                    3 => (byte)(value + (left + above) / 2),
                    4 => (byte)(value + Paeth(left, above, upperLeft)),
                    _ => throw new InvalidDataException($"Unsupported PNG row filter {filter}."),
                };
            }
        }
        return (width, height, rgba);
    }

    private static byte Paeth(byte a, byte b, byte c)
    {
        var estimate = a + b - c;
        int da = Math.Abs(estimate - a), db = Math.Abs(estimate - b), dc = Math.Abs(estimate - c);
        return da <= db && da <= dc ? a : db <= dc ? b : c;
    }
}
