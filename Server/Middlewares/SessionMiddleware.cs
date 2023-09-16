using Server.Accessors;
using Server.Managers;

namespace Server.Middlewares;

public class SessionMiddleware
{
    private const string SessionId = "SessionId";

    private readonly RequestDelegate _next;

    public SessionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, SessionManager sessionManager, ISessionAccessor sessionAccessor)
    {
        string? sessionId = null;
        context.Request.Cookies.TryGetValue(SessionId, out sessionId);

        if (sessionId is not null)
        {
            Guid sessionIdGuid;
            var isSuccess = Guid.TryParse(sessionId, out sessionIdGuid);

            if (!isSuccess)
                throw new Exception("Something went wrong");

            var trackingSession = sessionManager.TryGetSession(sessionIdGuid);

            if (trackingSession is null)
            {
                sessionAccessor.SetSessionId(null);
            }
            else
            {
                context.Response.Cookies.Append(SessionId, trackingSession.SessionId.ToString());
                sessionAccessor.SetSessionId(trackingSession.SessionId);
            }
        }

        await _next(context);
    }
}
