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