using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extractors.Unity.Helpers;
using AssetsTools.NET;

namespace Abstractions.Extractors.Unity.Services;

/// <summary>
/// JSON output for whatever ReflectionHelper.ToPlainObject() produces, plus the typetree-dump entry
/// point that most extractors fall back to.
/// </summary>
public static class ObjectSerializer
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <summary>Reads an object's full field tree into a plain object graph (Dictionary/List/primitives).</summary>
    public static object? ReadTypeTree(AssetTypeValueField baseField) => ReflectionHelper.ToPlainObject(baseField);

    public static async Task WriteJsonAsync(object? data, string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, data, Options, cancellationToken);
    }

    public static async Task WriteTextAsync(string content, string path, CancellationToken cancellationToken = default) =>
        await File.WriteAllTextAsync(path, content, cancellationToken);
}
