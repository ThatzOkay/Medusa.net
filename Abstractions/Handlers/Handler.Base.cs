using System;

namespace Abstractions.Handlers;

public abstract partial class BaseHandler: IHandler
{
    public HandlerDefinition Definition { get; set; } = null!;
    public virtual void Configure() { throw new NotImplementedException("Configure method not implemented."); }
    
    public virtual void Module(string module) { throw new NotImplementedException("Module not implemented."); }
    public virtual void Method(string method) { throw new NotImplementedException("Method not implemented."); }
    public virtual void GameCode(string code) { throw new NotImplementedException("GameCode not implemented."); }
    public virtual void MinVer(string minVer) { throw new NotImplementedException("MinVer not implemented."); }
    public virtual void MaxVer(string maxVer) { throw new NotImplementedException("MaxVer not implemented."); }
}