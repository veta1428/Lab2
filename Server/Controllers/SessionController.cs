﻿using Microsoft.AspNetCore.Mvc;
using Server.Accessors;
using Server.Managers;
using System.Security;
using System.Security.Cryptography;
using System.Text;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    private readonly ISessionAccessor _sessionAccessor;
    private readonly SessionManager _sessionManager;

    public SessionController(ISessionAccessor sessionAccessor, SessionManager sessionManager)
    {
        _sessionAccessor = sessionAccessor;
        _sessionManager = sessionManager;
    }

    [HttpPost]
    public string StartSession([FromBody] byte[] rsaPublicKey)
    {
        string sessionKey = "Super puper secret string!";
        RSA rsa = RSA.Create();
        rsa.ImportRSAPublicKey(rsaPublicKey, out _);
        byte[] sessionKeyEncrypted = rsa.Encrypt(Encoding.Unicode.GetBytes(sessionKey), RSAEncryptionPadding.Pkcs1);
        Guid sessionId = Guid.NewGuid();
        HttpContext.Response.Cookies.Append("SessionId", sessionId.ToString());

        _sessionManager.TryAddSession(sessionId, sessionKey);

        return Convert.ToBase64String(sessionKeyEncrypted);
    }
}
