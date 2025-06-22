using Server.Services;
using System.Xml.Linq;
using Abstractions.Handlers;

namespace Server.Handlers.LA9
{
    public class CheckoutEaCharge(ISessionService sessionService, XDocument body): Handler
    {
        private readonly ISessionService _sessionService = sessionService;
        private readonly XDocument _body = body;
        public override void Configure()
        {
            Module("eacharge");
            Method("checkout");
        }
        public override XDocument Handle(string model)
        {
            var eaCharge = _body.Root?.Element("eacharge")!;
            var sessionId = eaCharge.Element("sessid")!.Value!;

            _sessionService.removeSession(sessionId);

            var checkoutCharge = new XElement("eacharge", new XAttribute("status", "0"));
            var document = new XDocument(new XElement("response", checkoutCharge));
            return document;
        }
    }
}
