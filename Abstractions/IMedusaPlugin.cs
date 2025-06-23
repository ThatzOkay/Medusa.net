using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace Abstractions;

public interface IMedusaPlugin
{
    string Name { get; }
    string Version { get; }
    string Description { get; }
    Task OnBuilderInitialize(WebApplicationBuilder builder);
    Task OnAppInitialize(WebApplication app);
}