using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extractors.Unity.Helpers;
using Abstractions.Extractors.Unity.Services;

namespace Abstractions.Extractors.Unity;

/// <summary>Extracts a Material's shader reference, keywords, and saved properties (textures/floats/
/// colors) to JSON. Falls back to a raw typetree dump if m_SavedProperties doesn't look as expected.</summary>
public sealed class MaterialExtractor : IAssetExtractor
{
    public IReadOnlyCollection<string> HandledTypes { get; } = ["Material"];

    public async Task<bool> ExtractAsync(ExtractionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseField = context.BaseField;

            context.Loader.TryResolvePPtrName(context.FileInstance, baseField.Get("m_Shader"), out var shaderName);
            var shaderPptr = baseField.Get("m_Shader");
            ReflectionHelper.TryGetPPtr(shaderPptr, out var shaderPathId, out _);

            var materialData = new Dictionary<string, object?>
            {
                ["name"] = baseField.Get("m_Name") is { IsDummy: false } n ? n.AsString : "Unknown",
                ["shader"] = new Dictionary<string, object?>
                {
                    ["name"] = string.IsNullOrEmpty(shaderName) ? "Unknown" : shaderName,
                    ["path_id"] = shaderPathId,
                },
                ["keywords"] = ReflectionHelper.ToPlainObject(baseField.Get("m_ShaderKeywords")),
                ["properties"] = new Dictionary<string, object?>(),
                ["textures"] = new Dictionary<string, object?>(),
            };

            var textures = (Dictionary<string, object?>)materialData["textures"]!;
            var properties = (Dictionary<string, object?>)materialData["properties"]!;

            var savedProps = baseField.Get("m_SavedProperties");
            if (!savedProps.IsDummy)
            {
                foreach (var pair in BundleLoader.ArrayElements(savedProps.Get("m_TexEnvs")))
                {
                    var texName = pair.Get("first").AsString;
                    var texData = pair.Get("second");
                    if (texData.IsDummy) continue;

                    ReflectionHelper.TryGetPPtr(texData.Get("m_Texture"), out var texPathId, out _);
                    var scale = texData.Get("m_Scale");
                    var offset = texData.Get("m_Offset");

                    textures[texName] = new Dictionary<string, object?>
                    {
                        ["texture_path_id"] = texPathId,
                        ["scale"] = new[] { scale.Get("x").AsFloat, scale.Get("y").AsFloat },
                        ["offset"] = new[] { offset.Get("x").AsFloat, offset.Get("y").AsFloat },
                    };
                }

                foreach (var pair in BundleLoader.ArrayElements(savedProps.Get("m_Floats")))
                {
                    properties[pair.Get("first").AsString] = new Dictionary<string, object?>
                    {
                        ["type"] = "float",
                        ["value"] = pair.Get("second").AsFloat,
                    };
                }

                foreach (var pair in BundleLoader.ArrayElements(savedProps.Get("m_Colors")))
                {
                    var color = pair.Get("second");
                    properties[pair.Get("first").AsString] = new Dictionary<string, object?>
                    {
                        ["type"] = "color",
                        ["value"] = new[] { color.Get("r").AsFloat, color.Get("g").AsFloat, color.Get("b").AsFloat, color.Get("a").AsFloat },
                    };
                }
            }

            await ObjectSerializer.WriteJsonAsync(materialData, context.OutputPathNoExtension + ".json", cancellationToken);
            return true;
        }
        catch
        {
            return await FallbackExtractor.WriteTypeTreeDumpAsync(context, cancellationToken);
        }
    }
}
