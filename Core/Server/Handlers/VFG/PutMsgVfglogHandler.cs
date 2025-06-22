using System.Xml.Linq;
using Abstractions.Handlers;

namespace Server.Handlers.VFG
{
    public class PutMsgVfglogHandler(XDocument body) : Handler
    {
        private readonly XDocument _body = body;

        public override void Configure()
        {
            Module("vfglog");
            Method("put_msg");
        }

        public override XDocument Handle(string model)
        {
            var vfglog = new XElement("vfglog",
                new XAttribute("status", "0")
            );

            var document = new XDocument(
                new XElement("response",
                    vfglog
                )
            );

            return document;
        }
    }
}
