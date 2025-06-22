using System.Xml.Linq;

namespace Server.Handlers.MDX._2014032700
{
    public class TaxInfoGameHandler(XDocument body) : Handler
    {
        private readonly XDocument _body = body;

        public override void Configure()
        {
            Module("game");
            Method("tax_info");
            GameCode("MDX");
            MinVer("2014032700");
            MaxVer("2014032700");
        }

        public override XDocument Handle(string model)
        {
            var game = _body.Root?.Element("game")!;
            var recorderGame = new XElement("game", new XAttribute("status", "0"));
            var document = new XDocument(new XElement("response", recorderGame));
            return document;
        }
    }
}
