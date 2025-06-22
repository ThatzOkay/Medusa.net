using System.Xml.Linq;
using Abstractions.Handlers;
using Server.Attributes;

namespace Server.Handlers.Common
{
    [Handler("message", "get")]
    public class GetMessageHandler(XDocument body) : IHandler
    {
        private readonly XDocument _body = body;

        public Task<XDocument> HandleAsync(string model)
        {
            var message = new XElement("message",
                new XAttribute("expire", "300"),
                new XAttribute("status", "0"));

            var document = new XDocument(
                new XElement("response", message));

            return Task.FromResult(document);
        }
    }

}
