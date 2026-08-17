using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Abstractions;

public interface IMedusaPlugin
{
    string Name { get; }
    string Version { get; }
    string Description { get; }
    string GameCode { get; }
    int? MinVer { get; }
    int? MaxVer { get; }
    Encoding? ForcedEncoding { get; }
    PluginUiManifest? UiManifest { get; }

    /// <summary>
    /// Called once before <c>builder.Build()</c>.
    /// Register DbContexts, HotChocolate type extensions, and other services that must exist
    /// before the DI container is sealed.
    /// </summary>
    /// <remarks>
    /// <b>Hot-reload caveat:</b> this method is NOT called again on hot-reload — the DI container
    /// and HotChocolate schema are already built by then. Any GraphQL type extensions you add here
    /// only take effect when the host application restarts. REST endpoints belong in
    /// <see cref="OnAppInitialize"/> instead, which IS called on every reload.
    /// </remarks>
    Task OnBuilderInitialize(WebApplicationBuilder builder);

    /// <summary>
    /// Register plugin-owned services (DbContext, repositories, etc.) into the plugin's
    /// own mini-container. Called for both initial load and hot-reload.
    /// Note: also register your DbContext in OnBuilderInitialize if you need migrations via OnAppInitialize.
    /// </summary>
    void ConfigurePluginServices(IServiceCollection services);

    Task OnAppInitialize(WebApplication app, IServiceProvider pluginServices);

    Delegate DoesProfileExist { get; }
}

public record PluginNavItem(string Label, string Icon, string Path);

/// <param name="Path">Absolute path, e.g. "/plugins/foo" or "/plugins/foo/scores".</param>
/// <param name="ComponentKey">Key into the IIFE bundle's export object, e.g. "FooHome".</param>
public record PluginRoute(string Path, string ComponentKey);

public record PluginUiManifest(
    string DisplayName,
    PluginNavItem[] NavItems,
    PluginRoute[] Routes,
    string Icon = "material-symbols:extension-rounded",
    bool ShowWhenNoProfile = false);