using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Abstractions.Handlers;

public abstract partial class Handler<TRequest, TResponse>: BaseHandler where TRequest : notnull
{

    // protected void Module(string module)
    // {
    //     this.HandlerModule = module;
    // }
    //
    // protected void Method(string method)
    // {
    //     this.HandlerMethod = method;
    // }
    //
    // protected void GameCode(string gameCode)
    // {
    //     this.HandlerGameCode = gameCode;
    // }
    //
    // protected void MinVer(string minVer)
    // {
    //     this.HandlerMinVer = minVer;
    // }
    //
    // protected void MaxVer(string maxVer)
    // {
    //     this.HandlerMaxVer = maxVer;
    // }

    public virtual TResponse Handle(TRequest req, string model) { throw new NotImplementedException("Handle method not implemented."); }
    public virtual Task<TResponse> HandleAsync(TRequest req, string model) { throw new NotImplementedException("Handle method not implemented."); }
}

public abstract class HandlerWithoutRequest<TResponse> : Handler<EmptyRequest, TResponse>
{
    public virtual TResponse Handle(string model) { throw new NotImplementedException("Handle method not implemented."); }
    public virtual Task<TResponse> HandleAsync(string model) { throw new NotImplementedException("Handle method not implemented."); }
    
    public sealed override Task<TResponse> HandleAsync(EmptyRequest _, string model)
        => HandleAsync(model);
    
    public sealed override TResponse Handle(EmptyRequest _, string model)
     => Handle(model);
}