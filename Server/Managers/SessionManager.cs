using Server.Models;
using System.Collections.Concurrent;
using System.Security;

namespace Server.Managers;

public class SessionManager
{
    private ConcurrentDictionary<Guid, Session> _sessions = new();

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
        
        return session;         
    }

    public Session? TryAddSession(Guid sessionId, string sessionKey)
    {
        var session = new Session() { SessionId = sessionId, Expires = DateTime.UtcNow.AddDays(1), SessionKey = sessionKey };
        bool isSuccess = _sessions.TryAdd(sessionId, session);

        return isSuccess ? session : null;
    }
}
