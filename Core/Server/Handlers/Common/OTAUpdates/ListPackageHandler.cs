using Abstractions.Handlers;
using Server.Models.Response;
using Server.Services;

namespace Server.Handlers.Common.OTAUpdates;

public class ListPackageHandler : HandlerWithoutRequest<ListPackageResponse>
{
    private readonly IPackageService _packages;
    private readonly MedusaBaseUrl _baseUrl;

    public ListPackageHandler(IPackageService packages, MedusaBaseUrl baseUrl)
    {
        _packages = packages;
        _baseUrl  = baseUrl;
    }

    public override void Configure()
    {
        Module("package");
        Method("list");
    }

    public override ListPackageResponse Handle(GameModel model) =>
        new()
        {
            Status = 0,
            Expire = 1200,
            Items  = _packages.GetPackagesForModel(model, _baseUrl.Value)
        };
}
