using System.Collections.Concurrent;
using Server.Services;
using Path = System.IO.Path;

namespace Server.Plugins;

public sealed class PluginWatcher(IPluginService pluginService, ILogger<PluginWatcher> logger)
    : IHostedService, IDisposable
{
    // A single file write typically raises several raw Changed/Created events in a row
    // (buffered writes, temp-file renames, etc.) - debounce per path so one logical
    // save only triggers one reload instead of a pile of concurrent ones.
    private static readonly TimeSpan DebounceDelay = TimeSpan.FromMilliseconds(400);

    private FileSystemWatcher? _watcher;
    private readonly string _pluginPath = Path.Combine(AppContext.BaseDirectory, "plugins");
    private readonly ConcurrentDictionary<string, DebounceEntry> _debounceTimers = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // Watch everything under plugins/, not just *.dll - a whole plugin subfolder
        // getting deleted (e.g. `rm -rf plugins/Foo`) is a directory-name change, and
        // filtering to *.dll would never see that entry at all.
        _watcher = new FileSystemWatcher(_pluginPath)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
        };
        _watcher.Changed += OnChanged;
        _watcher.Created += OnChanged;
        _watcher.Deleted += OnDeleted;
        _watcher.EnableRaisingEvents = true;
        return Task.CompletedTask;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        // Only .dll writes matter for hot-reload; ignore other files and directory touches.
        if (!e.FullPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) return;

        // Debounce (and reload) by the plugin's own root directory rather than the exact file
        // that changed - a package-referenced plugin ships dozens of dependency dlls (and
        // culture subfolders of satellite resource dlls), so a single redeploy touches many
        // paths at once. Keying per-file would fire one reload per dll instead of one overall.
        var pluginDir = GetPluginRootDir(e.FullPath);
        if (pluginDir is null) return;

        Debounce(pluginDir, () =>
        {
            logger.LogInformation("Plugin directory changed: {dir}", pluginDir);
            _ = pluginService.ReloadAsync(pluginDir);
        });
    }

    // Resolves any path under plugins/<PluginName>/... (including nested culture subfolders
    // like plugins/<PluginName>/cs/X.resources.dll) back to plugins/<PluginName>. Null if the
    // path isn't inside a plugin folder at all (e.g. a stray file dropped directly in plugins/).
    private string? GetPluginRootDir(string fullPath)
    {
        var relative = Path.GetRelativePath(_pluginPath, fullPath);
        var separatorIndex = relative.IndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);
        return separatorIndex < 0 ? null : Path.Combine(_pluginPath, relative[..separatorIndex]);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        // Don't filter by extension here - a deleted path might be the plugin's own
        // subfolder rather than the dll directly. PluginService.UnloadAsync matches both.
        Debounce(e.FullPath, () =>
        {
            logger.LogInformation("Plugin path deleted: {path}", e.FullPath);
            _ = pluginService.UnloadAsync(e.FullPath);
        });
    }

    // A single logical change (e.g. a rebuild replacing a dll, or a delete immediately
    // followed by a recreation) commonly raises several raw events for the same path in a
    // row. Coalesce them per path, and always run whichever action arrived last -
    // resetting only the timer's due time would leave the *first* action queued, which
    // is wrong (e.g. a Deleted after a Changed must win, not silently re-run the reload).
    private void Debounce(string path, Action action)
    {
        _debounceTimers.AddOrUpdate(path,
            addValueFactory: key =>
            {
                var entry = new DebounceEntry(action);
                entry.Timer = new Timer(timerState =>
                {
                    _debounceTimers.TryRemove(path, out _);
                    entry.Action();
                }, null, DebounceDelay, Timeout.InfiniteTimeSpan);
                return entry;
            },
            updateValueFactory: (_, existing) =>
            {
                existing.Action = action;
                existing.Timer?.Change(DebounceDelay, Timeout.InfiniteTimeSpan);
                return existing;
            });
    }

    public Task StopAsync(CancellationToken ct) { _watcher?.Dispose(); return Task.CompletedTask; }

    public void Dispose()
    {
        _watcher?.Dispose();
        foreach (var entry in _debounceTimers.Values) entry.Timer?.Dispose();
        _debounceTimers.Clear();
    }

    private sealed class DebounceEntry(Action action)
    {
        public Action Action { get; set; } = action;
        public Timer? Timer { get; set; }
    }
}