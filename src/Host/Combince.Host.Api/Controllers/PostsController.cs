using Combince.Modules.Posts.Core.Features.Posts.Commands;
using Combince.Modules.Posts.Core.Features.Posts.Commands.UpdatePost;
using Combince.Modules.Posts.Core.Features.Posts.Queries.GetPosts;
using Combince.Modules.Posts.Core.Features.Queries.GetPosts;
using Combince.Modules.Posts.Core.Common; // Posts modülünün kendi Result namespace'i
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Combince.Host.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Yeni bir kombin (post) paylaşımı oluşturur ve çoklu medya/veri içeriğini mühürler.
    /// </summary>
    /// <param name="command">Kombin içeriğini, başlığını ve multipart/form-data formatındaki görsel dosyalarını barındıran komut modeli.</param>
    /// <param name="cancellationToken">İşlem iptal edildiğinde (request aborted) asenkron akışı güvenli şekilde sonlandıracak iptal token'ı.</param>
    /// <returns>İşlem başarılı ise oluşturulan kombine ait benzersiz veri modelini (Value), başarısız ise hata detaylarını barındıran HTTP sonucunu döner.</returns>
    [HttpPost]
    public async Task<IActionResult> CreatePost([FromForm] CreatePostCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return ProcessResult(result);
    }

    /// <summary>
    /// Sistemdeki kombin akışını (Feed) ters kronolojik sırayla ve sayfalama (Pagination) mimarisine uygun olarak getirir.
    /// </summary>
    /// <param name="page">İstenen sayfa numarası (Değer belirtilmezse varsayılan: 1).</param>
    /// <param name="pageSize">Sayfa başına listelenecek maksimum kombin sayısı (Değer belirtilmezse varsayılan: 10).</param>
    /// <param name="cancellationToken">İşlem iptal edildiğinde (request aborted) veritabanı sorgusunu güvenli şekilde sonlandıracak iptal token'ı.</param>
    /// <returns>İşlem başarılı ise sayfalama parametrelerine uyan kombin listesini (Value), başarısız ise ilgili HTTP hata kodunu döner.</returns>
    [HttpGet]
    public async Task<IActionResult> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var query = new GetPostsQuery(page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return ProcessResult(result);
    }

    /// <summary>
    /// Paylaşılan bir kombinin açıklamasını günceller.
    /// </summary>
    /// <param name="postId">Güncellenecek postun benzersiz kimliği</param>
    /// <param name="request">Yeni açıklama modelini içeren gövde</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>İşlem başarılı ise güncellenen kombine ait güncel veriyi, başarısız ise hata detayını döner.</returns>
    [HttpPut("{postId:guid}")]
    public async Task<IActionResult> UpdatePost(
        [FromRoute] Guid postId,
        [FromBody] UpdatePostRequest request,
        CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Users:InvalidUserClaim");
        }

        var command = new UpdatePostCommand(postId, userId, request.Description);
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