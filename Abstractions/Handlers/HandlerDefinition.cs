using System;

namespace Abstractions.Handlers;

public sealed class HandlerDefinition(Type handlerType, Type requestType, Type responseType)
{
    public Type HandlerType { get; init; } = handlerType;
    public Type RequestType { get; init; } = requestType;
    public Type ResponseType { get; init; } = responseType;
    public string HandlerModule { get; internal set; } = "";
    public string HandlerMethod { get; internal set; } = "";
    public string? HandlerGameCode { get; internal set; }
    public string? HandlerMinVer { get; internal set; }
    public string? HandlerMaxVer { get; internal set; }
}