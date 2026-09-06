using System;
using System.Collections.Generic;
using System.IO;
using Abstractions.Extractors.Unity.Helpers;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace Abstractions.Extractors.Unity.Services;

/// <summary>
/// Resolves PPtr references that point outside the currently-loaded bundle (e.g. a Sprite's
/// m_SpriteAtlas, which typically lives in a separate .bundle file entirely) back to an actual
/// object. Unity's AssetsFileExternal only records a virtual "archive:/CAB-xxx/CAB-xxx" path, not
/// a real file path, so this first builds an index of every bundle under a root directory by its
/// internal CAB-hash name(s) - a directory-only scan (no block decompression), so indexing even
/// ~10k bundles takes a couple of seconds - then lazily loads and caches whichever dependency
/// bundles actually get asked for.
/// </summary>
public sealed class BundleDependencyIndex : IDisposable
{
    private readonly AssetsManager _manager = new();
    private readonly Dictionary<string, string> _cabNameToBundlePath = new(StringComparer.Ordinal);
    private readonly Dictionary<string, AssetsFileInstance> _loadedFiles = new(StringComparer.Ordinal);
    private readonly Dictionary<string, BundleFileInstance> _loadedBundles = new(StringComparer.Ordinal);
    private readonly HashSet<AssetsFileInstance> _ownedFiles = [];
    private readonly HashSet<string> _loadedClassDatabaseVersions = [];

    // TryResolve lazily loads and caches dependency bundles on first use - unlike the CAB->path
    // index (built once up front, read-only afterwards), those caches and the single shared
    // AssetsManager backing them are mutated *during* extraction. If extraction is parallelized,
    // two bundles resolving the same not-yet-loaded dependency at once would race on this state,
    // so every call goes through this lock. This also covers work that happens *after* resolution
    // but still touches a shared bundle's data (e.g. decoding a texture that lives in a shared
    // dependency bundle reads bytes through that bundle's single shared reader) - see Owns/RunLocked.
    private readonly object _resolveLock = new();

    public BundleDependencyIndex(string? classDataTpkPath = null)
    {
        var tpkPath = ClassDataTpk.ResolvePath(classDataTpkPath);
        if (File.Exists(tpkPath))
            _manager.LoadClassPackage(tpkPath);
    }

    public int IndexedBundleCount { get; private set; }

    /// <summary>Scans every .bundle under <paramref name="rootDir"/> and records which file each
    /// internal CAB-hash entry lives in. Safe to call multiple times / with multiple roots.</summary>
    public void IndexDirectory(string rootDir, string searchPattern = "*.bundle")
    {
        var scanManager = new AssetsManager();
        try
        {
            foreach (var path in Directory.EnumerateFiles(rootDir, searchPattern, SearchOption.AllDirectories))
            {
                try
                {
                    var bunInst = scanManager.LoadBundleFile(path, false);
                    foreach (var name in bunInst.file.GetAllFileNames())
                        _cabNameToBundlePath[name] = path;
                    scanManager.UnloadBundleFile(bunInst);
                    IndexedBundleCount++;
                }
                catch
                {
                    // not a readable bundle - skip
                }
            }
        }
        finally
        {
            scanManager.UnloadAll(true);
        }
    }

    /// <summary>Resolves an AssetsFileExternal.PathName (e.g. "archive:/CAB-xxx/CAB-xxx") plus a
    /// path ID to the object it points at (and the file instance that owns it, needed if the
    /// caller has to chase a further PPtr found inside that object), loading and caching the
    /// owning bundle on first use.</summary>
    public bool TryResolve(string externalPathName, long pathId, out AssetTypeValueField? baseField, out AssetsFileInstance? fileInstance)
    {
        lock (_resolveLock)
        {
            baseField = null;
            fileInstance = null;

            var cabName = ExtractCabName(externalPathName);
            if (cabName is null) return false;

            if (!_loadedFiles.TryGetValue(cabName, out var fileInst))
            {
                if (!_cabNameToBundlePath.TryGetValue(cabName, out var bundlePath)) return false;

                if (!_loadedBundles.TryGetValue(bundlePath, out var bunInst))
                {
                    bunInst = _manager.LoadBundleFile(bundlePath, true);
                    _loadedBundles[bundlePath] = bunInst;
                }

                fileInst = _manager.LoadAssetsFileFromBundle(bunInst, cabName, false);
                if (fileInst.file?.Metadata is null) return false;

                _loadedFiles[cabName] = fileInst;
                _ownedFiles.Add(fileInst);
                EnsureClassDatabase(fileInst);
            }

            var info = fileInst.file.GetAssetInfo(pathId);
            if (info is null) return false;

            baseField = _manager.GetBaseField(fileInst, info);
            fileInstance = fileInst;
            return true;
        }
    }

    /// <summary>Reads an object directly out of a file instance this index has already loaded (e.g.
    /// chasing a further PPtr found inside a previously-resolved object). Must go through the same
    /// lock as everything else here, since the file instance is shared across every caller.</summary>
    public bool TryGetBaseField(AssetsFileInstance fileInstance, long pathId, out AssetTypeValueField? baseField, out AssetsFileInstance? resolvedFileInstance)
    {
        lock (_resolveLock)
        {
            baseField = null;
            resolvedFileInstance = null;

            var info = fileInstance.file.GetAssetInfo(pathId);
            if (info is null) return false;

            baseField = _manager.GetBaseField(fileInstance, info);
            resolvedFileInstance = fileInstance;
            return true;
        }
    }

    /// <summary>True if this file instance was loaded by this index (as opposed to a caller's own
    /// private BundleLoader). Anything that reads raw bytes out of an owned instance - not just
    /// GetBaseField, e.g. AssetsTools.NET.Texture's FillPictureData/DecodeTextureRaw pulling pixel
    /// data out of a shared bundle's single reader - needs to happen under <see cref="RunLocked{T}"/>,
    /// the same lock guarding everything else here.</summary>
    public bool Owns(AssetsFileInstance fileInstance)
    {
        lock (_resolveLock)
            return _ownedFiles.Contains(fileInstance);
    }

    /// <summary>Runs arbitrary work under this index's lock. For anything that reads a shared
    /// dependency bundle's raw data outside of GetBaseField itself - see <see cref="Owns"/>.</summary>
    public T RunLocked<T>(Func<T> action)
    {
        lock (_resolveLock)
            return action();
    }

    private void EnsureClassDatabase(AssetsFileInstance fileInst)
    {
        var version = fileInst.file.Metadata.UnityVersion;
        if (_loadedClassDatabaseVersions.Add(version))
            _manager.LoadClassDatabaseFromPackage(version);
    }

    private static string?
        ExtractCabName(string externalPathName)
    {
        if (string.IsNullOrEmpty(externalPathName)) return null;
        var slash = externalPathName.LastIndexOf('/');
        return slash >= 0 ? externalPathName[(slash + 1)..] : externalPathName;
    }

    public void Dispose() => _manager.UnloadAll(true);
}
