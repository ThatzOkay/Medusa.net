namespace Server.Services;

public interface IEaCoinSessionService
{
    public void AddSession(long userId, string sessionId, string refId);
    public EaCoinSession AddSession(long userId, string cardId);
    public EaCoinSession? GetSession(string sessionId);
    public void RemoveSession(string sessionId);
}