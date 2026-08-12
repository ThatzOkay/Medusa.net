using System.Reflection;
using System.Runtime.Loader;
using Path = System.IO.Path;

namespace Server.Plugins;

public sealed class PluginLoadContext(string pluginAssemblyPath)
: AssemblyLoadContext(isCollectible: true)
{
    private readonly AssemblyDependencyResolver _resolver = new(pluginAssemblyPath);

    public string AssemblyPath => pluginAssemblyPath;

    // Contract assemblies that every plugin compiles against and the host also has loaded in
    // its own default AssemblyLoadContext - these must always bind to the host's copy, never a
    // plugin-local one, or types like IMedusaPlugin end up with two distinct identities (the
    // host's and the plugin's own), and every `t.GetInterfaces().Contains(typeof(IMedusaPlugin))`
    // style check silently fails even though the plugin dll loaded fine. A plugin may still ship
    // its own copy of "Abstractions.dll" locally (e.g. it needs one for `dotnet ef migrations
    // add` to run standalone) - that copy is intentionally ignored here.
    private static readonly HashSet<string> SharedAssemblyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Abstractions"
    };

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name is not null && SharedAssemblyNames.Contains(assemblyName.Name))
            return null; // fall through to the Default AssemblyLoadContext

        var path = _resolver.ResolveAssemblyToPath(assemblyName);

        return path is not null ? LoadFromAssemblyPath(path) : null;
    }
}