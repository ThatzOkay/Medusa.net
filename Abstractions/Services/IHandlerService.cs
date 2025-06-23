using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Abstractions.Services
{
    public interface IHandlerService
    {
        public List<Type> Handlers { get; set; }
        Task<XDocument> Handle(string model, string module, string method, XDocument body);

    }
}
