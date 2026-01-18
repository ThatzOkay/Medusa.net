using Abstractions.Entities;
using System.Threading.Tasks;

namespace Abstractions.Services
{
    public interface IUserService
    {
        Task<User?> GetUserByKonamiId(string konamiId);
    }
}
