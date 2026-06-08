using System;
using System.Threading.Tasks;
using Combince.Modules.PostComments.Core.Features.Comments.Commands.CreateComment;
using Combince.Modules.PostComments.Core.Features.Comments.Commands.UpdateComment;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateComment(Guid postId, [FromBody] CreateCommentRequest request)
    {
        var command = new CreateCommentCommand(postId, request.UserId, request.Content);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result.Value);
    }

    [HttpPut("{commentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateComment(Guid postId, Guid commentId, [FromBody] UpdateCommentRequest request)
    {
        var command = new UpdateCommentCommand(commentId, request.UserId, request.NewContent);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound(result);
            }
            return BadRequest(result);
        }

        return Ok(result.Value);
    }
}

public record CreateCommentRequest(Guid UserId, string Content);
public record UpdateCommentRequest(Guid UserId, string NewContent);