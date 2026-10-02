using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using Abstractions.Handlers;
using Server.Models;
using SysPath = System.IO.Path;

namespace Server.Services;

public interface IPackageService
{
    List<PackageEntry> GetPackagesForModel(GameModel model, string baseUrl);
    (FileInfo File, string Sum)? GetPackageFile(string gameCode, string filename);
}

public sealed class PackageService : IPackageService, IDisposable
{
    private readonly string _packagesRoot;
    private readonly ILogger<PackageService> _logger;
    private readonly ConcurrentDictionary<string, GamePackageConfig> _cache = new();
    private readonly FileSystemWatcher? _watcher;

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public PackageService(IConfiguration config, ILogger<PackageService> logger)
    {
        _logger = logger;
        _packagesRoot = config["PackagesPath"] ?? "Data/packages";

        if (!Directory.Exists(_packagesRoot))
        {
            Directory.CreateDirectory(_packagesRoot);
            return;
        }

        _watcher = new FileSystemWatcher(_packagesRoot, "packages.json")
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            EnableRaisingEvents = true
        };
        _watcher.Changed += (_, e) => InvalidateCache(e.FullPath);
        _watcher.Created += (_, e) => InvalidateCache(e.FullPath);
        _watcher.Deleted += (_, e) => InvalidateCache(e.FullPath);
    }

    // Filter rule:  from <= model.Version < till
    //   from = minimum version that may receive this package (0 = everyone)
    //   till = the version this package installs; clients already at/above it are skipped
    public List<PackageEntry> GetPackagesForModel(GameModel model, string baseUrl)
    {
        var config = LoadConfig(model.GameCode);
        if (config is null) return [];

        var filesDir = SysPath.Combine(_packagesRoot, model.GameCode, "files");
        var entries  = new List<PackageEntry>();

        foreach (var pkg in config.Packages)
        {
            if (model.Version < pkg.From || model.Version >= pkg.Till)
                continue;

            var filePath = SysPath.Combine(filesDir, pkg.Filename);
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Package file missing: {Path}", filePath);
                continue;
            }

            entries.Add(new PackageEntry
            {
                Url     = $"{baseUrl}/packages/{model.GameCode}/{pkg.Filename}",
                Name    = pkg.Name,
                Desc    = pkg.Desc,
                Size    = new FileInfo(filePath).Length,
                PkgType = pkg.PkgType,
                SumType = pkg.SumType,
                Sum     = pkg.Sum,
                From    = pkg.From,
                Till    = pkg.Till
            });
        }

        return entries;
    }

    public (FileInfo File, string Sum)? GetPackageFile(string gameCode, string filename)
    {
        var config = LoadConfig(gameCode);
        if (config is null) return null;

        var pkg = config.Packages.FirstOrDefault(p => p.Filename == filename);
        if (pkg is null) return null;

        var path = SysPath.Combine(_packagesRoot, gameCode, "files", filename);
        if (!File.Exists(path)) return null;

        return (new FileInfo(path), pkg.Sum);
    }

    private GamePackageConfig? LoadConfig(string gameCode)
    {
        return _cache.GetOrAdd(gameCode, code =>
        {
            var path = SysPath.Combine(_packagesRoot, code, "packages.json");
            if (!File.Exists(path)) return new GamePackageConfig { GameCode = code };

            try
            {
                var json = File.ReadAllText(path);
                return JsonSerializer.Deserialize<GamePackageConfig>(json, JsonOpts)
                       ?? new GamePackageConfig { GameCode = code };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to parse packages.json for {GameCode}", code);
                return new GamePackageConfig { GameCode = code };
            }
        });
    }

    private void InvalidateCache(string filePath)
    {
        var gameCode = SysPath.GetFileName(SysPath.GetDirectoryName(filePath));
        if (gameCode is not null) _cache.TryRemove(gameCode, out _);
    }

    public void Dispose() => _watcher?.Dispose();
}

// ── Config models (mirrors MedusaPackager's PackageDefinition) ──────────────

internal sealed class PackageDefinition
{
    [JsonPropertyName("name")]    public string Name     { get; set; } = "";
    [JsonPropertyName("desc")]    public string Desc     { get; set; } = "";
    [JsonPropertyName("pkgType")] public string PkgType  { get; set; } = "all";
    [JsonPropertyName("filename")]public string Filename { get; set; } = "";
    [JsonPropertyName("sumType")] public string SumType  { get; set; } = "md5";
    [JsonPropertyName("sum")]     public string Sum      { get; set; } = "";
    [JsonPropertyName("from")]    public long   From     { get; set; } = 0;
    [JsonPropertyName("till")]    public long   Till     { get; set; } = 0;
}

internal sealed class GamePackageConfig
{
    [JsonPropertyName("gameCode")] public string GameCode { get; set; } = "";
    [JsonPropertyName("packages")] public List<PackageDefinition> Packages { get; set; } = [];
}
