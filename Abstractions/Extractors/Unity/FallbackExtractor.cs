using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extractors.Unity.Services;

namespace Abstractions.Extractors.Unity;

/// <summary>
/// Catch-all extractor: dumps the full typetree as JSON for any asset type with no dedicated
/// extractor. Also used internally by MeshExtractor/MaterialExtractor/AnimationExtractor/
/// MonoBehaviourExtractor as their own on-failure fallback.
/// </summary>
public sealed class FallbackExtractor : IAssetExtractor
{
    public IReadOnlyCollection<string> HandledTypes { get; } = [];

    public Task<bool> ExtractAsync(ExtractionContext context, CancellationToken cancellationToken = default) =>
        WriteTypeTreeDumpAsync(context, cancellationToken);

    /// <summary>Dumps the object's full typetree to "{output}.json"; if it can't be JSON-serialized
    /// for some reason, falls back further to a plain-text dump ("{output}.txt").</summary>
    public static async Task<bool> WriteTypeTreeDumpAsync(ExtractionContext context, CancellationToken cancellationToken = default)
    {
        var info = ObjectSerializer.ReadTypeTree(context.BaseField);

        try
        {
            await ObjectSerializer.WriteJsonAsync(info, context.OutputPathNoExtension + ".json", cancellationToken);
            return true;
        }
        catch
        {
            await ObjectSerializer.WriteTextAsync(info?.ToString() ?? "null", context.OutputPathNoExtension + ".txt", cancellationToken);
            return true;
        }
    }
}
