using Microsoft.AspNetCore.Identity;
using System.Xml.Linq;
using Abstractions.Entities;
using Abstractions.Handlers;
using Abstractions.Services;

namespace Server.Handlers.Common.Card
{
    public class GetRefIdCardManagmentHandler(ICardService cardService, UserManager<User> userManager, XDocument body) : Handler
    {
        private readonly XDocument _body = body;
        private readonly ICardService _cardService = cardService;
        private readonly UserManager<User> _userManager = userManager;

        public override void Configure()
        {
            Module("cardmng");
            Method("getrefid");
        }

        public override async Task<XDocument> HandleAsync(string model)
        {
            var rootCall = _body.Root;
            var cardManagement = rootCall?.Element("cardmng")!;
            var cardId = cardManagement.Attribute("cardid")?.Value!;
            var password = cardManagement.Attribute("passwd")?.Value!;

            var konamiId = _cardService.ConvertUidToKonamiId(cardId);

            var card = new Abstractions.Entities.Card { RawId = cardId, KonamiId = konamiId };
            var user = new User { Pin = int.Parse(password), UserName = konamiId, Cards = [card] };

            await _userManager.CreateAsync(user);

            var getRefId = new XElement("cardmng", new XAttribute("status", "0"), new XAttribute("refid", konamiId), new XAttribute("dataid", konamiId));

            var document = new XDocument(new XElement("response", getRefId));

            return document;
        }
    }
}
