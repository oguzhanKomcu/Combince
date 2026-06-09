using Microsoft.AspNetCore.Mvc;
using System.Net;
using Combince.Modules.PostComments.Core.Common; 

namespace Combince.Host.Api.Controllers;

[ApiController]
public abstract class BaseController : ControllerBase
{
    public IActionResult ProcessResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.StatusCode switch
        {
            HttpStatusCode.NotFound => NotFound(result),
            HttpStatusCode.BadRequest => BadRequest(result),
            HttpStatusCode.Unauthorized => Unauthorized(result),
            HttpStatusCode.Forbidden => Forbid(),
            _ => StatusCode((int)HttpStatusCode.InternalServerError, result)
        };
    }

}