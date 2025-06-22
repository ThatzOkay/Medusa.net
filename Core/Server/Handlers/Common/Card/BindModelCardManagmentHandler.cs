using System.Xml.Linq;

namespace Server.Handlers.Common.Card
{
    public class BindModelCardManagmentHandler(XDocument body) : Handler
    {
        private readonly XDocument _body = body;
        public override void Configure()
        {
            Module("cardmng");
            Method("bindmodel");
        }
        public override XDocument Handle(string model)
        {
            var game = _body.Root?.Element("cardmng")!;
            var cardmng = new XElement("cardmng", new XAttribute("status", "0"));
            var document = new XDocument(new XElement("response", cardmng));
            return document;
        }
    }
}
