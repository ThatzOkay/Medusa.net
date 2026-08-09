using Abstractions;
using Microsoft.AspNetCore.Builder;

namespace Server.Services;

public interface IPluginService
{
    /// <summary>Pre-build: scan plugin dirs, load slots, call OnBuilderInitialize on each.</summary>
    Task DiscoverPluginsAsync(WebApplicationBuilder builder);

    /// <summary>Post-build: add slots to the registry, call OnAppInitialize on each.</summary>
    Task ActivatePluginsAsync(WebApplication app);

    /// <summary>Hot-swap a single plugin DLL without restarting.</summary>
    Task ReloadAsync(string pluginDllPath);

    void SetServiceProvider(IServiceProvider serviceProvider);

    IEnumerable<IMedusaPlugin> GetPlugins();
    IMedusaPlugin? FindPlugin(string gameCode, int? minVer = null, int? maxVer = null);
    Task<bool> DoesProfileExistAsync(IMedusaPlugin plugin, string cardId);
}