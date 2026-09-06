namespace Abstractions.Extractors.Unity.Models;

/// <summary>Mirrors extract_streaming_info() - where an object's payload is streamed from on disk (e.g. resS files).</summary>
public record StreamingInfo
{
    public long Offset { get; set; }
    public long Size { get; set; }
    public string Path { get; set; } = "";

    public bool HasData => Size > 0;
}
