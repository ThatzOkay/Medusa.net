namespace Server.Models.Request;

public class RegisterRequest
{
    public required string KonamiId { get; set; }
    public required string Pin { get; set;  }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}
