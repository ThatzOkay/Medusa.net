namespace Server.Services
{
    public interface ISessionService
    {
        public void AddSession(string sessionId, string refId);
        string AddSession(string cardid);
        public (string sessionId, string refId) GetSession(string sessionId);
        public void removeSession(string sessionId);
    }
}
