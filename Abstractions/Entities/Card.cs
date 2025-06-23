using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Abstractions.Entities
{
    public class Card
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public required string KonamiId { get; set; }
        public required string RawId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = default!;
    }
}
