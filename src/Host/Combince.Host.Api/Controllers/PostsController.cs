using Combince.Modules.Posts.Core.Features.Posts.Commands;
using Combince.Modules.Posts.Core.Features.Posts.Commands.UpdatePost;
using Combince.Modules.Posts.Core.Features.Posts.Queries.GetPosts;
using Combince.Modules.Posts.Core.Features.Queries.GetPosts;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Combince.Host.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostsController(IMediator _mediator)
    {
        this._mediator = _mediator;
    }

    /// <summary>
    /// Yeni bir kombin/post paylaşımı oluşturur.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreatePost([FromForm] CreatePostCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode((int)result.StatusCode, result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Kombin akışını (Feed) sayfalayarak jet hızında getirir.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var query = new GetPostsQuery(page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
        {
            return StatusCode((int)result.StatusCode, result.Error);
        }

        return Ok(result.Value);
    }


    /// <summary>
    /// Paylaşılan bir kombinin açıklamasını günceller.
    /// </summary>
    /// <param name="postId">Güncellenecek postun benzersiz kimliği</param>
    /// <param name="request">Yeni açıklama modelini içeren gövde</param>
    /// <param name="cancellationToken">İptal token'ı</param>
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

        if (!result.IsSuccess)
        {
            return StatusCode((int)result.StatusCode, result.Error);
        }

        return Ok(result.Value);
    }
}