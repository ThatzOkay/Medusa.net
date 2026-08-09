using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Abstractions.Entities
{
    public class User: IdentityUser<int>
    {
        public int PaseliAmount { get; set; } = 0;
        public string Pin { get; set; } = "0000";

        public List<Card> Cards { get; init; } = null!;
    }
}
