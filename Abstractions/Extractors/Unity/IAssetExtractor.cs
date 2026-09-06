using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Extractors.Unity.Models;
using Abstractions.Extractors.Unity.Services;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace Abstractions.Extractors.Unity;

/// <summary>Everything a single extractor needs to do its job, gathered up-front by the orchestrator
/// (loader lookups, name resolution, output path) so each extractor only has to deal with its own
/// asset-type-specific fields.</summary>
public sealed record ExtractionContext(
    BundleLoader Loader,
    AssetsFileInstance FileInstance,
    AssetFileInfo Info,
    AssetTypeValueField BaseField,
    string ObjectType,
    long PathId,
    string OutputPathNoExtension,
    StreamingInfo? StreamingInfo,
    BundleDependencyIndex? DependencyIndex = null);

/// <summary>One asset-type-specific extraction strategy.</summary>
public interface IAssetExtractor
{
    /// <summary>The AssetClassID type name(s) this extractor handles.</summary>
    IReadOnlyCollection<string> HandledTypes { get; }

    Task<bool> ExtractAsync(ExtractionContext context, CancellationToken cancellationToken = default);
}
