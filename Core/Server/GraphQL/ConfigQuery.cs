using Server.Utils;

namespace Server.GraphQL;

// Lets the frontend read server-side config values instead of hardcoding
// them - starting with the public base URL sppass.open already builds QR
// links from (see ServerAddress + Handlers/Sppass/SppassOpenHandler).
[ExtendObjectType(OperationTypeNames.Query)]
public class ConfigQuery
{
    public string PublicUrl() => ServerAddress.PublicUrl;
}
