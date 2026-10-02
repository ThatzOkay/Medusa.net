namespace Server.Services;

/// <summary>
/// Holds the server's externally-reachable base URL, populated on ApplicationStarted
/// using the same logic that logs "Accessible at:" to the console.
/// </summary>
public sealed class MedusaBaseUrl
{
    public string Value { get; set; } = "";
}
