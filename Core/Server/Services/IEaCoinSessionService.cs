namespace Server.Services;

public interface IEaCoinSessionService
{
    public void AddSession(int userId, string sessionId, string refId);
    public EaCoinSession AddSession(int userId, string cardId);
    public EaCoinSession? GetSession(string sessionId);
    public void RemoveSession(string sessionId);
}