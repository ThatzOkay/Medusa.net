using Server.Services;
using System.Xml.Linq;
using Abstractions.Handlers;
using Abstractions.Services;

namespace Server.Handlers.Common.Card
{
    public class InquireCardManagmentHandlerInquireCardManagmentHandler(IPluginService pluginService, ICardService cardService, XDocument body) : Handler
    {
        public override void Configure()
        {
            Module("cardmng");
            Method("inquire");
        }

        public override async Task<XDocument> HandleAsync(string model)
        {
            var rootCall = body.Root;
            
            var splitModel = model.Split(':');
            var gameCode = splitModel[0];
            
            var plugin = pluginService.FindPlugin(gameCode);
            
            var cardId = rootCall?.Element("cardmng")?.Attribute("cardid")?.Value;

            if(string.IsNullOrEmpty(cardId))
            {
                var noCardId = new XElement("cardmng", new XAttribute("status", "111"));
                return new XDocument(new XElement("response", noCardId));
            }

            var konamiId = cardService.ConvertUidToKonamiId(cardId);
            if(string.IsNullOrEmpty(konamiId))
            {
                var noKonamiId = new XElement("cardmng", new XAttribute("status", "111"));
                return new XDocument(new XElement("response", noKonamiId));
            }

            var exisitngCard = await cardService.FindByKonamiId(konamiId);

            if(exisitngCard == null)
            {
                var noCard = new XElement("cardmng", new XAttribute("status", "112"));
                return new XDocument(new XElement("response", noCard));
            }

            if (plugin is null)
            {
                var noPlugin = new XElement("cardmng", new XAttribute("status", "113"));
                return new XDocument(new XElement("response", noPlugin));           
            }

            var profileExists = await plugin.DoesProfileExist(cardId);
            
            //TODO get actual user profile id
            var userIdFlag = profileExists ? "1" : "0";
            var extIdFlag = profileExists ? "1" : "0";
            var newFlag = profileExists ? "0" : "1";
            
            var cardManagment = new XElement("cardmng", new XAttribute("binded", "0"), new XAttribute("dataid", konamiId),
                new XAttribute("ecflag", "1"), new XAttribute("expired", "0"), new XAttribute("newflag", newFlag), new XAttribute("extidflag", extIdFlag),
                new XAttribute("refid", konamiId), new XAttribute("status", "0"), new XAttribute("useridflag", userIdFlag));

            var document = new XDocument(new XElement("response", cardManagment));

            return document;
        }
    }
}
