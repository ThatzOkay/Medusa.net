using System;
using Microsoft.AspNetCore.Builder;

namespace Abstractions;

/// <summary>
/// Derives the same versioned plugin id used both for UI asset routing
/// (<c>/pluginAssets/{pluginUiId}/...</c>, see Server.Api.PluginsApi.DerivePluginUiId) and for
/// this file's plugin-owned API route prefix - single source of truth so the two never drift
/// apart. GameCode alone isn't always unique: PluginRegistry keys slots by
/// (GameCode, MinVer, MaxVer), so two simultaneously-loaded plugins for the same game but
/// different version windows need distinct ids to avoid colliding on the same route/asset path.
/// </summary>
public static class PluginIdentity
{
    public static string DeriveId(IMedusaPlugin plugin) =>
        (plugin.MinVer, plugin.MaxVer) switch
        {
            (null, null) => plugin.GameCode,
            (not null, null) => $"{plugin.GameCode}-{plugin.MinVer}",
            (null, not null) => $"{plugin.GameCode}-{plugin.MaxVer}",
            (not null, not null) => $"{plugin.GameCode}-{plugin.MinVer}-{plugin.MaxVer}",
        };
}

/// <summary>
/// REST endpoint registration for plugins. Call from <see cref="IMedusaPlugin.OnAppInitialize"/>
/// only - the host never calls AddFastEndpoints()/UseFastEndpoints(), so a FastEndpoints
/// Endpoint&lt;T&gt; class is never reached; these are plain minimal-API routes instead, and
/// (per OnAppInitialize's own doc remarks) get re-registered on every hot-reload along with
/// everything else OnAppInitialize does.
/// </summary>
public static class PluginEndpointExtensions
{
    // "/api/plugins/{pluginId}" rather than bare "/api/{pluginId}" so plugin-owned routes live
    // under the same namespace the host already reserves for plugin concerns (/api/plugins/ui,
    // /api/plugins/events - see Server.Api.PluginsApi) instead of squatting directly on /api/.
    private static string BuildPath(IMedusaPlugin plugin, string path)
    {
        var relative = path.StartsWith('/') ? path : $"/{path}";
        return $"/api/plugins/{PluginIdentity.DeriveId(plugin)}{relative}";
    }

    public static RouteHandlerBuilder MapPluginGet(
        this WebApplication app, IMedusaPlugin plugin, string path, Delegate handler) =>
        app.MapGet(BuildPath(plugin, path), handler);

    public static RouteHandlerBuilder MapPluginPost(
        this WebApplication app, IMedusaPlugin plugin, string path, Delegate handler) =>
        app.MapPost(BuildPath(plugin, path), handler);

    public static RouteHandlerBuilder MapPluginPut(
        this WebApplication app, IMedusaPlugin plugin, string path, Delegate handler) =>
        app.MapPut(BuildPath(plugin, path), handler);

    public static RouteHandlerBuilder MapPluginDelete(
        this WebApplication app, IMedusaPlugin plugin, string path, Delegate handler) =>
        app.MapDelete(BuildPath(plugin, path), handler);
}
