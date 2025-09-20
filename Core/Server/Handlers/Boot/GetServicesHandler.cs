using Microsoft.AspNetCore.Hosting.Server;
using System.Xml.Linq;
using Abstractions.Handlers;

namespace Server.Handlers.Boot
{
    public class GetServicesHandler(IServer server, ILogger<GetServicesHandler> logger, XDocument body) : Handler
    {
        private readonly IServer _server = server;
        private readonly ILogger<GetServicesHandler> _logger = logger;
        private readonly XDocument _body = body;
        private const string CommonUrl = "http://127.0.0.1:5120/eamuse";
        private string listeningAddress = "http://127.0.0.1:5120";

        public override void Configure()
        {
            Module("services");
            Method("get");
        }

        public override Task<XDocument> HandleAsync(string model)
        {
            var services = CreateCoreServicesElement();
            var document = new XDocument(new XElement("response", services));
            return Task.FromResult(document);
        }

        private static XElement CreateCoreServicesElement()
        {
            var services = new XElement("services",
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
                services.Add(new XElement("item",
                    new XAttribute("name", service),
                    new XAttribute("url", $"{CommonUrl}/{service}")));
            }

            services.Add(new XElement("item", new XAttribute("name", "ntp"),
                new XAttribute("url", "ntp://pool.ntp.org/")));

            services.Add(new XElement("item", new XAttribute("name", "keepalive"),
                new XAttribute("url",
                    "http://127.0.0.1:8083/keepalive?pa=127.0.0.1&ia=127.0.0.1&ga=127.0.0.1&ma=127.0.0.1&t1=2&t2=10")));

            return services;
        }

        private static void AddKfcServices(XElement services)
        {
            const string sdvxurl = $"{CommonUrl}";
            string[] kfcServices = [
        "local", "local2", "lobby", "slocal", "slocal2", "sglocal", "sglocal2", "lab", "globby",
        "slobby", "sglobby"
    ];

            foreach(var service in kfcServices)
                services.Add(new XElement("item", new XAttribute("name", service), new XAttribute("url", sdvxurl)));
        }

        private static void AddMdxServices(XElement services)
        {
            const string mdxurl = $"{CommonUrl}";
            string[] mdxServices = [
        "local", "local2", "lobby", "slocal", "slocal2", "sglocal", "sglocal2", "lab", "globby",
        "slobby", "sglobby"
    ];

            foreach(var service in mdxServices)
                services.Add(new XElement("item", new XAttribute("name", service), new XAttribute("url", mdxurl)));
        }

        private static void AddM39Services(XElement services)
        {
            const string m39url = $"{CommonUrl}";
            string[] m39Services = [
        "local", "local2", "lobby", "slocal", "slocal2", "sglocal", "sglocal2", "lab", "globby",
        "slobby", "sglobby"
    ];

            foreach(var service in m39Services)
                services.Add(new XElement("item", new XAttribute("name", service), new XAttribute("url", m39url)));
        }

    }
}
