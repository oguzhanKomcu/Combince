using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Combince.Modules.PostComments.Core.Abstractions;
using Combince.Modules.PostComments.Core.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.PostComments.Core.Features.Comments.Queries.GetPostComments;

public record CommentDto(Guid CommentId, Guid UserId, string Content, DateTime CreatedAt, DateTime? UpdatedAt);

public record GetPostCommentsQuery(Guid PostId, int PageNumber = 1, int PageSize = 10) : IRequest<Result<List<CommentDto>>>;

public class GetPostCommentsQueryValidator : AbstractValidator<GetPostCommentsQuery>
{
    public GetPostCommentsQueryValidator(ILocalizedMessageProvider messageProvider)
    {
        RuleFor(x => x.PostId)
            .NotEmpty()
            .WithMessage(_ => messageProvider.GetMessage("ValidationMessages", "PostComments:PostIdNotEmpty"));

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage(_ => messageProvider.GetMessage("ValidationMessages", "PostComments:PageNumberGreaterThanZero"));

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(50)
            .WithMessage(_ => messageProvider.GetMessage("ValidationMessages", "PostComments:PageSizeRangeInvalid"));
    }
}

public class GetPostCommentsQueryHandler : IRequestHandler<GetPostCommentsQuery, Result<List<CommentDto>>>
{
    private readonly IPostCommentsDbContext _context;

    public GetPostCommentsQueryHandler(IPostCommentsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<CommentDto>>> Handle(GetPostCommentsQuery request, CancellationToken cancellationToken)
    {
        var comments = await _context.Comments
            .Where(c => c.PostId == request.PostId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CommentDto(c.Id, c.UserId, c.Content, c.CreatedAt, c.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Result<List<CommentDto>>.Success(comments);
    }
}