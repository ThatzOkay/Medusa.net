using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Hosting.Server;
using System.Xml.Linq;
using Abstractions.Handlers;
using Server.Attributes;

namespace Server.Handlers.Common
{
    [Handler("facility", "get")]
    public class GetFacilityHandler(IServer server, XDocument body) : IHandler
    {
        private readonly IServer _server = server;
        private IServerAddressesFeature? serverAddressesFeature => _server.Features.Get<IServerAddressesFeature>();
        private readonly XDocument _body = body;

        public Task<XDocument> HandleAsync(string model)
        {
            var port = "8083";

            var facility = new XElement("facility",
                new XAttribute("status", "0"));

            var location = new XElement("location",
                new XElement("id", "00000000", new XAttribute("__type", "str")),
                new XElement("country", "US", new XAttribute("__type", "str")),
                new XElement("region", "NA", new XAttribute("__type", "str")),
                new XElement("name", "Medusa", new XAttribute("__type", "str")),
                new XElement("type", 0, new XAttribute("__type", "u8")));

            var line = new XElement("line",
                new XElement("id", "1", new XAttribute("__type", "str")),
                new XElement("class", 0, new XAttribute("__type", "u8")));

            var portfw = new XElement("portfw",
                new XElement("globalip", "127.0.0.1", new XAttribute("__type", "ip4")),
                new XElement("globalport", "5246", new XAttribute("__type", "u16")),
                new XElement("privateport", "5246", new XAttribute("__type", "u16")));

            var _public = new XElement("public",
                new XElement("flag", 1, new XAttribute("__type", "u8")),
                new XElement("name", "Medusa", new XAttribute("__type", "str")),
                new XElement("latitude", "0.0", new XAttribute("__type", "str")),
                new XElement("longitude", "0.0", new XAttribute("__type", "str")));

            var share = new XElement("share",
                new XElement("eacoin",
                    new XElement("notchamount", 3000, new XAttribute("__type", "s32")),
                    new XElement("notchcount", 3, new XAttribute("__type", "s32")),
                    new XElement("supplylimit", 100000, new XAttribute("__type", "s32"))),
                new XElement("url",
                    new XElement("eapass", "http://eagate.573.jp", new XAttribute("__type", "str")),
                    new XElement("arcadefan", "http://eagate.573.jp", new XAttribute("__type", "str")),
                    new XElement("konaminetdx", "http://eagate.573.jp", new XAttribute("__type", "str")),
                    new XElement("konamiid", "http://eagate.573.jp", new XAttribute("__type", "str")),
                    new XElement("eagate", "http://eagate.573.jp", new XAttribute("__type", "str"))));

            facility.Add(location, line, portfw, _public, share);

            var document = new XDocument(new XElement("response", facility));
            
            Console.WriteLine(document.ToString());
            
            return Task.FromResult(document);
        }
    }

}
