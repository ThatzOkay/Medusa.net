namespace Server.Models.Request;

public class UpdatePinRequest
{
    public required string OldPin { get; set; }
    public required string NewPin { get; set; }
}
