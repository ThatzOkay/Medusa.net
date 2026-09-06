using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extractors.Unity.Helpers;
using Abstractions.Extractors.Unity.Services;

namespace Abstractions.Extractors.Unity;

/// <summary>Extracts MonoScript metadata. Note: real Unity MonoScript objects only carry metadata
/// (m_ClassName/m_Namespace/m_AssemblyName/m_ExecutionOrder) - there is no m_Script source-code
/// field on the built-in type, so the "write the .cs source" half of this is a no-op on real data.</summary>
public sealed class MonoScriptExtractor : IAssetExtractor
{
    public IReadOnlyCollection<string> HandledTypes { get; } = ["MonoScript"];

    public async Task<bool> ExtractAsync(ExtractionContext context, CancellationToken cancellationToken = default)
    {
        var baseField = context.BaseField;

        var scriptInfo = new Dictionary<string, object?>
        {
            ["class_name"] = GetStringOr(baseField, "m_ClassName", "Unknown"),
            ["namespace"] = GetStringOr(baseField, "m_Namespace", ""),
            ["assembly_name"] = GetStringOr(baseField, "m_AssemblyName", "Unknown"),
            ["execution_order"] = baseField.Get("m_ExecutionOrder") is { IsDummy: false } order ? order.AsInt : 0,
        };

        var scriptField = baseField.Get("m_Script");
        if (!scriptField.IsDummy)
        {
            var content = scriptField.Value?.ValueType == AssetsTools.NET.AssetValueType.ByteArray
                ? ReflectionHelper.Utf8IgnoreErrors.GetString(scriptField.AsByteArray)
                : scriptField.AsString;

            if (!string.IsNullOrEmpty(content))
            {
                var trimmed = content.TrimStart();
                var isCSharp = trimmed.StartsWith("using ") || trimmed.StartsWith("namespace ") ||
                               trimmed.StartsWith("public class") || trimmed.StartsWith("class ");
                var ext = isCSharp ? ".cs" : ".txt";
                await ObjectSerializer.WriteTextAsync(content, context.OutputPathNoExtension + ext, cancellationToken);
            }
        }

        await ObjectSerializer.WriteJsonAsync(scriptInfo, context.OutputPathNoExtension + "_info.json", cancellationToken);
        return true;
    }

    private static string GetStringOr(AssetsTools.NET.AssetTypeValueField baseField, string name, string fallback) =>
        ReflectionHelper.TryGetNonEmptyString(baseField, name, out var value) ? value : fallback;
}
