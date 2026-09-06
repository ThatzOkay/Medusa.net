using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Abstractions.Extractors.IFS;

/// <summary>A node in the IFS "kbin" binary XML tree.</summary>
internal sealed class KNode
{
    public required string Name { get; init; }
    public required int Type { get; init; }
    public Dictionary<string, string> Attrs { get; } = new();
    public double[] Values { get; set; } = [];
    public int? ValueOffset { get; set; }
    public List<KNode> Children { get; } = [];
    public KNode? Parent { get; init; }
}

internal readonly record struct KFormat(int Size, int Count, bool Signed = false, bool Float = false);

/// <summary>Reads the compressed/uncompressed "kbin" binary XML format used throughout IFS archives.</summary>
internal static class KBin
{
    private const string SixBit = "0123456789:ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";

    internal static readonly Dictionary<int, KFormat> Formats = new()
    {
        [1] = new KFormat(0, 0),
        [2] = new KFormat(1, 1, Signed: true),
        [3] = new KFormat(1, 1),
        [4] = new KFormat(2, 1, Signed: true),
        [5] = new KFormat(2, 1),
        [6] = new KFormat(4, 1, Signed: true),
        [7] = new KFormat(4, 1),
        [8] = new KFormat(8, 1, Signed: true),
        [9] = new KFormat(8, 1),
        [10] = new KFormat(1, -1),
        [11] = new KFormat(1, -1),
        [12] = new KFormat(4, 1),
        [13] = new KFormat(4, 1),
        [14] = new KFormat(4, 1, Float: true),
        [15] = new KFormat(8, 1, Float: true),
        [16] = new KFormat(1, 2, Signed: true),
        [17] = new KFormat(1, 2),
        [18] = new KFormat(2, 2, Signed: true),
        [19] = new KFormat(2, 2),
        [20] = new KFormat(4, 2, Signed: true),
        [21] = new KFormat(4, 2),
        [22] = new KFormat(8, 2, Signed: true),
        [23] = new KFormat(8, 2),
        [24] = new KFormat(4, 2, Float: true),
        [25] = new KFormat(8, 2, Float: true),
        [26] = new KFormat(1, 3, Signed: true),
        [27] = new KFormat(1, 3),
        [28] = new KFormat(2, 3, Signed: true),
        [29] = new KFormat(2, 3),
        [30] = new KFormat(4, 3, Signed: true),
        [31] = new KFormat(4, 3),
        [32] = new KFormat(8, 3, Signed: true),
        [33] = new KFormat(8, 3),
        [34] = new KFormat(4, 3, Float: true),
        [35] = new KFormat(8, 3, Float: true),
        [36] = new KFormat(1, 4, Signed: true),
        [37] = new KFormat(1, 4),
        [38] = new KFormat(2, 4, Signed: true),
        [39] = new KFormat(2, 4),
        [40] = new KFormat(4, 4, Signed: true),
        [41] = new KFormat(4, 4),
        [42] = new KFormat(8, 4, Signed: true),
        [43] = new KFormat(8, 4),
        [44] = new KFormat(4, 4, Float: true),
        [45] = new KFormat(8, 4, Float: true),
        [48] = new KFormat(1, 16, Signed: true),
        [49] = new KFormat(1, 16),
        [50] = new KFormat(2, 8, Signed: true),
        [51] = new KFormat(2, 8),
        [52] = new KFormat(1, 1, Signed: true),
        [53] = new KFormat(1, 2, Signed: true),
        [54] = new KFormat(1, 3, Signed: true),
        [55] = new KFormat(1, 4, Signed: true),
        [56] = new KFormat(1, 16, Signed: true),
    };

    private static int Align4(int value) => (value + 3) & ~3;

    internal static int Align16(int value) => (value + 15) & ~15;

    internal static string FixedName(string name)
    {
        var result = name.Replace("_E", ".").Replace("__", "_");
        if (result.Length >= 2 && result[0] == '_' && char.IsAsciiDigit(result[1]))
            result = result[1..];
        return result;
    }

    internal static KNode? FindChild(KNode node, string name) =>
        node.Children.FirstOrDefault(child => FixedName(child.Name) == name);

    internal static IEnumerable<KNode> Walk(KNode node)
    {
        yield return node;
        foreach (var child in node.Children)
            foreach (var descendant in Walk(child))
                yield return descendant;
    }

