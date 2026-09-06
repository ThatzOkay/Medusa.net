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

    public virtual TResponse Handle(TRequest req, GameModel model) { throw new NotImplementedException("Handle method not implemented."); }
    public virtual Task<TResponse> HandleAsync(TRequest req, GameModel model) { throw new NotImplementedException("Handle method not implemented."); }
}

public abstract class HandlerWithoutRequest<TResponse> : Handler<EmptyRequest, TResponse>
{
    public virtual TResponse Handle(GameModel model) { throw new NotImplementedException("Handle method not implemented."); }
    public virtual Task<TResponse> HandleAsync(GameModel model) { throw new NotImplementedException("Handle method not implemented."); }
    
    public sealed override Task<TResponse> HandleAsync(EmptyRequest _, GameModel model)
        => HandleAsync(model);
    
    public sealed override TResponse Handle(EmptyRequest _, GameModel model)
     => Handle(model);
}