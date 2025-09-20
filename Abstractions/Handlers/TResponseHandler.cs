using System;
using System.Threading.Tasks;

namespace Abstractions.Handlers;

public abstract class ResponseHandler<TResponse>
{
    public string HandlerModule { get; set; } = "";
    public string HandlerMethod { get; set; } = "";
    public string? HandlerGameCode { get; set; }
    public string? HandlerMinVer { get; set; }
    public string? HandlerMaxVer { get; set; }

    public virtual void Configure() { throw new NotImplementedException("Configure method not implemented."); }

    protected void Module(string module)
    {
        this.HandlerModule = module;
    }

    protected void Method(string method)
    {
        this.HandlerMethod = method;
    }

    protected void GameCode(string gameCode)
    {
        this.HandlerGameCode = gameCode;
    }

    protected void MinVer(string minVer)
    {
        this.HandlerMinVer = minVer;
    }
    
    public virtual TResponse Handle(string model) { throw new NotImplementedException("Handle method not implemented."); }
    public virtual Task<TResponse> HandleAsync(string model) { throw new NotImplementedException("Handle method not implemented."); }
}