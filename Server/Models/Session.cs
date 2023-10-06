﻿using System.Security;

namespace Server.Models;

public class Session
{
    public Guid SessionId { get; init; }

    public DateTime Expires { get; set; }

    public byte[] SessionKey { get; set; } = null!;
}
