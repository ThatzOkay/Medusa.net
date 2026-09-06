using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extractors.Unity.Models;

namespace Abstractions.Extractors.Unity.Services;

/// <summary>
/// Sniffs a file's header for reporting purposes (bundle_metadata.json / extraction_log.txt).
/// Actual bundle/assets-file decompression is handled internally by AssetsTools.NET's AssetsManager
/// (see BundleLoader) - this class only identifies what's there, it doesn't decompress anything.
/// </summary>
public static class CompressionDetector
{
    public static string DetectCompressionType(ReadOnlySpan<byte> data)
    {
        switch (data.Length)
        {
            case < 2:
                return "unknown";
            case >= 4 when data[..4].SequenceEqual("LZ4\0"u8):
                return "lz4";
        }

        if (data.Length >= 8 && data[..8].SequenceEqual("UnityFS\0"u8))
            return "unityfs";

        if (data.Length >= 8 && data[..8].SequenceEqual("UnityRaw"u8))
            return "raw";

        return (data[0], data[1]) switch
        {
            (0x78, 0x9C) => "zlib",
            (0x78, 0x01) => "zlib",
            (0x78, 0xDA) => "zlib",
            (0x1F, 0x8B) => "gzip",
            _ => "unknown"
        };
    }

    public static async Task<FileSignature> GetFileSignatureAsync(string path, CancellationToken cancellationToken = default)
    {
        try
        {
            await using var stream = File.OpenRead(path);
            var header = new byte[32];
            var read = await stream.ReadAsync(header.AsMemory(0, 32), cancellationToken);
            if (read <= 0)
                return new FileSignature([], [], "unknown", 0);

            var fileInfo = new FileInfo(path);
            return new FileSignature(
                header[..Math.Min(8, read)],
                read > 8 ? header[8..Math.Min(12, read)] : [],
                DetectCompressionType(header.AsSpan(0, read)),
                fileInfo.Length);
        }
        catch
        {
            return new FileSignature([], [], "unknown", 0);
        }
    }
}
