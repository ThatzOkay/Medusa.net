using System;
using System.Collections.Generic;
using System.IO;

namespace Abstractions.Extractors.IFS;

/// <summary>The simple LZ variant ("AVSLZ") used to compress some IFS texture payloads.</summary>
internal static class Avslz
{
    internal static byte[] Decompress(byte[] input)
    {
        var output = new List<byte>();
        var offset = 0;
        while (offset < input.Length)
        {
            var flag = input[offset++];
            for (var bit = 0; bit < 8; bit++)
            {
                if (((flag >> bit) & 1) != 0)
                {
                    if (offset >= input.Length) throw new InvalidDataException("Truncated AVSLZ literal");
                    output.Add(input[offset++]);
                    continue;
                }
                if (offset + 1 >= input.Length) throw new InvalidDataException("Truncated AVSLZ reference");
                var word = (input[offset] << 8) | input[offset + 1];
                offset += 2;
                var position = word >> 4;
                var length = (word & 15) + 3;
                if (position == 0) return [.. output];
                if (position > output.Count)
                {
                    var zeros = Math.Min(position - output.Count, length);
                    for (var index = 0; index < zeros; index++) output.Add(0);
                    length -= zeros;
                }
                for (var index = 0; index < length; index++) output.Add(output[output.Count - position]);
            }
        }
        throw new InvalidDataException("AVSLZ stream has no terminator");
    }

    /// <summary>Encodes raw bytes as an all-literal AVSLZ stream (no back-references), terminated correctly.</summary>
    internal static byte[] CompressLiterals(byte[] input)
    {
        var completeGroups = input.Length / 8;
        var remainder = input.Length % 8;
        // completeGroups * (1 flag byte + 8 literal bytes) + a terminating flag byte + any trailing literals
        // + a 2-byte zero terminator reference.
        var compressed = new byte[completeGroups * 9 + (remainder != 0 ? remainder + 3 : 3)];
        var source = 0;
        var target = 0;
        for (var group = 0; group < completeGroups; group++)
        {
            compressed[target++] = 0xFF;
            Buffer.BlockCopy(input, source, compressed, target, 8);
            source += 8;
            target += 8;
        }
        compressed[target++] = (byte)(remainder != 0 ? (1 << remainder) - 1 : 0);
        if (remainder == 0) return compressed;
        Buffer.BlockCopy(input, source, compressed, target, remainder);
        target += remainder;
        // Trailing 2 bytes are the zero back-reference terminator (position 0); the array is
        // already zero-initialized, so no explicit write is needed here.
        return compressed;
    }
}
