using System;
using System.IO;

namespace Abstractions.Extractors.Unity.Helpers;

/// <summary>Shared default-location lookup for the bundled classdata.tpk, used by both BundleLoader
/// and BundleDependencyIndex (each owns its own AssetsManager, so each needs to load it).</summary>
internal static class ClassDataTpk
{
    // Resolved next to UnityBundleLib's own assembly, not AppContext.BaseDirectory - a consumer
    // loaded into its own AssemblyLoadContext (e.g. a Medusa plugin loaded from a "plugins/<name>"
    // folder) has an AppContext.BaseDirectory pointing at the *host* process's directory, not the
    // plugin's, even though the NuGet package's contentFiles copy classdata.tpk next to this DLL.
    public static string ResolvePath(string? overridePath) =>
        overridePath ?? Path.Combine(
            Path.GetDirectoryName(typeof(ClassDataTpk).Assembly.Location) ?? AppContext.BaseDirectory,
            "Resources", "classdata.tpk");
}
