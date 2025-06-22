using System.Xml.Linq;
using Abstractions.Handlers;

namespace Server.Handlers.VFG
{
    public class ServiceListVfgacHandler(XDocument body) : Handler
    {
        private readonly XDocument _body = body;

        public override void Configure()
        {
            Module("vfgac");
            Method("service_list");
        }

        public override XDocument Handle(string model)
        {

            var serviceUrl = new XElement("service_url", "http://127.0.0.1:5120/eamuse");
            var service1 = new XElement("service", new XAttribute("service", "http://127.0.0.1:5120/eamuse"), new XAttribute("mode", "test"), "http://127.0.0.1:5120/eamuse");
            var services = new XElement("services", service1);

            var vfgac = new XElement("vfgac",
                new XAttribute("status", "0"),
                serviceUrl,
                services
            );

            var document = new XDocument(
                new XElement("response",
                    vfgac
                )
            );

            return document;
        }
    }
}
