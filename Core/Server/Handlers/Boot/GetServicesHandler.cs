using Microsoft.AspNetCore.Hosting.Server;
using Abstractions.Handlers;
using Abstractions.Utils;
using Server.Models.Response;

namespace Server.Handlers.Boot
{
    public class GetServicesHandler(IServer server, ILogger<GetServicesHandler> logger) : HandlerWithoutRequest<GetServicesResponse>
    {
        private readonly ILogger<GetServicesHandler> _logger = logger;
        private static readonly string ListeningAddress = Environment.GetEnvironmentVariable("MAIN_ADDRESS") ?? $"http://{IpUtils.GetLocalIPv4()}:5120";
        private static readonly string CommonUrl = $"{ListeningAddress}/eamuse";

        public override void Configure()
        {
            Module("services");
            Method("get");
        }

        public override GetServicesResponse Handle(GameModel model)
        {
            var services = CreateCoreServicesElement();
            return services;
        }

        private static GetServicesResponse CreateCoreServicesElement()
        {
            var services = new GetServicesResponse()
            {
                Expire = 3600,
                Method = "get",
                Mode = "operation",
                Status = 0
            };

            var coreServices = new string[]{
        "cardmng", "facility", "message", "numbering", "package", "pcbevent", "pcbtracker", "pkglist",
        "posevent", "userdata", "userid", "eacoin", "dlstatus", "netlog", "info", "reference", "sidmgr",
        "local", "local2", "lobby", "slocal", "slocal2", "sglocal", "sglocal2", "lab", "globby",
        "slobby", "sglobby", "eacharge", "sppass"
    };

            foreach(var service in coreServices)
            {
                services.Items.Add(new ServiceItem
                {
                    Name = service,
                    Url = $"{CommonUrl}/{service}"
                });
            }

            services.Items.Add(new ServiceItem()
            {
                Name = "ntp",
                Url = "ntp://pool.ntp.org/"
            });

            services.Items.Add(new ServiceItem()
            {
                Name = "keepalive",
                Url = "http://127.0.0.1:8083/keepalive?pa=127.0.0.1&ia=127.0.0.1&ga=127.0.0.1&ma=127.0.0.1&t1=2&t2=10"
            });

            return services;
        }

    }
}
