using System.Threading.Tasks;
using System.Xml.Linq;

namespace Abstractions.Handlers
{
    public interface IHandler
    {
        Task<XDocument> HandleAsync(string model);
    }
}
