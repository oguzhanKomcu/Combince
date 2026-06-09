using Combince.Modules.PostComments.Core.Features.Comments.Commands.CreateComment;
using Combince.Modules.PostComments.Core.Features.Comments.Commands.DeleteComment;
using Combince.Modules.PostComments.Core.Features.Comments.Commands.UpdateComment;
using Combince.Modules.PostComments.Core.Features.Comments.Queries.GetPostComments;
using Combince.Modules.PostComments.Core.Common; // Sadece bu modülün Result namespace'i
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Combince.Modules.PostComments.Presentation.Controllers;

[ApiController]
[Route("api/posts/{postId}/comments")]
public class PostCommentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PostCommentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateComment(Guid postId, [FromBody] CreateCommentRequest request)
    {
        var command = new CreateCommentCommand(postId, request.UserId, request.Content);
        var result = await _mediator.Send(command);
        return ProcessResult(result);
    }

    [HttpPut("{commentId}")]
    public async Task<IActionResult> UpdateComment(Guid postId, Guid commentId, [FromBody] UpdateCommentRequest request)
    {
        var command = new UpdateCommentCommand(commentId, request.UserId, request.NewContent);
        var result = await _mediator.Send(command);
        return ProcessResult(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetPostComments(Guid postId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var query = new GetPostCommentsQuery(postId, pageNumber, pageSize);
        var result = await _mediator.Send(query);
        return ProcessResult(result);
    }

    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(Guid postId, Guid commentId, [FromQuery] Guid userId)
    {
        var command = new DeleteCommentCommand(commentId, userId);
        var result = await _mediator.Send(command);
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

public record CreateCommentRequest(Guid UserId, string Content);
public record UpdateCommentRequest(Guid UserId, string NewContent);