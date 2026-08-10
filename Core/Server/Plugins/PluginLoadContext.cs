using System.Reflection;
using System.Runtime.Loader;
using Path = System.IO.Path;

namespace Server.Plugins;

public sealed class PluginLoadContext(string pluginAssemblyPath)
: AssemblyLoadContext(isCollectible: true)
{
    private readonly AssemblyDependencyResolver _resolver = new(pluginAssemblyPath);

    public string AssemblyPath => pluginAssemblyPath;

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        
        return path is not null ? LoadFromAssemblyPath(path) : null;
    }
}