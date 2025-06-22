using Microsoft.AspNetCore.Identity;
using Server.Entities;
using Server.Services;
using System.Xml.Linq;
using Abstractions.Handlers;

namespace Server.Handlers.LA9
{
    public class ChargeEaChargeHandler(ICardService cardService, ISessionService sessionService, UserManager<User> userManager, XDocument body): Handler
    {
        private readonly XDocument _body = body;
        private readonly ICardService _cardService = cardService;
        private readonly ISessionService _sessionService = sessionService;
        private readonly UserManager<User> _userManager = userManager;

        public override void Configure()
        {
            Module("eacharge");
            Method("charge");
        }
        public override async Task<XDocument> HandleAsync(string model)
        {
            var rootCall = _body.Root;
            var charge = rootCall?.Element("eacharge")!;
            var amount = int.Parse(charge.Element("chargenum")?.Value!) * 1000;
            var session = charge.Element("sessid")?.Value!;

            var foundSession = _sessionService.GetSession(session);

            var card = await _cardService.FindByCardId(foundSession.refId)!;
            card.User.PaseliAmount += amount;

            await _userManager.UpdateAsync(card.User);

            var balance = new XElement("balance", card.User.PaseliAmount, new XAttribute("__type", "s32"));
            var chargeDailyLimit = new XElement("chargedailylimit", 100000, new XAttribute("__type", "s32"));
            var chargeDaily = new XElement("chargedaily", 0, new XAttribute("__type", "s32"));
            var chargepoint = new XElement("chargepoint", amount, new XAttribute("__type", "s32"));

            var eaCharge = new XElement("charge", balance, chargeDailyLimit, chargeDaily, chargepoint, new XAttribute("status", "0"));
            var document = new XDocument(new XElement("response", eaCharge));
            return document;
        }
    }
}
