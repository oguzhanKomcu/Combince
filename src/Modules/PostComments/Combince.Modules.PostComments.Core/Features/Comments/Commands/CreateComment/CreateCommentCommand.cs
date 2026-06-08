using System;
using System.Threading;
using System.Threading.Tasks;
using Combince.Modules.PostComments.Core.Abstractions;
using Combince.Modules.PostComments.Core.Common;
using Combince.Modules.PostComments.Core.Entities;
using MediatR;

namespace Combince.Modules.PostComments.Core.Features.Comments.Commands.CreateComment;

public record CreateCommentCommand(Guid PostId, Guid UserId, string Content) : IRequest<Result<CommentSuccessResponse>>;

public record CommentSuccessResponse(Guid CommentId, string Message, bool Success);

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Result<CommentSuccessResponse>>
{
    private readonly IPostCommentsDbContext _context;
    private readonly ILocalizedMessageProvider _messageProvider;

    public CreateCommentCommandHandler(
        IPostCommentsDbContext context,
        ILocalizedMessageProvider messageProvider)
    {
        _context = context;
        _messageProvider = messageProvider;
    }

    public async Task<Result<CommentSuccessResponse>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = new Comment(request.PostId, request.UserId, request.Content);

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync(cancellationToken);

        var successMessage = _messageProvider.GetMessage("UserMessages", "PostCommentsMessages:CommentSavedSuccessfully");

        return Result<CommentSuccessResponse>.Success(new CommentSuccessResponse(comment.Id, successMessage, true));
    }
}