using System;

namespace Server.Accessors;

public class SessionAccessor : ISessionAccessor
{
    private Guid? _guid;

    public Guid? CurrentSessionId { get => _guid; set => _guid = value; }

    public void SetSessionId(Guid? sessionId)
    {
        CurrentSessionId = sessionId;
    }
}
