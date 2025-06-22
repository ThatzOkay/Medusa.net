using System.Xml.Linq;
using Abstractions.Handlers;
using Server.Attributes;

namespace Server.Handlers.Common.OTAUpdates
{
    [Handler("package", "list")]
    public class ListPackageHandler(XDocument body) : IHandler
    {
        private readonly XDocument _body = body;

        public Task<XDocument> HandleAsync(string model)
        {
            var package = new XElement("package", new XAttribute("expire", "600"), new XAttribute("status", "0"));

            var document = new XDocument(new XElement("response", package));

            return Task.FromResult(document);
        }
    }

}
