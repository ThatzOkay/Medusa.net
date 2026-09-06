using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extractors.Unity.Helpers;
using Abstractions.Extractors.Unity.Services;
using AssetsTools.NET;

namespace Abstractions.Extractors.Unity;

/// <summary>Dumps a MonoBehaviour's full typetree, or falls back to a handful of always-present
/// base fields if that can't be serialized.</summary>
public sealed class MonoBehaviourExtractor : IAssetExtractor
{
    public IReadOnlyCollection<string> HandledTypes { get; } = ["MonoBehaviour"];

    public async Task<bool> ExtractAsync(ExtractionContext context, CancellationToken cancellationToken = default)
    {
        var baseField = context.BaseField;

        try
        {
            var info = ObjectSerializer.ReadTypeTree(baseField);
            await ObjectSerializer.WriteJsonAsync(info, context.OutputPathNoExtension + ".json", cancellationToken);
            return true;
        }
        catch
        {
            TryGetPPtrPathId(baseField, "m_GameObject", out var gameObjectPathId);
            TryGetPPtrPathId(baseField, "m_Script", out var scriptPathId);

            var basicFields = new Dictionary<string, object?>
            {
                ["m_Name"] = baseField.Get("m_Name") is { IsDummy: false } n ? n.AsString : "Unknown",
                ["m_Enabled"] = baseField.Get("m_Enabled") is { IsDummy: false } e ? e.AsBool : true,
                ["m_GameObject_PathID"] = gameObjectPathId,
                ["m_Script_PathID"] = scriptPathId,
            };

            await ObjectSerializer.WriteJsonAsync(basicFields, context.OutputPathNoExtension + "_basic.json", cancellationToken);
            return true;
        }
    }

    private static void TryGetPPtrPathId(AssetTypeValueField baseField, string fieldName, out long pathId)
    {
        pathId = -1;
        var pptr = baseField.Get(fieldName);
        if (!pptr.IsDummy && ReflectionHelper.TryGetPPtr(pptr, out var resolved, out _))
            pathId = resolved;
    }
}
