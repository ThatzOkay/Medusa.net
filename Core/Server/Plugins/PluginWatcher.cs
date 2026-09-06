using System.Collections.Concurrent;
using Server.Services;
using Path = System.IO.Path;

namespace Server.Plugins;

public sealed class PluginWatcher(IPluginService pluginService, ILogger<PluginWatcher> logger)
    : IHostedService, IDisposable
{
    // A single file write typically raises some raw Changed/Created events in a row
    // (buffered writes, temp-file renames, etc.) - debounce per path so one logical
    // save only triggers one reload instead of a pile of concurrent ones.
    private static readonly TimeSpan DebounceDelay = TimeSpan.FromMilliseconds(400);

    private FileSystemWatcher? _watcher;
    private readonly string _pluginPath = Path.Combine(AppContext.BaseDirectory, "plugins");
    private readonly ConcurrentDictionary<string, DebounceEntry> _debounceTimers = new();

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _watcher = CreateWatcher();
        _watcher.EnableRaisingEvents = true;
        return Task.CompletedTask;
    }

    private FileSystemWatcher CreateWatcher()
    {
        // Watch everything under plugins/, not just *.dll - a plugin's whole subfolder
        // getting deleted (e.g. `rm -rf plugins/Foo`) is a directory-name change, and
        // filtering to *.dll would never see that entry at all. Size is included alongside
        // LastWrite because some copy tools preserve the source file's original mtime on
        // the copy (rsync -a, some GUI file managers) - a redeployed dll with a changed
        // size but an untouched timestamp would otherwise raise no event at all.
        var watcher = new FileSystemWatcher(_pluginPath)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName |
                            NotifyFilters.Size
        };
        watcher.Changed += OnChanged;
        watcher.Created += OnCreated;
        watcher.Renamed += OnRenamed;
        watcher.Deleted += OnDeleted;
        watcher.Error += OnError;
        return watcher;
    }

    // rsync (and similar tools) write via temp file + atomic rename, which shows up as
    // Renamed, not Changed/Created - route it the same way or those deploys go unseen.
    private void OnRenamed(object sender, RenamedEventArgs e) => OnCreated(sender, e);

    // A dropped/overflowed watcher (e.g. its internal event buffer overflowing because a new
    // plugin folder dumped dozens of dependency dlls in at once) doesn't just miss that burst -
    // FileSystemWatcher stops reliably raising events at all afterwards unless recreated. Log
    // it and swap in a fresh watcher rather than silently going deaf to every future change.
    private void OnError(object sender, ErrorEventArgs e)
    {
        logger.LogError(e.GetException(), "Plugin file watcher failed; recreating it");

        _watcher?.Dispose();
        _watcher = CreateWatcher();
        _watcher.EnableRaisingEvents = true;
    }

    // A brand-new plugin folder (as opposed to a redeploy into an already-watched one) races
    // FileSystemWatcher's own subdirectory bookkeeping: the watch on a newly created
    // subdirectory is only registered once this Created event for the directory itself has
    // been processed. If the folder is populated in one fast burst (`cp -r`, an unzip, an
    // installer script that mkdirs then writes) the dlls inside can land during that gap, so
    // their own Created events are simply never seen. A single synchronous check right here
    // isn't enough either - most copies mkdir the folder first and stream files in afterward,
    // so the directory can still be empty at the instant this handler runs. Poll for a .dll to
    // show up instead of taking one snapshot, same debounce delay between checks, and give up
    // (with a log) after a few seconds rather than polling forever.a
    private const int NewPluginDirPollAttempts = 10;

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        if (Directory.Exists(e.FullPath) && GetPluginRootDir(e.FullPath) is null)
        {
            PollNewPluginDir(e.FullPath, attempt: 1);
            return;
        }

        OnChanged(sender, e);
    }

    private void PollNewPluginDir(string dir, int attempt)
    {
        Debounce(dir, () =>
        {
            if (Directory.EnumerateFiles(dir, "*.dll", SearchOption.AllDirectories).Any())
            {
                logger.LogInformation("New plugin directory detected: {dir}", dir);
                _ = pluginService.ReloadAsync(dir);
                return;
            }

            if (attempt >= NewPluginDirPollAttempts)
            {
                logger.LogWarning(
                    "New plugin directory '{dir}' still has no .dll after {attempts} checks; giving up",
                    dir, NewPluginDirPollAttempts);
                return;
            }

            PollNewPluginDir(dir, attempt + 1);
        });
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