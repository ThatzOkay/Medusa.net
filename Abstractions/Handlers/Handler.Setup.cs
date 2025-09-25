using System;

namespace Abstractions.Handlers;

public abstract partial class Handler<TRequest, TResponse> where TRequest : notnull
{
    static readonly Type _tRequest = typeof(TRequest);
    static readonly Type _tResponse = typeof(TResponse);

    public override void Module(string moduleName)
        => Definition.HandlerModule = moduleName;

    public override void Method(string method)
        => Definition.HandlerMethod = method;

    public override void GameCode(string code)
        => Definition.HandlerGameCode = code;

    public override void MinVer(string minVer)
        => Definition.HandlerMinVer = minVer;

    public override void MaxVer(string maxVer)
        => Definition.HandlerMaxVer = maxVer;
}