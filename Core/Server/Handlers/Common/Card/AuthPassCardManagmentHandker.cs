using Server.Services;
using System.Xml.Linq;
using Abstractions.Handlers;
using Abstractions.Services;

namespace Server.Handlers.Common.Card
{
    public class AuthPassCardManagmentHandker(ICardService cardService, XDocument body) : Handler
    {
        private readonly XDocument _body = body;
        private readonly ICardService _cardService = cardService;

        public override void Configure()
        {
            Module("cardmng");
            Method("authpass");
        }

        public override async Task<XDocument> HandleAsync(string model)
        {
            var rootCall = _body.Root;
            var cardManagement = rootCall?.Element("cardmng")!;
            var referanceId = cardManagement.Attribute("refid")?.Value!;
            var password = cardManagement.Attribute("pass")?.Value!;

            var card = await _cardService.FindByKonamiId(referanceId);

            if(card == null)
            {
                var noCard = new XElement("cardmng", new XAttribute("status", "116"));
                return new XDocument(new XElement("response", noCard));
            }

            if(card.User.Pin != int.Parse(password))
            {
                var wrongPassword = new XElement("cardmng", new XAttribute("status", "116"));
                return new XDocument(new XElement("response", wrongPassword));
            }

            var authPass = new XElement("cardmng", new XAttribute("status", "0"));

            var document = new XDocument(new XElement("response", authPass));

            return document;
        }
    }
}
