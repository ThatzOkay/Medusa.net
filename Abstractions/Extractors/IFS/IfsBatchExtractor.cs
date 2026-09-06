using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Abstractions.Extractors.IFS;

/// <summary>Progress reported after each archive finishes during a batch extract.</summary>
public sealed record BatchExtractProgress(int Processed, int TotalFiles, int ItemsExtracted, int Skipped, int FailedCount);

/// <summary>One archive that failed to extract during a batch run.</summary>
public sealed record BatchExtractFailure(string File, string Error);

/// <summary>Outcome of a batch extract across every archive under a source directory.</summary>
public sealed record BatchExtractResult(int TotalFiles, int TotalItemsExtracted, int Skipped, IReadOnlyList<BatchExtractFailure> Failures)
{
    public int Succeeded => TotalFiles - Failures.Count - Skipped;
}

/// <summary>
/// Reusable, logic for extracting every .ifs archive under a directory tree.
/// </summary>
public static class IfsBatchExtractor
{
    public static string[] FindIfsFiles(string sourceRootFull) =>
    [
        .. Directory.EnumerateFiles(sourceRootFull, "*", SearchOption.AllDirectories)
            .Where(file => file.EndsWith(".ifs", StringComparison.OrdinalIgnoreCase))
    ];

    public static BatchExtractResult ExtractFiles(
        string sourceRootFull,
        IReadOnlyList<string> files,
        string outputDirectory,
        string? filter,
        bool includeTextures,
        bool includeRaw,
        bool skipExisting,
        Action<BatchExtractProgress>? onProgress = null)
    {
        var processed = 0;
        var totalItems = 0;
        var skipped = 0;
        var failures = new ConcurrentBag<BatchExtractFailure>();

        Parallel.ForEach(files, file =>
        {
            try
            {
                // Mirror the archive's own directory under the output root, so e.g. sd/popn1/anime.ifs
                // and tex/6/rose_6a.ifs land in separate places even if two archives share a base name.
                var relativeDirectory = Path.GetDirectoryName(Path.GetRelativePath(sourceRootFull, file));
                var fileOutputRoot = string.IsNullOrEmpty(relativeDirectory)
                    ? outputDirectory
                    : Path.Combine(outputDirectory, relativeDirectory);

                if (skipExisting && Directory.Exists(IfsFile.GetOutputDirectory(file, fileOutputRoot)))
                {
                    Interlocked.Increment(ref skipped);
                    return;
                }

                var outputs = IfsFile.ExtractAll(file, fileOutputRoot, filter, includeTextures, includeRaw);
                Interlocked.Add(ref totalItems, outputs.Count);
            }
            catch (Exception ex)
            {
                failures.Add(new BatchExtractFailure(file, ex.Message));
            }
            finally
            {
                var done = Interlocked.Increment(ref processed);
                onProgress?.Invoke(new BatchExtractProgress(done, files.Count, totalItems, skipped, failures.Count));
            }
        });

        return new BatchExtractResult(
            files.Count,
            totalItems,
            skipped,
            [.. failures.OrderBy(failure => failure.File, StringComparer.Ordinal)]);
    }
}
