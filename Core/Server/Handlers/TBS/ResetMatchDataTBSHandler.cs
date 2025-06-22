using System.Xml.Linq;
using Abstractions.Handlers;

namespace Server.Handlers.TBS
{
    public class ResetMatchDataTBSHandler(XDocument body) : Handler
    {
        private readonly XDocument _body = body;

        public override void Configure()
        {
            Module("tbs");
            Method("resetmatchdata");
        }

        public override XDocument Handle(string model)
        {
            var tbs = new XElement("tbs",
                new XAttribute("status", "0")
            );

            var document = new XDocument(
                new XElement("response",
                    tbs
                )
            );

            return document;
        }
    }
}
