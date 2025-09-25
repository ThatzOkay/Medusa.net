namespace Server.Models.Request
{
    public class AmusementRequest
    {
        public required string Model { get; init; }
        public required string Module { get; set; }
        public required string Method { get; set; }

    }
}
