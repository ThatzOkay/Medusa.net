using Server.Services;
using System.Xml.Linq;
using Abstractions.Handlers;

namespace Server.Handlers.Common.Card
{
    public class InquireCardManagmentHandlerInquireCardManagmentHandler(ICardService cardService, XDocument body) : Handler
    {
        private readonly XDocument _body = body;
        private readonly ICardService _cardService = cardService;

        public override void Configure()
        {
            Module("cardmng");
            Method("inquire");
        }

        public override async Task<XDocument> HandleAsync(string model)
        {
            var cardId = _body.Root?.Element("cardmng")?.Attribute("cardid")?.Value;

            if(string.IsNullOrEmpty(cardId))
            {
                var noCardId = new XElement("cardmng", new XAttribute("status", "111"));
                return new XDocument(new XElement("response", noCardId));
            }

            var konamiId = _cardService.ConvertUidToKonamiId(cardId);
            if(string.IsNullOrEmpty(konamiId))
            {
                var noKonamiId = new XElement("cardmng", new XAttribute("status", "111"));
                return new XDocument(new XElement("response", noKonamiId));
            }

            var exisitngCard = await _cardService.FindByKonamiId(konamiId);

            if(exisitngCard == null)
            {
                var noCard = new XElement("cardmng", new XAttribute("status", "112"));
                return new XDocument(new XElement("response", noCard));
            }

            var cardManagment = new XElement("cardmng", new XAttribute("binded", "0"), new XAttribute("dataid", konamiId),
                new XAttribute("ecflag", "1"), new XAttribute("expired", "0"), new XAttribute("newflag", "1"), new XAttribute("extidflag", "1"),
                new XAttribute("refid", konamiId), new XAttribute("status", "0"), new XAttribute("useridflag", "1"));

            var document = new XDocument(new XElement("response", cardManagment));

            return document;
        }
    }
}
