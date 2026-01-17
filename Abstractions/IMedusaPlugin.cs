using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

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
    Task OnAppInitialize(WebApplication app);
    Task<bool> DoesProfileExist(string cardId);
}