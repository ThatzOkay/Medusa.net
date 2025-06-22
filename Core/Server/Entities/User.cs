using Microsoft.AspNetCore.Identity;

namespace Server.Entities
{
    public class User: IdentityUser<int>
    {
        public int PaseliAmount { get; set; } = 0;
        public required int Pin { get; set; }

        public List<Card> Cards { get; set; } = default!;
    }
}
