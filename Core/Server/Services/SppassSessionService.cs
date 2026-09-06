using Microsoft.Extensions.Caching.Memory;

namespace Server.Services;

public class SppassSessionService(IMemoryCache cache) : ISppassSessionService
{
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(3);

    private readonly IMemoryCache _cache = cache;

    public SppassSession Open()
    {
        var session = new SppassSession(GenerateToken(), DateTimeOffset.UtcNow.Add(SessionLifetime));
        _cache.Set(session.Token, session, session.ExpiresAt);
        return session;
    }

    public SppassSession? GetSession(string token)
        => _cache.TryGetValue(token, out SppassSession? session) ? session : null;

    public bool TryApprove(string token, string cardType, string cardId)
    {
        var session = GetSession(token);
        if (session is null)
        {
            return false;
        }

        session.CardType = cardType;
        session.CardId = cardId;
        return true;
    }

    public void Close(string token)
    {
        _cache.Remove(token);
    }

    private static string GenerateToken()
        => Guid.NewGuid().ToString().Replace("-", "").ToUpper();
}

public record SppassSession(string Token, DateTimeOffset ExpiresAt)
{
    public string? CardType { get; set; }
    public string? CardId { get; set; }
    public bool IsApproved => CardId is not null;
}
