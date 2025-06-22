using Server.Handlers.Boot;
using Server.Attributes;
using System.Xml.Linq;
using Abstractions.Handlers;

namespace Server.Handlers.Common
{
    [Handler("pcbtracker", "alive")]
    public class AlivePcbTrackerHandler(ILogger<GetServicesHandler> logger, XDocument body) : IHandler
    {
        private readonly ILogger<GetServicesHandler> _logger = logger;
        private readonly XDocument _body = body;

        public Task<XDocument> HandleAsync(string model)
        {
            var pcbTracker = new XElement("response",
                new XElement("pcbtracker",
                    new XAttribute("status", "0"),
                    new XAttribute("expire", "1200"),
                    new XAttribute("ecenable", "1"),
                    new XAttribute("eclimit", "0"),
                    new XAttribute("limit", "0"),
                    new XAttribute("time", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())));

            var document = new XDocument(new XElement("response", pcbTracker));

            return Task.FromResult(document);
        }
    }

}
