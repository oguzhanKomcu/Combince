using Combince.Modules.Users.Core.Features.Users.Commands.RefreshTokenUser;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Combince.Host.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Süresi dolan erişim token'ını (Access Token) geçerli bir Refresh Token ile yeniler.
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return StatusCode((int)result.StatusCode, new { StatusCode = (int)result.StatusCode, Message = result.Error });
        }

        return Ok(result.Value);
    }
}