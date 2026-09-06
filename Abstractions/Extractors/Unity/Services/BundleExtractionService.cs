using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extractors.Unity.Helpers;
using Abstractions.Extractors.Unity.Models;

namespace Abstractions.Extractors.Unity.Services;

/// <summary>
/// The per-bundle driver: loads a bundle, walks every object, dispatches each to the right
/// IAssetExtractor (or FallbackExtractor), and writes out bundle_metadata.json / type_tree.json /
/// dependencies.json / extraction_log.txt.
/// </summary>
public sealed class BundleExtractionService
{
    private static readonly HashSet<string> ComponentTypes =
    [
        "Transform", "Animator", "SkinnedMeshRenderer", "MeshRenderer", "MeshFilter",
        "Camera", "Light", "AudioSource", "AudioListener", "CapsuleCollider",
        "PlayableDirector", "LookAtConstraint", "NavMeshAgent"
    ];

    private readonly Dictionary<string, IAssetExtractor> _extractorsByType = new();
    private readonly FallbackExtractor _fallback = new();
    private readonly string? _classDataTpkPath;
    private readonly BundleDependencyIndex? _dependencyIndex;

    /// <param name="dependencyIndex">Optional. When supplied, PPtrs that point at a *different*
    /// bundle file (e.g. a Sprite's m_SpriteAtlas) can be resolved instead of just failing - pass an
    /// index built (once) over the directory tree the bundle(s) being processed live in. Without
    /// one, cross-bundle references are simply left unresolved, same as before this existed.</param>
    public BundleExtractionService(string? classDataTpkPath = null, IEnumerable<IAssetExtractor>? extractors = null, BundleDependencyIndex? dependencyIndex = null)
    {
        _classDataTpkPath = classDataTpkPath;
        _dependencyIndex = dependencyIndex;

        extractors ??=
        [
            new TextureExtractor(), new AudioExtractor(), new MeshExtractor(), new MaterialExtractor(),
            new AnimationExtractor(), new FontExtractor(), new TextAssetExtractor(), new MonoScriptExtractor(),
            new MonoBehaviourExtractor(),
        ];

        foreach (var extractor in extractors)
        foreach (var type in extractor.HandledTypes)
            _extractorsByType[type] = extractor;
    }

