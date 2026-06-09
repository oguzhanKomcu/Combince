using System;
using System.Threading;
using System.Threading.Tasks;
using Combince.Modules.PostComments.Core.Abstractions;
using Combince.Modules.PostComments.Core.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.PostComments.Core.Features.Comments.Commands.DeleteComment;

public record CommentDeleteSuccessResponse(string Message, bool Success);
public record DeleteCommentCommand(Guid CommentId, Guid UserId) : IRequest<Result<CommentDeleteSuccessResponse>>;
public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator(ILocalizedMessageProvider messageProvider)
    {
        RuleFor(x => x.CommentId)
            .NotEmpty()
            .WithMessage(_ => messageProvider.GetMessage("ValidationMessages", "PostComments:CommentIdNotEmpty"));

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage(_ => messageProvider.GetMessage("ValidationMessages", "PostComments:UserIdNotEmpty"));
    }
}

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Result<CommentDeleteSuccessResponse>>
{
    private readonly IPostCommentsDbContext _context;
    private readonly ILocalizedMessageProvider _messageProvider;

    public DeleteCommentCommandHandler(
        IPostCommentsDbContext context,
        ILocalizedMessageProvider messageProvider)
    {
        _context = context;
        _messageProvider = messageProvider;
    }

    public async Task<Result<CommentDeleteSuccessResponse>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _context.Comments
            .FirstOrDefaultAsync(c => c.Id == request.CommentId && c.UserId == request.UserId, cancellationToken);

        if (comment == null)
        {
            var notFoundMessage = _messageProvider.GetMessage("UserMessages", "PostCommentsMessages:CommentNotFound");
            return Result<CommentDeleteSuccessResponse>.Failure(notFoundMessage, System.Net.HttpStatusCode.NotFound);
        }

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync(cancellationToken);

        var successMessage = _messageProvider.GetMessage("UserMessages", "PostCommentsMessages:CommentDeletedSuccessfully");

        return Result<CommentDeleteSuccessResponse>.Success(new CommentDeleteSuccessResponse(successMessage, true));
    }
}