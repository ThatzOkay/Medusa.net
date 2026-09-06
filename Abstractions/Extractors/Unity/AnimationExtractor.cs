using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extractors.Unity.Services;

namespace Abstractions.Extractors.Unity;

/// <summary>Extracts an AnimationClip's float curves (attribute/path/keyframes) to JSON. Keyframe
/// tangents are read from AnimationCurve.Keyframe's real field names, inSlope/outSlope.</summary>
public sealed class AnimationExtractor : IAssetExtractor
{
    public IReadOnlyCollection<string> HandledTypes { get; } = ["AnimationClip"];

    public async Task<bool> ExtractAsync(ExtractionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseField = context.BaseField;

            var animData = new Dictionary<string, object?>
            {
                ["name"] = baseField.Get("m_Name") is { IsDummy: false } n ? n.AsString : "Unknown",
                ["length"] = baseField.Get("m_Length") is { IsDummy: false } len ? len.AsFloat : 0f,
                ["frame_rate"] = baseField.Get("m_SampleRate") is { IsDummy: false } rate ? rate.AsFloat : 30f,
                ["wrap_mode"] = baseField.Get("m_WrapMode") is { IsDummy: false } wrap ? wrap.AsInt : 0,
                ["curves"] = new List<object?>(),
            };

            var curves = (List<object?>)animData["curves"]!;
            foreach (var curve in BundleLoader.ArrayElements(baseField.Get("m_FloatCurves")))
            {
                var curveInfo = new Dictionary<string, object?>
                {
                    ["attribute"] = curve.Get("attribute") is { IsDummy: false } attr ? attr.AsString : "Unknown",
                    ["path"] = curve.Get("path") is { IsDummy: false } path ? path.AsString : "",
                    ["type"] = "float",
                    ["keyframes"] = new List<object?>(),
                };

                var keyframes = (List<object?>)curveInfo["keyframes"]!;
                foreach (var keyframe in BundleLoader.ArrayElements(curve.Get("curve").Get("m_Curve")))
                {
                    keyframes.Add(new Dictionary<string, object?>
                    {
                        ["time"] = keyframe.Get("time") is { IsDummy: false } t ? t.AsFloat : 0f,
                        ["value"] = keyframe.Get("value") is { IsDummy: false } v ? v.AsFloat : 0f,
                        ["in_tangent"] = keyframe.Get("inSlope") is { IsDummy: false } inS ? inS.AsFloat : 0f,
                        ["out_tangent"] = keyframe.Get("outSlope") is { IsDummy: false } outS ? outS.AsFloat : 0f,
                    });
                }

                curves.Add(curveInfo);
            }

            await ObjectSerializer.WriteJsonAsync(animData, context.OutputPathNoExtension + ".json", cancellationToken);
            return true;
        }
        catch
        {
            return await FallbackExtractor.WriteTypeTreeDumpAsync(context, cancellationToken);
        }
    }
}