    public async Task<ExtractionLog> ExtractBundleAsync(
        string bundlePath,
        string outputDir,
        IProgress<(int Done, int Total)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(outputDir);

        var signature = await CompressionDetector.GetFileSignatureAsync(bundlePath, cancellationToken);
        var log = new ExtractionLog
        {
            BundlePath = bundlePath,
            BundleInfo = new LoggedBundleInfo(Convert.ToHexString(signature.Signature).ToLowerInvariant(), signature.Compression, signature.Size),
        };

        using var loader = new BundleLoader(_classDataTpkPath);
        try
        {
            loader.Load(bundlePath);
        }
        catch (Exception e)
        {
            await File.WriteAllTextAsync(Path.Combine(outputDir, "extraction_log.txt"),
                $"Critical Error: Could not load bundle. Details: {e.Message}\nTraceback:\n{e}\n", cancellationToken);
            throw;
        }

        var objects = loader.EnumerateObjects().ToList();
        var typeTreeData = new Dictionary<string, object?>();
        var allDependencies = new Dictionary<string, object?>();

        for (var i = 0; i < objects.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var obj = objects[i];
            log.TotalObjectsProcessed++;
            var objType = obj.TypeName;
            var extractedSuccessfully = false;

            try
            {
                var baseField = loader.GetBaseField(obj);

                typeTreeData[$"{objType}_{obj.Info.PathId}"] = new TypeTreeInfo
                {
                    ClassId = obj.TypeId,
                    ClassName = objType,
                    PathId = obj.Info.PathId,
                    DataOffset = obj.Info.ByteOffset,
                    DataSize = obj.Info.ByteSize,
                };

                var dependencies = DependencyResolver.GetObjectDependencies(baseField);
                if (dependencies.Count > 0)
                    allDependencies[$"{objType}_{obj.Info.PathId}"] = dependencies;

                var objName = ReflectionHelper.GetObjectName(baseField, objType, obj.Info.PathId);
                if (ComponentTypes.Contains(objType) && loader.TryGetOwningGameObjectName(obj.FileInstance, baseField, out var gameObjectName))
                    objName = $"{FileNameSanitizer.Sanitize(gameObjectName)}_{objName}";

                var finalOutputPath = Path.Combine(outputDir, objType, objName);
                Directory.CreateDirectory(Path.GetDirectoryName(finalOutputPath)!);

                var streamingInfo = ReflectionHelper.TryGetStreamingInfo(baseField);
                if (streamingInfo is { HasData: true })
                    await ObjectSerializer.WriteJsonAsync(streamingInfo, finalOutputPath + "_streaming.json", cancellationToken);

                var context = new ExtractionContext(loader, obj.FileInstance, obj.Info, baseField, objType, obj.Info.PathId, finalOutputPath, streamingInfo, _dependencyIndex);
                var extractor = _extractorsByType.GetValueOrDefault(objType, _fallback);
                extractedSuccessfully = await extractor.ExtractAsync(context, cancellationToken);
            }
            catch (Exception e)
            {
                log.Errors.Add(new ExtractionError { ObjectId = obj.Info.PathId, Type = objType, Error = e.Message, Traceback = e.ToString() });
            }

            if (extractedSuccessfully)
            {
                log.SuccessfulExtractions++;
                log.ExtractedCounts[objType] = log.ExtractedCounts.GetValueOrDefault(objType) + 1;
            }
            else
            {
                log.FailedExtractions++;
            }

            progress?.Report((i + 1, objects.Count));
        }

        var bundleMetadata = new BundleMetadata
        {
            Version = loader.UnityVersion,
            Platform = loader.Platform,
            ObjectCount = objects.Count,
            ContainerPaths = loader.GetContainerPaths(),
        };

        await ObjectSerializer.WriteJsonAsync(bundleMetadata, Path.Combine(outputDir, "bundle_metadata.json"), cancellationToken);
        await ObjectSerializer.WriteJsonAsync(typeTreeData, Path.Combine(outputDir, "type_tree.json"), cancellationToken);
        if (allDependencies.Count > 0)
            await ObjectSerializer.WriteJsonAsync(allDependencies, Path.Combine(outputDir, "dependencies.json"), cancellationToken);

        await WriteLogTextAsync(log, Path.Combine(outputDir, "extraction_log.txt"), cancellationToken);
        return log;
    }

    private static async Task WriteLogTextAsync(ExtractionLog log, string path, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        sb.AppendLine("== Enhanced Unity Bundle Extraction Log ==");
        sb.AppendLine($"Timestamp: {log.Timestamp:O}");
        sb.AppendLine($"Bundle: {log.BundlePath}");
        sb.AppendLine($"Signature: {log.BundleInfo.Signature}");
        sb.AppendLine($"Compression: {log.BundleInfo.Compression}");
        sb.AppendLine($"Size: {log.BundleInfo.Size} bytes");
        sb.AppendLine();
        sb.AppendLine($"Total objects processed: {log.TotalObjectsProcessed}");
        sb.AppendLine($"Successfully extracted: {log.SuccessfulExtractions}");
        sb.AppendLine($"Failed extractions: {log.FailedExtractions}");
        sb.AppendLine();
        sb.AppendLine("== Asset Type Statistics ==");
        if (log.ExtractedCounts.Count == 0)
        {
            sb.AppendLine("No objects were successfully extracted.");
        }
        else
        {
            foreach (var (type, count) in log.ExtractedCounts.OrderBy(kv => kv.Key, StringComparer.Ordinal))
                sb.AppendLine($"- {type}: {count}");
        }

        sb.AppendLine();
        sb.AppendLine("== Error Details ==");
        if (log.Errors.Count == 0)
        {
            sb.AppendLine("No errors recorded.");
        }
        else
        {
            foreach (var error in log.Errors)
            {
                sb.AppendLine($"Object ID: {error.ObjectId}");
                sb.AppendLine($"Type: {error.Type}");
                sb.AppendLine($"Error: {error.Error}");
                if (!string.IsNullOrEmpty(error.Traceback))
                    sb.AppendLine($"Traceback:\n{error.Traceback}");
                sb.AppendLine(new string('=', 50));
            }
        }

        await File.WriteAllTextAsync(path, sb.ToString(), cancellationToken);
    }
}
