namespace Server.Accessors;

public interface ISessionAccessor
{
    public Guid? CurrentSessionId { get; protected set; }

    void SetSessionId(Guid? sessionId);
}
