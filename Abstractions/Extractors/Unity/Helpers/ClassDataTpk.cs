using System;
using System.IO;

namespace Abstractions.Extractors.Unity.Helpers;

/// <summary>Shared default-location lookup for the bundled classdata.tpk, used by both BundleLoader
/// and BundleDependencyIndex (each owns its own AssetsManager, so each needs to load it).</summary>
internal static class ClassDataTpk
{
    // Always resolved from the host process (core Medusa) - plugins don't ship their own copy.
    public static string ResolvePath(string? overridePath) =>
        overridePath ?? Path.Combine(AppContext.BaseDirectory, "Resources", "classdata.tpk");
}
