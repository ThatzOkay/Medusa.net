using System.Text.Json;
using Abstractions;
using Abstractions.Services;
using Microsoft.AspNetCore.Mvc;
using Server.Models.Response;
using Server.Plugins;
using Server.Services;
using Path = System.IO.Path;

namespace Server.Api;

public static class PluginsApi
{
    public static WebApplication MapPluginStaticFiles(this WebApplication app)
    {
        // Deliberately a distinct prefix from the client-side `/plugins/{pluginId}/...` SPA
        // routes (registered dynamically by usePluginLoader.ts) — sharing `/plugins` between
        // the two meant a hard navigation/reload of a plugin page got proxied here instead of
        // falling through to the SPA's index.html, producing a false 404.
        app.MapGet("/pluginAssets/{pluginUiId}/{**filePath}", ServePluginFile);
        return app;
    }

    public static RouteGroupBuilder MapPluginApiEndpoints(this RouteGroupBuilder group)
    {
        var pluginsGroup = group.MapGroup("/plugins").WithTags("Plugins");

        pluginsGroup
            .MapGet("/ui", GetUiManifestsAsync)
            .Produces<PluginUiManifestResponse[]>()
            .RequireAuthorization();

        // Documented as a raw string, not PluginUiEventResponse: the OpenAPI-driven client
        // codegen (orval) can only describe what a plain fetch() body deserializes to, and for
        // an SSE stream that's the raw multi-frame text - PluginUiEventResponse only describes
        // the shape of each individual event, which usePluginLoader.ts's real EventSource
        // consumer parses out itself, not this endpoint's declared response type.
        pluginsGroup
            .MapGet("/events", StreamEvents)
            .Produces<string>(200, "text/event-stream");

        return group;
    }

    private static IResult ServePluginFile(
        string pluginUiId,
        string filePath,
        PluginRegistry registry)
    {
        var slot = registry.GetSlots()
            .FirstOrDefault(s =>
                s.Plugin.UiManifest is not null &&
                DerivePluginUiId(s.Plugin) == pluginUiId);

        if (slot is null) return Results.NotFound();

        var pluginDir = Path.GetDirectoryName(slot.Context.AssemblyPath);
        if (pluginDir is null) return Results.NotFound();

        var uiDistDir = Path.GetFullPath(Path.Combine(pluginDir, "ui", "dist"));
        var safeFile  = Path.GetFullPath(Path.Combine(uiDistDir, filePath));

        // Path-traversal guard — never escape ui/dist/
        if (!safeFile.StartsWith(uiDistDir, StringComparison.OrdinalIgnoreCase))
            return Results.BadRequest();

        if (!File.Exists(safeFile)) return Results.NotFound();

        var contentType = Path.GetExtension(safeFile) switch
        {
            ".js"  => "application/javascript",
            ".css" => "text/css",
            ".map" => "application/json",
            _      => "application/octet-stream"
        };

        return Results.File(safeFile, contentType);
    }

    private static async Task<IResult> GetUiManifestsAsync(
        HttpContext httpContext,
        PluginRegistry registry,
        [FromServices] ICardService cardService,
        [FromServices] IPluginService pluginService)
    {
        var userId = httpContext.User
            .FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")
            ?.Value;

        List<string> userCardIds = [];
        if (userId is not null && long.TryParse(userId, out var uid))
        {
            var cards = await cardService.GetCardsByUserIdAsync(uid);
            userCardIds = [.. cards.Select(c => c.RawId)];
        }

        var results = new List<PluginUiManifestResponse>();

        foreach (var s in registry.GetSlots().Where(s => s.Plugin.UiManifest is not null))
        {
            var manifest   = s.Plugin.UiManifest!;
            var pluginUiId = DerivePluginUiId(s.Plugin);
            var pluginDir  = Path.GetDirectoryName(s.Context.AssemblyPath) ?? "";

            if (!manifest.ShowWhenNoProfile && userCardIds.Count > 0)
            {
                var hasProfile = false;
                foreach (var cardId in userCardIds)
                {
                    if (!await pluginService.DoesProfileExistAsync(s.Plugin, cardId)) continue;
                    hasProfile = true;
                    break;
                }
                if (!hasProfile) continue;
            }

            var bundleFile = Path.Combine(pluginDir, "ui", "dist", "plugin.js");
            var sriFile    = Path.Combine(pluginDir, "ui", "dist", "plugin.js.sri");

            var timestamp = File.Exists(bundleFile)
                ? new DateTimeOffset(File.GetLastWriteTimeUtc(bundleFile)).ToUnixTimeSeconds()
                : 0L;

            var sri = File.Exists(sriFile)
                ? (await File.ReadAllTextAsync(sriFile)).Trim()
                : null;

            results.Add(new PluginUiManifestResponse(
                PluginId:    pluginUiId,
                DisplayName: manifest.DisplayName,
                Icon:        manifest.Icon,
                BundleUrl:   $"/pluginAssets/{pluginUiId}/plugin.js?t={timestamp}",
                Sri:         sri,
                NavItems:    manifest.NavItems,
                Routes:      manifest.Routes));
        }

        return Results.Ok(results);
    }

    // Kept as a thin alias (rather than inlining PluginIdentity.DeriveId at call sites) since
    // callers here think of it as "the plugin's UI asset id" - PluginIdentity.DeriveId is the
    // same value, shared with plugin-owned API routes (see Abstractions.PluginEndpointExtensions).
    internal static string DerivePluginUiId(IMedusaPlugin p) => PluginIdentity.DeriveId(p);

    private static async Task StreamEvents(
        PluginUiEventBroadcaster broadcaster,
        HttpContext ctx,
        CancellationToken ct)
    {
        ctx.Response.Headers.ContentType          = "text/event-stream";
        ctx.Response.Headers.CacheControl         = "no-cache";
        ctx.Response.Headers.Connection           = "keep-alive";
        ctx.Response.Headers["X-Accel-Buffering"] = "no";

        var (connectionId, channel) = broadcaster.Subscribe();
        try
        {
            await ctx.Response.WriteAsync(": connected\n\n", ct);
            await ctx.Response.Body.FlushAsync(ct);

            await foreach (var evt in channel.Reader.ReadAllAsync(ct))
            {
                var json = JsonSerializer.Serialize(
                    new PluginUiEventResponse(evt.Type, evt.PluginUiId),
                    JsonSerializerOptions.Web);
                await ctx.Response.WriteAsync($"data: {json}\n\n", ct);
                await ctx.Response.Body.FlushAsync(ct);
            }
        }
        catch (OperationCanceledException) { /* client disconnected */ }
        finally
        {
            broadcaster.Unsubscribe(connectionId);
        }
    }
}