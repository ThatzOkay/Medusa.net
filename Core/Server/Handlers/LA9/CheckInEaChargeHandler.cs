using Server.Services;
using System.Xml.Linq;
using Abstractions.Handlers;

namespace Server.Handlers.LA9
{
    public class CheckInEaChargeHandler(ISessionService sessionService, ICardService cardService, XDocument body): Handler
    {
        private readonly XDocument _body = body;
        private readonly ISessionService _sessionService = sessionService;
        private readonly ICardService _cardService = cardService;

        public override void Configure()
        {
            Module("eacharge");
            Method("checkin");
        }
        public override async Task<XDocument> HandleAsync(string model)
        {
            var rootCall = _body.Root;
            var eaCharge = rootCall?.Element("eacharge")!;
            var cardId = eaCharge.Element("cardid")?.Value!;

            var sessionId = _sessionService.AddSession(cardId);

            var card = await _cardService.FindByCardId(cardId);

            var userPaseli = card!.User.PaseliAmount;

            var sessseq = new XElement("sessseq", 0, new XAttribute("__type", "s32"));
            var acname = new XElement("acname", "DUMMY_NAME", new XAttribute("__type", "str"));
            var balance = new XElement("balance", userPaseli, new XAttribute("__type", "s32"));
            var buyablep = new XElement("buyablep", 100000, new XAttribute("__type", "s32"));
            var sessionIdXElement = new XElement("sessid", sessionId, new XAttribute("__type", "str"));
            var eacharge = new XElement("eacharge", sessseq, acname, balance, buyablep, sessionIdXElement, new XAttribute("status", "0"));
            var document = new XDocument(new XElement("response", eacharge));
            return document;
        }
    }
}
