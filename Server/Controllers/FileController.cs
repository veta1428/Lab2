using Microsoft.AspNetCore.Mvc;
using Serpent;
using Server.Accessors;
using Server.Managers;
using System.Net;

namespace Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : ControllerBase
{
    private readonly ISessionAccessor _sessionAccessor;
    private readonly SessionManager _sessionManager;

    public FileController(
        ISessionAccessor sessionAccessor,
        SessionManager sessionManager)
    {
        _sessionAccessor = sessionAccessor;
        _sessionManager = sessionManager;
    }

    [HttpGet]
    [Route("{filename}")]
    public ActionResult<string> GetFileDecrypted([FromRoute] string fileName)
    {
        var currentSessionId = _sessionAccessor.CurrentSessionId;
        if (currentSessionId is null)
            return new UnauthorizedResult();

        var session = _sessionManager.TryGetSession(currentSessionId.Value);

        if (session is null)
            return new UnauthorizedResult();

        var sessionKey = session.SessionKey;

        var filePath = Environment.CurrentDirectory + "\\" + fileName;
        if (!System.IO.File.Exists(filePath))
            return new NotFoundResult();

        SerpentCipher sa = new SerpentCipher();

        var encryptionResult = sa.Encrypt(fileName, sessionKey, 32, EncryptionMode.ECB);

        if (!encryptionResult.IsSuccessful)
            return StatusCode((int)HttpStatusCode.InternalServerError); 

        var exportFile = Path.ChangeExtension(fileName, ".serpent");

        var fileEncrypted = System.IO.File.ReadAllBytes(exportFile);

        return Convert.ToBase64String(fileEncrypted);
    }
}
