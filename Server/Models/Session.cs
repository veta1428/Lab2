﻿using System.Security;

namespace Server.Models;

public class Session
{
    public Guid SessionId { get; init; }

    public DateTime Expires { get; init; }

    public string SessionKey { get; set; } = null!;
}