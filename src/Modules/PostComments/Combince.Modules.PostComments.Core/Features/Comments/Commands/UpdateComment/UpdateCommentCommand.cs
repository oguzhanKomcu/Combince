using System;
using System.Threading;
using System.Threading.Tasks;
using Combince.Modules.PostComments.Core.Abstractions;
using Combince.Modules.PostComments.Core.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.PostComments.Core.Features.Comments.Commands.UpdateComment;

public record UpdateCommentCommand(Guid CommentId, Guid UserId, string NewContent) : IRequest<Result<CommentUpdateSuccessResponse>>;

public record CommentUpdateSuccessResponse(string Message, bool Success);

public class UpdateCommentCommandHandler : IRequestHandler<UpdateCommentCommand, Result<CommentUpdateSuccessResponse>>
{
    private readonly IPostCommentsDbContext _context;
    private readonly ILocalizedMessageProvider _messageProvider;

    public UpdateCommentCommandHandler(
        IPostCommentsDbContext context,
        ILocalizedMessageProvider messageProvider)
    {
        _context = context;
        _messageProvider = messageProvider;
    }

    public async Task<Result<CommentUpdateSuccessResponse>> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId && c.UserId == request.UserId, cancellationToken);

        if (comment == null)
        {
            var notFoundMessage = _messageProvider.GetMessage("UserMessages", "PostCommentsMessages:CommentNotFound");
            return Result<CommentUpdateSuccessResponse>.Failure(notFoundMessage, System.Net.HttpStatusCode.NotFound);
        }

        var isUpdated = comment.UpdateContent(request.NewContent);

        if (!isUpdated)
        {
            var errorMessage = _messageProvider.GetMessage("ValidationMessages", "PostComments:CommentContentNotEmpty");
            return Result<CommentUpdateSuccessResponse>.Failure(errorMessage, System.Net.HttpStatusCode.BadRequest);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var successMessage = _messageProvider.GetMessage("UserMessages", "PostCommentsMessages:CommentUpdatedSuccessfully");

        return Result<CommentUpdateSuccessResponse>.Success(new CommentUpdateSuccessResponse(successMessage, true));
    }
}