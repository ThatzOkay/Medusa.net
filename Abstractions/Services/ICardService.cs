
using System.Threading.Tasks;
using Abstractions.Entities;

namespace Abstractions.Services
{
    public interface ICardService
    {
        string ConvertUidToKonamiId(string uid);
        string ConvertKonamiIdToUid(string konamiId);
        Task<Card?> FindByKonamiId(string konamiId);
        Task<Card?> FindByCardId(string cardId);
    }
}
