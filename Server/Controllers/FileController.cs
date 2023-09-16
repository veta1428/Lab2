using Microsoft.AspNetCore.Mvc;
using Server.Accessors;
using System.Runtime.Intrinsics.Arm;

namespace Server.Controllers;

public class FileController : ControllerBase
{
    private readonly ISessionAccessor _sessionAccessor;

    public FileController(ISessionAccessor sessionAccessor)
    {
        _sessionAccessor = sessionAccessor;
    }

    [HttpGet]
    [Route("{filename}")]
    public string GetFile([FromRoute] string fileName)
    {
        var file = System.IO.File.ReadAllTextAsync(fileName, CancellationToken.None);

        file
    }
}
