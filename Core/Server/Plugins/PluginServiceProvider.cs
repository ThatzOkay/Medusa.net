namespace Server.Plugins;

/// <summary>
/// Resolves services from the plugin's own mini-container first,
/// falling back to the host container for shared services (ICardService, IUserService, etc.).
/// </summary>
internal sealed class PluginServiceProvider(IServiceProvider plugin, IServiceProvider host) : IServiceProvider
{
    public object? GetService(Type t) => plugin.GetService(t) ?? host.GetService(t);
}
