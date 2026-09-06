using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extractors.Unity.Services;

namespace Abstractions.Extractors.Unity;

/// <summary>Extracts an embedded font's raw TTF data, or a small info dump if none is embedded.</summary>
public sealed class FontExtractor : IAssetExtractor
{
    public IReadOnlyCollection<string> HandledTypes { get; } = ["Font"];

    public async Task<bool> ExtractAsync(ExtractionContext context, CancellationToken cancellationToken = default)
    {
        var baseField = context.BaseField;
        var fontDataField = baseField.Get("m_FontData");

        if (!fontDataField.IsDummy && fontDataField.AsByteArray is { Length: > 0 } fontData)
        {
            await File.WriteAllBytesAsync(context.OutputPathNoExtension + ".ttf", fontData, cancellationToken);
            return true;
        }

        var fontInfo = new Dictionary<string, object?>
        {
            ["name"] = baseField.Get("m_Name") is { IsDummy: false } name ? name.AsString : "Unknown",
            ["size"] = baseField.Get("m_FontSize") is { IsDummy: false } size ? size.AsInt : 0,
            ["style"] = baseField.Get("m_FontStyle") is { IsDummy: false } style ? style.AsInt : 0,
        };
        await ObjectSerializer.WriteJsonAsync(fontInfo, context.OutputPathNoExtension + "_info.json", cancellationToken);
        return true;
    }
}
