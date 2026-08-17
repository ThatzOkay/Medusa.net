using Abstractions;

namespace Server.Models.Response;

public record PluginUiManifestResponse(
    string           PluginId,
    string           DisplayName,
    string           Icon,
    string           BundleUrl,
    string?          Sri,
    PluginNavItem[]  NavItems,
    PluginRoute[]    Routes);