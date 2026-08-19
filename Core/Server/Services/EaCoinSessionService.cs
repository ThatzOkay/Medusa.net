namespace Server.Services;

public class EaCoinSessionService : IEaCoinSessionService
{
    private readonly List<EaCoinSession> sessions = [];
    public void AddSession(long userId, string sessionId, string refId)
    {
        sessions.Add(new EaCoinSession(sessionId, userId, refId));
    }

    public EaCoinSession AddSession(long userId, string cardid)
    {
        var sessionId = GenerateSessionId();
        var session = new EaCoinSession(sessionId, userId, cardid);
        sessions.Add(session);
        return session;
    }

    public EaCoinSession? GetSession(string sessionId)
    {
        return sessions.Find(s => s.SessionId == sessionId);
    }

    public void RemoveSession(string sessionId)
    {
        sessions.Remove(sessions.Find(s => s.SessionId == sessionId));
    }

    private string GenerateSessionId()
    {
        return Guid.NewGuid().ToString().Replace("-", "").ToUpper();
    }
}

public record EaCoinSession(string SessionId, long UserId, string RefId)
{
    public bool IsCharging { get; set; } = false;
}