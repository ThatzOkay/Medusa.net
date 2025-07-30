using Abstractions;

namespace Server.Services;

public interface IPluginService
{
    void AddPlugin(IMedusaPlugin plugin);
    void RegisterPlugins();
    List<IMedusaPlugin> GetPlugins();
    IMedusaPlugin? FindPlugin(string gameCode, int? minVer = null, int? maxVer = null);
}