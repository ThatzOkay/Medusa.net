using System.Reflection;
using System.Runtime.Loader;
using Path = System.IO.Path;

namespace Server.Plugins;

public sealed class PluginLoadContext(string pluginAssemblyPath)
: AssemblyLoadContext(isCollectible: true)
{
    private readonly AssemblyDependencyResolver _resolver = new(pluginAssemblyPath);

    public string AssemblyPath => pluginAssemblyPath;

    // Host-shared contract assemblies. Plugins may ship their own copies for build-time tooling
    // (e.g. `dotnet ef migrations add`), but at runtime these must resolve to the host's ALC
    // so that types like IMedusaPlugin and AssetTypeValueField have a single identity.
    // Without this, `t.GetInterfaces().Contains(typeof(IMedusaPlugin))` silently fails.
    private static readonly HashSet<string> SharedAssemblyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Abstractions",
        "AssetsTools.NET",
        "AssetsTools.NET.Texture"
    };

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        if (assemblyName.Name is not null && SharedAssemblyNames.Contains(assemblyName.Name))
            return null; // fall through to the Default AssemblyLoadContext

        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        if (path is null) return null;

        using var stream = new MemoryStream(File.ReadAllBytes(path));
        return LoadFromStream(stream);
    }
}