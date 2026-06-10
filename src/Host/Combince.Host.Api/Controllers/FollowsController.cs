using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Combince.Modules.Social.Core.Common;
using Combince.Modules.Social.Core.Features.Follows.Commands.FollowUser;
using Combince.Modules.Social.Core.Features.Follows.Commands.UnfollowUser;
using Combince.Modules.Social.Core.Features.Follows.Queries.GetFollowers;
using Combince.Modules.Social.Core.Features.Follows.Queries.GetFollowing;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Combince.Modules.Social.Infrastructure.Controllers;

/// <summary>
/// Kullanıcıların takipleşme ilişkilerini yöneten yönetim merkezidir.
/// </summary>
[ApiController]
[Route("api/social/follows")]
public class FollowsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FollowsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Belirtilen bir kullanıcıyı takip etme operasyonunu başlatır.
    /// </summary>
    /// <param name="command">Takip eden ve takip edilen kullanıcı kimlik bilgilerini içeren komut gövdesi.</param>
    /// <param name="cancellationToken">İşlem iptal belirteci.</param>
    /// <returns>İşlem başarılı ise oluşturulan takip ilişkisine ait benzersiz kimlik bilgisini döner.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> FollowUser([FromBody] FollowUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return ProcessResult(result);
    }

    /// <summary>
    /// Belirtilen bir kullanıcıyı takipten çıkarma operasyonunu başlatır.
    /// </summary>
    /// <param name="command">Takipten çıkacak ve çıkarılacak kullanıcı kimlik bilgilerini içeren komut gövdesi.</param>
    /// <param name="cancellationToken">İşlem iptal belirteci.</param>
    /// <returns>İşlem başarılı ise operasyonun başarı durumunu döner.</returns>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnfollowUser([FromBody] UnfollowUserCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return ProcessResult(result);
    }

    /// <summary>
    /// Belirtilen kullanıcının takipçilerini sayfalama ile listeler.
    /// </summary>
    /// <param name="userId">Takipçileri listelenecek kullanıcının benzersiz kimliği.</param>
    /// <param name="pageNumber">Sayfa numarası (Varsayılan: 1).</param>
    /// <param name="pageSize">Sayfa başına getirilecek kayıt sayısı (Varsayılan: 10).</param>
    /// <param name="cancellationToken">İşlem iptal belirteci.</param>
    /// <returns>Takipçi listesini döner.</returns>
    [HttpGet("{userId:guid}/followers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFollowers(
        [FromRoute] Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFollowersQuery(userId, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return ProcessResult(result);
    }

    /// <summary>
    /// Belirtilen kullanıcının takip ettiği kişileri sayfalama ile listeler.
    /// </summary>
    /// <param name="userId">Takip ettiği kişiler listelenecek kullanıcının benzersiz kimliği.</param>
    /// <param name="pageNumber">Sayfa numarası (Varsayılan: 1).</param>
    /// <param name="pageSize">Sayfa başına getirilecek kayıt sayısı (Varsayılan: 10).</param>
    /// <param name="cancellationToken">İşlem iptal belirteci.</param>
    /// <returns>Takip edilen kullanıcıların listesini döner.</returns>
    [HttpGet("{userId:guid}/following")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFollowing(
        [FromRoute] Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFollowingQuery(userId, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return ProcessResult(result);
    }

    private IActionResult ProcessResult<T>(Result<T> result)
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