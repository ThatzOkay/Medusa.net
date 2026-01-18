namespace Server.Models.Request;

public class ValidateCardRequest
{
    public required string CardId { get; set; }
    public required string Pin { get; set; }
}
