using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extractors.Unity.Services;

namespace Abstractions.Extractors.Unity;

/// <summary>
/// Extracts AudioClip metadata plus whatever raw compressed bytes Unity stored (m_AudioData, or the
/// streamed resource m_StreamData points at if m_AudioData is empty). Decoding the compressed audio
/// to PCM (FMOD/vorbis) isn't implemented - that would need a decoder of its own.
/// </summary>
public sealed class AudioExtractor : IAssetExtractor
{
    public IReadOnlyCollection<string> HandledTypes { get; } = ["AudioClip"];

    public async Task<bool> ExtractAsync(ExtractionContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var baseField = context.BaseField;

            var compressionFormat = baseField.Get("m_CompressionFormat") is { IsDummy: false } cf ? cf.AsInt : 0;
            var audioInfo = new Dictionary<string, object?>
            {
                ["name"] = baseField.Get("m_Name") is { IsDummy: false } n ? n.AsString : "Unknown",
                ["length"] = baseField.Get("m_Length") is { IsDummy: false } len ? len.AsFloat : 0f,
                ["frequency"] = baseField.Get("m_Frequency") is { IsDummy: false } freq ? freq.AsInt : 0,
                ["channels"] = baseField.Get("m_Channels") is { IsDummy: false } ch ? ch.AsInt : 0,
                ["bits_per_sample"] = baseField.Get("m_BitsPerSample") is { IsDummy: false } bps ? bps.AsInt : 0,
                ["compression_format"] = compressionFormat,
                ["load_type"] = baseField.Get("m_LoadType") is { IsDummy: false } lt ? lt.AsInt : 0,
                ["streaming_info"] = context.StreamingInfo,
            };

            var audioDataField = baseField.Get("m_AudioData");
            var audioData = !audioDataField.IsDummy ? audioDataField.AsByteArray : [];

            if (audioData.Length == 0 && context.StreamingInfo is { HasData: true } streamingInfo)
                audioData = context.Loader.TryReadStreamedResource(streamingInfo) ?? [];

            if (audioData.Length > 0)
            {
                var ext = compressionFormat == 1 ? ".ogg" : ".wav";
                await File.WriteAllBytesAsync(context.OutputPathNoExtension + ext, audioData, cancellationToken);
            }

            await ObjectSerializer.WriteJsonAsync(audioInfo, context.OutputPathNoExtension + "_info.json", cancellationToken);
            return true; // writing the info json alone still counts as success
        }
        catch
        {
            return false;
        }
    }
}
