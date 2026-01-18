using Microsoft.AspNetCore.Hosting.Server;
using System.Xml.Linq;
using Abstractions.Handlers;
using Abstractions.SerializationTypes;
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

        public override GetServicesResponse Handle(string model)
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
            
            var servicesr = new XElement("services",
                new XAttribute("expire", "3600"),
                new XAttribute("method", "get"),
                new XAttribute("mode", "operation"),
                new XAttribute("status", "0"));

            var coreServices = new string[]{
        "cardmng", "facility", "message", "numbering", "package", "pcbevent", "pcbtracker", "pkglist",
        "posevent", "userdata", "userid", "eacoin", "dlstatus", "netlog", "info", "reference", "sidmgr",
        "local", "local2", "lobby", "slocal", "slocal2", "sglocal", "sglocal2", "lab", "globby",
        "slobby", "sglobby", "eacharge"
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

        private static void AddKfcServices(XElement services)
        {
            var sdvxurl = $"{CommonUrl}";
            string[] kfcServices = [
        "local", "local2", "lobby", "slocal", "slocal2", "sglocal", "sglocal2", "lab", "globby",
        "slobby", "sglobby"
    ];

            foreach(var service in kfcServices)
                services.Add(new XElement("item", new XAttribute("name", service), new XAttribute("url", sdvxurl)));
        }

        private static void AddMdxServices(XElement services)
        {
            var mdxurl = $"{CommonUrl}";
            string[] mdxServices = [
        "local", "local2", "lobby", "slocal", "slocal2", "sglocal", "sglocal2", "lab", "globby",
        "slobby", "sglobby"
    ];

            foreach(var service in mdxServices)
                services.Add(new XElement("item", new XAttribute("name", service), new XAttribute("url", mdxurl)));
        }

        private static void AddM39Services(XElement services)
        {
            var m39url = $"{CommonUrl}";
            string[] m39Services = [
        "local", "local2", "lobby", "slocal", "slocal2", "sglocal", "sglocal2", "lab", "globby",
        "slobby", "sglobby"
    ];

            foreach(var service in m39Services)
                services.Add(new XElement("item", new XAttribute("name", service), new XAttribute("url", m39url)));
        }

    }
}
