using Abstractions.SerializationTypes;

namespace Abstractions.Utils;

public static class XrpcIntExtensions
{
    public static bool ToBool(this XrpcInt self)
    {
        return self != 0;
    }
}
