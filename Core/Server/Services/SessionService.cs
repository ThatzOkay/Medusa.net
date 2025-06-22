namespace Server.Services
{
    public class SessionService : ISessionService
    {
        private readonly List<(string sessionId, string refId)> sessions = [];
        public void AddSession(string sessionId, string refId)
        {
            sessions.Add((sessionId, refId));
        }

        public string AddSession(string cardid)
        {
            var sessionId = GenerateSessionId();
            sessions.Add((sessionId, cardid));
            return sessionId;
        }

        public (string sessionId, string refId) GetSession(string sessionId)
        {
            return sessions.Find(s => s.sessionId == sessionId);
        }

        public void removeSession(string sessionId)
        {
            sessions.Remove(sessions.Find(s => s.sessionId == sessionId));
        }

        private string GenerateSessionId()
        {
            return Guid.NewGuid().ToString().Replace("-", "").ToUpper();
        }
    }
}
