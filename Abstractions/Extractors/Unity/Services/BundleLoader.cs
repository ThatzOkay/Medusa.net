using System;
using System.Collections.Generic;
using System.IO;
using Abstractions.Extractors.Unity.Helpers;
using Abstractions.Extractors.Unity.Models;
using AssetsTools.NET;
using AssetsTools.NET.Extra;

namespace Abstractions.Extractors.Unity.Services;

/// <summary>One object found while enumerating a loaded bundle/assets file.</summary>
public record LoadedObject(AssetsFileInstance FileInstance, AssetFileInfo Info)
{
    public int TypeId => Info.GetTypeId(FileInstance.file);
    public string TypeName => BundleLoader.GetTypeName(TypeId);
}

/// <summary>
/// Wraps AssetsTools.NET's AssetsManager. Loads a .bundle (UnityFS/UnityRaw) or a bare serialized
/// .assets file, and exposes its objects and container path map.
/// </summary>
public sealed class BundleLoader : IDisposable
{
    private readonly AssetsManager _manager = new();
    private readonly List<AssetsFileInstance> _fileInstances = [];
    private BundleFileInstance? _bundleInst;
    private bool _classDatabaseLoaded;

    public BundleLoader(string? classDataTpkPath = null)
    {
        var tpkPath = ClassDataTpk.ResolvePath(classDataTpkPath);
        if (File.Exists(tpkPath))
            _manager.LoadClassPackage(tpkPath);
    }

    public AssetsManager Manager => _manager;
    public IReadOnlyList<AssetsFileInstance> Files => _fileInstances;
    public string UnityVersion => _fileInstances.Count > 0 ? _fileInstances[0].file.Metadata.UnityVersion : "Unknown";
    public string Platform => _fileInstances.Count > 0 ? $"TargetPlatform_{_fileInstances[0].file.Metadata.TargetPlatform}" : "Unknown";

    public void Load(string path)
    {
        var header = ReadHeader(path);
        var compression = CompressionDetector.DetectCompressionType(header);

        if (compression is "unityfs" or "raw")
        {
            _bundleInst = _manager.LoadBundleFile(path, true);
            var names = _bundleInst.file.GetAllFileNames();
            for (var i = 0; i < names.Count; i++)
            {
                // Not every entry in a bundle is a serialized assets file (e.g. .resS resource data
                // streamed alongside textures/audio) - IsAssetsFile filters those out up front, but
                // LoadAssetsFileFromBundle can also silently hand back a broken instance (null .file)
                // instead of throwing for some non-assets entries, so double-check the result too.
                if (!_bundleInst.file.IsAssetsFile(i)) continue;

                try
                {
                    var loaded = _manager.LoadAssetsFileFromBundle(_bundleInst, names[i], false);
                    if (loaded.file?.Metadata is not null)
                        _fileInstances.Add(loaded);
                }
                catch
                {
                    // skip anything that doesn't actually parse as an assets file
                }
            }
        }
        else
        {
            _fileInstances.Add(_manager.LoadAssetsFile(path, false));
        }

        if (!_classDatabaseLoaded && _fileInstances.Count > 0)
        {
            _manager.LoadClassDatabaseFromPackage(_fileInstances[0].file.Metadata.UnityVersion);
            _classDatabaseLoaded = true;
        }
    }

    /// <summary>Every object across every loaded assets file.</summary>
    public IEnumerable<LoadedObject> EnumerateObjects()
    {
        foreach (var fileInst in _fileInstances)
        foreach (var info in fileInst.file.AssetInfos)
            yield return new LoadedObject(fileInst, info);
    }

    public AssetTypeValueField GetBaseField(LoadedObject obj) => _manager.GetBaseField(obj.FileInstance, obj.Info);

    /// <summary>The AssetBundle object's m_Container map, resolved down to (path -> path_id/type)
    /// so callers don't need to touch AssetTypeValueField directly.</summary>
    public Dictionary<string, ContainerEntry> GetContainerPaths()
    {
        var result = new Dictionary<string, ContainerEntry>();

        foreach (var fileInst in _fileInstances)
        foreach (var info in fileInst.file.GetAssetsOfType(AssetClassID.AssetBundle))
        {
            var baseField = _manager.GetBaseField(fileInst, info);
            foreach (var pair in ArrayElements(baseField.Get("m_Container")))
            {
                var path = pair.Get("first").AsString;
                var asset = pair.Get("second").Get("asset");
                if (!ReflectionHelper.TryGetPPtr(asset, out var pathId, out var fileId)) continue;

                var typeName = fileId == 0 && fileInst.file.GetAssetInfo(pathId) is { } targetInfo
                    ? GetTypeName(targetInfo.GetTypeId(fileInst.file))
                    : "Unknown";

                result[path] = new ContainerEntry(pathId, typeName);
            }
        }

        return result;
    }

