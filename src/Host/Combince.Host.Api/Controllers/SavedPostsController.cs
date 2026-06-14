using Combince.Modules.Social.Core.Common;
using Combince.Modules.Social.Core.Features.SavedPosts.Commands.SavePost;
using Combince.Modules.Social.Core.Features.SavedPosts.Commands.UnsavePost;
using Combince.Modules.Social.Core.Features.SavedPosts.Queries.GetSavedPosts;
using Combince.Shared.Core.Extensions; // User.GetUserId() extension'ının yaşadığı namespace
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Combince.Host.Api.Controllers;

/// <summary>
/// Kullanıcıların kombin kaydetme, kaydı geri alma ve kaydedilen kombinleri listeleme etkileşimlerini yönetir.
/// </summary>
[Authorize]
[ApiController]
[Route("api/social/saved-posts")]
public class SavedPostsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SavedPostsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Belirtilen kombini giriş yapmış olan kullanıcının profiline kaydeder (Favorilere ekler).
    /// </summary>
    /// <param name="postId">Kaydedilecek olan kombinin (Post) benzersiz kimliği.</param>
    /// <param name="cancellationToken">İptal token'ı.</param>
    /// <returns>Kayıt başarılı ise oluşturulan SavedPost entity kimliğini döner.</returns>
    [HttpPost("{postId:guid}")]
    public async Task<IActionResult> Save([FromRoute] Guid postId, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var command = new SavePostCommand(userId, postId);
        var result = await _mediator.Send(command, cancellationToken);

        return ProcessResult(result);
    }

    /// <summary>
    /// Daha önce kaydedilmiş bir kombinin kaydını kullanıcının profilinden kaldırır (Favorilerden çıkarır).
    /// </summary>
    /// <param name="postId">Kaydı kaldırılacak olan kombinin (Post) benzersiz kimliği.</param>
    /// <param name="cancellationToken">İptal token'ı.</param>
    /// <returns>İşlem başarılı ise HTTP 200 döner.</returns>
    [HttpDelete("{postId:guid}")]
    public async Task<IActionResult> Unsave([FromRoute] Guid postId, CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var command = new UnsavePostCommand(userId, postId);
        var result = await _mediator.Send(command, cancellationToken);

        return ProcessResult(result);
    }

    /// <summary>
    /// Giriş yapmış olan kullanıcının daha önce kaydettiği tüm kombinleri sayfalayarak (Pagination) listeler.
    /// </summary>
    /// <param name="pageNumber">İstenen sayfa numarası (Varsayılan: 1).</param>
    /// <param name="pageSize">Sayfa başına getirilecek kombin sayısı (Varsayılan: 10).</param>
    /// <param name="cancellationToken">İptal token'ı.</param>
    /// <returns>Sayfalanmış kaydedilen kombin listesini döner.</returns>
    [HttpGet]
    public async Task<IActionResult> GetSaved(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = User.GetUserId();
        var query = new GetSavedPostsQuery(userId, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return ProcessResult(result);
    }

    /// <summary>
    /// Servis sonuçlarını (Result Pattern) uygun HTTP durum kodlarına haritalayan merkezi metot.
    /// </summary>
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