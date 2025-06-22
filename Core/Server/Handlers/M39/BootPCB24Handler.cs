using System.Xml.Linq;
using Abstractions.Handlers;

namespace Server.Handlers.M39
{
    public class BootPCB24Handler(XDocument body) : Handler
    {
        private readonly XDocument _body = body;

        public override void Configure()
        {
            Module("pcb24");
            Method("boot");
        }

        public override XDocument Handle(string model)
        {
            var pcb24 = new XElement("pcb24",
                new XAttribute("status", "0"));

            var document = new XDocument(
                new XElement("response",
                    pcb24
                )
            );

            return document;
        }
    }
}
