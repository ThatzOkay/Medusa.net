namespace Server.Services;

public interface ISppassSessionService
{
    SppassSession Open();
    SppassSession? GetSession(string token);
    bool TryApprove(string token, string cardType, string cardId);
    void Close(string token);
}