    /// <summary>Resolves a component's owning GameObject's m_Name, used to prefix component object
    /// names as "{GameObjectName}_{ComponentName}".</summary>
    public bool TryGetOwningGameObjectName(AssetsFileInstance relativeTo, AssetTypeValueField componentBaseField, out string name) =>
        TryResolvePPtrName(relativeTo, componentBaseField.Get("m_GameObject"), out name);

    /// <summary>Follows a PPtr field to the object it points at and reads its m_Name. Used for
    /// GameObject names (components) and Shader names (materials). Same-file references only (no
    /// dependencyIndex) - name resolution has never needed to cross bundles in practice.</summary>
    public bool TryResolvePPtrName(AssetsFileInstance relativeTo, AssetTypeValueField pptrField, out string name)
    {
        name = "";
        try
        {
            return TryResolvePPtr(relativeTo, pptrField, null, out var baseField, out _) &&
                   baseField is not null &&
                   ReflectionHelper.TryGetNonEmptyString(baseField, "m_Name", out name);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Resolves a PPtr field to the object it points at, and the file instance that owns it (needed
    /// if the caller has to chase a further PPtr found inside that object - PPtr file IDs are always
    /// relative to whichever file the PPtr itself lives in). fileId == 0 (same file as
    /// <paramref name="relativeTo"/>) always works; fileId != 0 (a genuinely external bundle, e.g. a
    /// Sprite's m_SpriteAtlas) only resolves if a <paramref name="dependencyIndex"/> is supplied.
    ///
    /// A same-file (fileId == 0) lookup normally uses this loader's own private AssetsManager - but
    /// if <paramref name="relativeTo"/> was itself reached through a *previous* cross-bundle hop
    /// (e.g. chasing a PPtr found inside a SpriteAtlas that a different bundle referenced), it's a
    /// shared object owned by <paramref name="dependencyIndex"/>, not this loader, and reading it
    /// still has to go through that index's lock - dependencyIndex is what's actually safe to touch
    /// concurrently here, not "no bundle boundary was crossed this time".
    /// </summary>
    public bool TryResolvePPtr(
        AssetsFileInstance relativeTo,
        AssetTypeValueField pptrField,
        BundleDependencyIndex? dependencyIndex,
        out AssetTypeValueField? baseField,
        out AssetsFileInstance? resolvedFileInstance)
    {
        baseField = null;
        resolvedFileInstance = null;

        if (pptrField.IsDummy || !ReflectionHelper.TryGetPPtr(pptrField, out var pathId, out var fileId))
            return false;

        if (fileId == 0)
        {
            if (!_fileInstances.Contains(relativeTo))
                return dependencyIndex is not null && dependencyIndex.TryGetBaseField(relativeTo, pathId, out baseField, out resolvedFileInstance);

            var info = relativeTo.file.GetAssetInfo(pathId);
            if (info is null) return false;

            baseField = _manager.GetBaseField(relativeTo, info);
            resolvedFileInstance = relativeTo;
            return true;
        }

        if (dependencyIndex is null) return false;

        var externals = relativeTo.file.Metadata.Externals;
        var externalIndex = fileId - 1;
        if (externalIndex < 0 || externalIndex >= externals.Count) return false;

        return dependencyIndex.TryResolve(externals[externalIndex].PathName, pathId, out baseField, out resolvedFileInstance);
    }

    /// <summary>Resolves streamed payload bytes (audio/textures kept out-of-line in a sibling .resS
    /// entry) from the currently loaded bundle. Returns null if this isn't a bundle load, or the
    /// referenced resource entry can't be found (e.g. streamed from a loose file on disk instead).</summary>
    public byte[]? TryReadStreamedResource(StreamingInfo streamingInfo)
    {
        if (_bundleInst is null || string.IsNullOrEmpty(streamingInfo.Path) || streamingInfo.Size <= 0)
            return null;

        var slash = streamingInfo.Path.LastIndexOf('/');
        var entryName = slash >= 0 ? streamingInfo.Path[(slash + 1)..] : streamingInfo.Path;

        var index = _bundleInst.file.GetFileIndex(entryName);
        if (index < 0) return null;

        _bundleInst.file.GetFileRange(index, out var rangeOffset, out _);
        var reader = _bundleInst.file.DataReader;
        reader.Position = rangeOffset + streamingInfo.Offset;
        return reader.ReadBytes((int)streamingInfo.Size);
    }

    internal static IEnumerable<AssetTypeValueField> ArrayElements(AssetTypeValueField field)
    {
        var arrayField = field.Get("Array");
        return arrayField.IsDummy ? [] : arrayField.Children;
    }

    internal static string GetTypeName(int typeId) =>
        Enum.IsDefined(typeof(AssetClassID), typeId) ? ((AssetClassID)typeId).ToString() : $"Type_{typeId}";

    private static byte[] ReadHeader(string path)
    {
        using var stream = File.OpenRead(path);
        var buffer = new byte[Math.Min(32, stream.Length)];
        _ = stream.Read(buffer, 0, buffer.Length);
        return buffer;
    }

    public void Dispose()
    {
        _manager.UnloadAll(true);
    }
}
