using Server.Services;
using Path = System.IO.Path;

namespace Server.Plugins;

public sealed class PluginWatcher(IPluginService pluginService, ILogger<PluginWatcher> logger) 
    : IHostedService, IDisposable
{
    private FileSystemWatcher? _watcher;
    private readonly string _pluginPath = Path.Combine(AppContext.BaseDirectory, "plugins");
    
    public Task StartAsync(CancellationToken cancellationToken)
    {       
        _watcher = new FileSystemWatcher(_pluginPath, "*.dll")
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName
        };
        _watcher.Changed += OnChanged;
        _watcher.Created += OnChanged;
        _watcher.EnableRaisingEvents = true;
        return Task.CompletedTask;
    }

    private void OnChanged(object _, FileSystemEventArgs e)
    {
        logger.LogInformation("Plugin file changed: {path}", e.FullPath);
        _ = pluginService.ReloadAsync(e.FullPath);
    }
    
    public Task StopAsync(CancellationToken ct) { _watcher?.Dispose(); return Task.CompletedTask; }
    public void Dispose() => _watcher?.Dispose();

}