using System.Xml.Linq;
using Abstractions.Handlers;
using Server.Attributes;

namespace Server.Handlers.Common
{
    [Handler("pcbevent", "put")]
    public class PutPcbEventHandler(ILogger<PutPcbEventHandler> logger, XDocument body) : IHandler
    {
        private readonly XDocument _body = body;
        private readonly ILogger<PutPcbEventHandler> _logger = logger;

        public Task<XDocument> HandleAsync(string model)
        {
            var rootCallElement = _body.Root;

            var itemElement = rootCallElement?.Element("pcbevent")?.Element("item");

            var timeElement = itemElement?.Element("time");
            string time = timeElement is not null ? UnixTimeStampToDateTime(double.Parse(timeElement.Value)).ToString() : "";

            _logger.LogInformation("PCBEvent: tag={Tag} srcid={SrcId} model={Model} name={Name} time={Time}",
                rootCallElement?.Attribute("tag")?.Value ?? "",
                rootCallElement?.Attribute("srcid")?.Value ?? "",
                rootCallElement?.Attribute("model")?.Value ?? "",
                itemElement?.Element("name")?.Value ?? "",
                time
            );

            var pcbEvent = new XElement("response",
                new XElement("pcbevent",
                    new XAttribute("status", "0")));

            var document = new XDocument(new XElement("response", pcbEvent));
            return Task.FromResult(document);
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }

}
