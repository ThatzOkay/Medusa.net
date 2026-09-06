using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extractors.Unity.Helpers;
using Abstractions.Extractors.Unity.Services;

namespace Abstractions.Extractors.Unity;

/// <summary>Extracts a TextAsset's raw content as .csv/.json/.txt, detected from its shape. This is
/// where CSV/json/plain-text game data (e.g. the *.bytes master tables) comes out.</summary>
public sealed class TextAssetExtractor : IAssetExtractor
{
    public IReadOnlyCollection<string> HandledTypes { get; } = ["TextAsset"];

    public async Task<bool> ExtractAsync(ExtractionContext context, CancellationToken cancellationToken = default)
    {
        var scriptField = context.BaseField.Get("m_Script");
        var bytes = scriptField.IsDummy ? [] : scriptField.AsByteArray;
        var content = ReflectionHelper.Utf8IgnoreErrors.GetString(bytes);

        var trimmed = content.Trim();
        var isJson = (trimmed.StartsWith('{') || trimmed.StartsWith('[')) &&
                     (trimmed.EndsWith('}') || trimmed.EndsWith(']'));

        var isCsv = !isJson && LooksLikeCsv(trimmed);

        var ext = isJson ? ".json" : isCsv ? ".csv" : ".txt";

        await ObjectSerializer.WriteTextAsync(content, context.OutputPathNoExtension + ext, cancellationToken);

        var info = new Dictionary<string, object?>
        {
            ["size"] = content.Length,
            ["type"] = isJson ? "json" : isCsv ? "csv" : "text",
            ["encoding"] = "utf-8",
        };
        await ObjectSerializer.WriteJsonAsync(info, context.OutputPathNoExtension + "_info.json", cancellationToken);

        return true;
    }

    /// <summary>Header row and first data row have the same (non-zero) comma count. Simple, but
    /// avoids false-positiving on ordinary prose that happens to contain a comma somewhere.</summary>
    private static bool LooksLikeCsv(string trimmed)
    {
        var lines = trimmed.ReplaceLineEndings("\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return false;

        var headerCommas = lines[0].Count(c => c == ',');
        return headerCommas > 0 && headerCommas == lines[1].Count(c => c == ',');
    }
}
