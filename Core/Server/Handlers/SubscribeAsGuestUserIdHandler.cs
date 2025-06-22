using System.Xml.Linq;
using Abstractions.Handlers;

namespace Server.Handlers
{
    public class SubscribeAsGuestUserIdHandler(XDocument body): Handler
    {
        private readonly XDocument _body = body;

        public override void Configure()
        {
            Module("userid");
            Method("subscribeAsGuest");
        }

        public override XDocument Handle(string model)
        {
            var subscribeAsGuest = new XElement("userid", new XAttribute("status", "0"));

            var document = new XDocument(new XElement("response", subscribeAsGuest));

            return document;
        }
    }
}
