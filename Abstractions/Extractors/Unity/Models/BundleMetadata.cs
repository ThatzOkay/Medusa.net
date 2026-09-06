using System;
using System.Collections.Generic;

namespace Abstractions.Extractors.Unity.Models;

/// <summary>An entry from the bundle's container map (the AssetBundle object's m_Container field): a logical
/// asset path (e.g. "assets/foo/bar.asset") mapped to the object it resolves to.</summary>
public record ContainerEntry(long PathId, string Type);

/// <summary>Summary of a loaded bundle, written to bundle_metadata.json.</summary>
public class BundleMetadata
{
    public string Version { get; set; } = "Unknown";
    public string Platform { get; set; } = "Unknown";
    public int ObjectCount { get; set; }
    public Dictionary<string, ContainerEntry> ContainerPaths { get; set; } = new();
    public DateTimeOffset ExtractionTimestamp { get; set; } = DateTimeOffset.Now;
}
