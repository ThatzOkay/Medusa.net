using System;
using System.Collections.Generic;

namespace Abstractions.Extractors.Unity.Models;

/// <summary>One failed/degraded object extraction, recorded for extraction_log.txt.</summary>
public record ExtractionError
{
    public long ObjectId { get; set; }
    public string Type { get; set; } = "";
    public string Error { get; set; } = "";
    public string Traceback { get; set; } = "";
}

/// <summary>Summary of a bundle's file signature, as reported in the log (signature/compression/size only -
/// this is just what was sniffed from the header, not the result of a live decompression).</summary>
public record LoggedBundleInfo(string Signature, string Compression, long Size);

/// <summary>Accumulated results for one bundle extraction run, written out as extraction_log.txt.</summary>
public class ExtractionLog
{
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.Now;
    public string BundlePath { get; set; } = "";
    public LoggedBundleInfo BundleInfo { get; set; } = new("", "unknown", 0);
    public int TotalObjectsProcessed { get; set; }
    public int SuccessfulExtractions { get; set; }
    public int FailedExtractions { get; set; }
    public List<ExtractionError> Errors { get; } = [];
    public Dictionary<string, int> ExtractedCounts { get; } = new();
}
