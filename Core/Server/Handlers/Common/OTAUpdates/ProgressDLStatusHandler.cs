using System.Xml.Linq;
using Abstractions.Handlers;
using Server.Attributes;

namespace Server.Handlers.Common.OTAUpdates
{
    [Handler("dlstatus", "progress")]
    public class ProgressDLStatusHandler(XDocument body) : IHandler
    {
        private readonly XDocument _body = body;

        public Task<XDocument> HandleAsync(string model)
        {
            var dlStatus = new XElement("dlstatus", new XAttribute("status", "0"));

            var document = new XDocument(new XElement("response", dlStatus));

            return Task.FromResult(document);
        }
    }

}