    internal static KNode Read(byte[] input)
    {
        if (input.Length < 12 || input[0] != 0xA0 || (input[1] != 0x42 && input[1] != 0x45))
            throw new InvalidDataException("Invalid binary XML");
        var compressed = input[1] == 0x42;
        var nodeOffset = 8;
        var nodeEnd = (int)BinaryPrimitives.ReadUInt32BigEndian(input.AsSpan(4, 4)) + 8;
        var dataOffset = nodeEnd + 4;
        var byteOffset = nodeEnd;
        var wordOffset = nodeEnd;
        var root = new KNode { Name = "$root", Type = 1 };
        var current = root;

        string ReadName()
        {
            if (!compressed)
            {
                var length = (input[nodeOffset++] & ~0x40) + 1;
                var value = Encoding.Latin1.GetString(input, nodeOffset, length);
                nodeOffset += length;
                return value;
            }
            int len = input[nodeOffset++];
            var byteLength = (len * 6 + 7) / 8;
            var bits = BigInteger.Zero;
            for (var index = 0; index < byteLength; index++)
                bits = (bits << 8) | input[nodeOffset++];
            var padding = (8 - len * 6 % 8) % 8;
            bits >>= padding;
            var chars = new char[len];
            for (var index = len - 1; index >= 0; index--)
            {
                chars[index] = SixBit[(int)(bits & 63)];
                bits >>= 6;
            }
            return new string(chars);
        }

        (double[] Values, int Offset) ReadValues(KFormat format, int count, bool array)
        {
            int offset;
            if (array || format.Size * count > 2)
            {
                offset = dataOffset;
                var result = new double[count];
                for (var index = 0; index < count; index++)
                    result[index] = ReadNumber(offset + index * format.Size, format);
                dataOffset = Align4(offset + count * format.Size);
                return (result, offset);
            }
            if (format.Size == 1)
            {
                if (byteOffset % 4 == 0) byteOffset = dataOffset;
                offset = byteOffset;
                byteOffset += count;
            }
            else
            {
                if (wordOffset % 4 == 0) wordOffset = dataOffset;
                offset = wordOffset;
                wordOffset += format.Size * count;
            }
            var values = new double[count];
            for (var index = 0; index < count; index++)
                values[index] = ReadNumber(offset + index * format.Size, format);
            var trailing = Math.Max(byteOffset, wordOffset);
            if (dataOffset < trailing) dataOffset = Align4(trailing);
            return (values, offset);
        }

        string ReadString()
        {
            var size = BinaryPrimitives.ReadInt32BigEndian(input.AsSpan(dataOffset, 4));
            var start = dataOffset + 4;
            dataOffset = Align4(start + size);
            var length = Math.Max(0, size - 1);
            return Encoding.UTF8.GetString(input, start, length).TrimEnd('\0');
        }

        while (nodeOffset < nodeEnd)
        {
            while (nodeOffset < nodeEnd && input[nodeOffset] == 0) nodeOffset++;
            if (nodeOffset >= nodeEnd) break;
            int rawType = input[nodeOffset++];
            var array = (rawType & 64) != 0;
            var type = rawType & ~64;
            if (type == 190) { if (current.Parent is not null) current = current.Parent; continue; }
            if (type == 191) break;
            var name = ReadName();
            if (type == 46) { current.Attrs[name] = ReadString(); continue; }
            if (!Formats.TryGetValue(type, out var format))
                throw new InvalidDataException($"Unsupported binary XML node type {type}");
            var node = new KNode { Name = name, Type = type, Parent = current };
            current.Children.Add(node);
            current = node;
            if (type == 1) continue;
            var count = format.Count;
            var isArray = array;
            if (count == -1)
            {
                count = (int)BinaryPrimitives.ReadUInt32BigEndian(input.AsSpan(dataOffset, 4));
                dataOffset += 4;
                isArray = true;
            }
            else if (array)
            {
                var scale = BinaryPrimitives.ReadUInt32BigEndian(input.AsSpan(dataOffset, 4));
                count = (int)(count * (scale / (double)(format.Size * format.Count)));
                dataOffset += 4;
            }
            var decoded = ReadValues(format, count, isArray);
            node.Values = decoded.Values;
            node.ValueOffset = decoded.Offset;
        }
        return root.Children.Count != 1 ? throw new InvalidDataException("Binary XML has no root node") : root.Children[0];

        double ReadNumber(int offset, KFormat format)
        {
            if (format.Float)
                return format.Size == 4
                    ? BinaryPrimitives.ReadSingleBigEndian(input.AsSpan(offset, 4))
                    : BinaryPrimitives.ReadDoubleBigEndian(input.AsSpan(offset, 8));
            return format.Size switch
            {
                1 => format.Signed ? unchecked((sbyte)input[offset]) : input[offset],
                2 => format.Signed
                    ? BinaryPrimitives.ReadInt16BigEndian(input.AsSpan(offset, 2))
                    : BinaryPrimitives.ReadUInt16BigEndian(input.AsSpan(offset, 2)),
                4 => format.Signed
                    ? BinaryPrimitives.ReadInt32BigEndian(input.AsSpan(offset, 4))
                    : BinaryPrimitives.ReadUInt32BigEndian(input.AsSpan(offset, 4)),
                _ => format.Signed
                    ? BinaryPrimitives.ReadInt64BigEndian(input.AsSpan(offset, 8))
                    : BinaryPrimitives.ReadUInt64BigEndian(input.AsSpan(offset, 8))
            };
        }
    }
}
