using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Abstractions.Entities
{
    public class User: IdentityUser<int>
    {
        public int PaseliAmount { get; set; } = 0;
        public int Pin { get; init; }

        public List<Card> Cards { get; init; } = null!;
    }
}
