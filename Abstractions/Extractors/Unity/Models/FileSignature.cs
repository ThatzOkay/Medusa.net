namespace Abstractions.Extractors.Unity.Models;

/// <summary>Raw header bytes and detected compression for a file on disk, used for logging/reporting only.</summary>
public class FileSignature(byte[] signature, byte[] version, string compression, long size)
{
    public byte[] Signature { get; } = signature;
    public byte[] Version { get; } = version;
    public string Compression { get; } = compression;
    public long Size { get; } = size;
}