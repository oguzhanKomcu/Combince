using Combince.Modules.Ratings.Core.Features.Ratings.Commands.RatePost;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Combince.Host.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/ratings")]
public class RatingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RatingsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RatePost([FromBody] RatePostRequest request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Users:InvalidUserClaim");
        }

        var command = new RatePostCommand(request.PostId, userId, request.Score);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode((int)result.StatusCode, result.Error);
        }

        return Ok(result.Value);
    }
}

public record RatePostRequest(Guid PostId, int Score);