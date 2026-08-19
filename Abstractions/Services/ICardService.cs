
using System.Collections.Generic;
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
        Task<Card?> FindById(long parse);
        Task<bool> Exists(string konamiId);
        Task<bool> ValidatePinAsync(string konamiId, string pin);
        Task<bool> IsRegistered(string konamiId, string pin);
        Task<List<Card>> GetCardsByUserIdAsync(long userId);
    }
}
