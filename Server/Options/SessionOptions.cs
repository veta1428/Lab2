namespace Server.Options;

public class SessionOptions
{
    public bool SlidingExpiration { get; set; } = false;

    public int ExpirationPeriodMinutes { get; set; } = 1;
}
