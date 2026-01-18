using Abstractions.Entities;
using Abstractions.Services;
using Microsoft.EntityFrameworkCore;

namespace Server.Services
{
    public class UserService(AppDbContext context) : IUserService
    {
        public Task<User?> GetUserByKonamiId(string konamiId)
        {
            return context.Users
                .Include(u => u.Cards)
                .FirstOrDefaultAsync(u => u.Cards.Any(c => c.KonamiId == konamiId));
        }
    }
}
