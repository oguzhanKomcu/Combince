using Combince.Modules.Ratings.Core.Features.Ratings.Commands.RatePost;
using Combince.Modules.Ratings.Core.Common; // Ratings modülünün kendi Result namespace'i
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
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

    /// <summary>
    /// Bir kombine puan verir veya mevcut puanı günceller.
    /// </summary>
    /// <param name="request">Puanlanacak postun kimliği ve skor değerini barındıran istek modeli.</param>
    /// <param name="cancellationToken">İşlem iptal edildiğinde asenkron akışı sonlandıracak iptal token'ı.</param>
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
        return ProcessResult(result);
    }

    /// <summary>
    /// Geriye generic bir veri modeli (Value) dönen Result sonuçlarını işler.
    /// </summary>
    private IActionResult ProcessResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            if (result.Value is bool || result.Value == null)
            {
                return Ok();
            }
            return Ok(result.Value);
        }

        return result.StatusCode switch
        {
            HttpStatusCode.NotFound => NotFound(result.Error),
            HttpStatusCode.BadRequest => BadRequest(result.Error),
            HttpStatusCode.Unauthorized => Unauthorized(result.Error),
            HttpStatusCode.Forbidden => Forbid(),
            _ => StatusCode((int)result.StatusCode, result.Error)
        };
    }

}

public record RatePostRequest(Guid PostId, int Score);