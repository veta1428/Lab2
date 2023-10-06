using Microsoft.Extensions.Options;
using Server.Models;
using Server.Options;
using System.Collections.Concurrent;
using SessionOptions = Server.Options.SessionOptions;

namespace Server.Managers;

public class SessionManager
{
    private ConcurrentDictionary<Guid, Session> _sessions = new();
    private SessionOptions _options;

    public SessionManager(IOptions<SessionOptions> options)
    {
        _options = options.Value;
    }

    public Session? TryGetSession(Guid sessionId)
    {
        Session? session = null;
        _sessions.TryGetValue(sessionId, out session);

        if (session is null)
            return null;

        if (session.Expires <= DateTime.UtcNow)
        {
            _sessions.Remove(sessionId, out _);
            return null;
        }
        else if (_options.SlidingExpiration && session.Expires <= DateTime.UtcNow.AddMinutes(_options.ExpirationPeriodMinutes / 2.0))
        {
            session.Expires = DateTime.UtcNow.AddMinutes(_options.ExpirationPeriodMinutes);
        }

        return session;
    }

    public Session? TryAddSession(Guid sessionId, byte[] sessionKey)
    {
        var session = new Session() { SessionId = sessionId, Expires = DateTime.UtcNow.AddMinutes(_options.ExpirationPeriodMinutes), SessionKey = sessionKey };
        bool isSuccess = _sessions.TryAdd(sessionId, session);

        return isSuccess ? session : null;
    }
}
