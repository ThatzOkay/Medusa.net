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

        Debounce(e.FullPath, () =>
        {
            logger.LogInformation("Plugin file changed: {path}", e.FullPath);
            _ = pluginService.ReloadAsync(e.FullPath);
        });
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