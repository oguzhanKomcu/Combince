using Combince.Modules.Posts.Core.Features.Posts.Commands;
using Combince.Modules.Posts.Core.Features.Posts.Queries.GetPosts;
using Combince.Modules.Posts.Core.Features.Queries.GetPosts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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

        // 🎯 .Data yerine .Value olarak güncellendi
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

        // 🎯 .Data yerine .Value olarak güncellendi
        return Ok(result.Value);
    }
}