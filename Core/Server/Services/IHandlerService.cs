using System.Xml.Linq;

namespace Server.Services
{
    public interface IHandlerService
    {
        public List<Type> Handlers { get; set; }
        Task<XDocument> Handle(string model, string module, string method, XDocument body);

    }
}
