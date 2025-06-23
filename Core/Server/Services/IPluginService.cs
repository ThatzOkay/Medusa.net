using Abstractions;

namespace Server.Services;

public interface IPluginService
{
    void AddPlugin(IMedusaPlugin plugin);
    void RegisterPlugins();
    List<IMedusaPlugin> GetPlugins();
}