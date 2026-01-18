namespace Server.Models.Response;

public class ValidateCardResponse
{
    public required string Message { get; set; }
    public bool Success { get; set; } = false;
    }
