using Abstractions;

namespace Server.Plugins;

public sealed class PluginSlot(
    PluginLoadContext context,
    IMedusaPlugin plugin,
    IReadOnlyList<Type> handlerTypes,
    IServiceProvider pluginServices)
{
    public PluginLoadContext Context => context;
    public IMedusaPlugin Plugin => plugin;
    public IReadOnlyList<Type> HandlerTypes => handlerTypes;
    public PluginSlotKey Key => new(plugin.GameCode, plugin.MinVer, plugin.MaxVer);

    /// <summary>Mini service provider built from the plugin's ConfigurePluginServices call.</summary>
    public IServiceProvider PluginServices => pluginServices;

    public void Unload()
    {
        context.Unload();
        if (pluginServices is IDisposable d) d.Dispose();
    }
}