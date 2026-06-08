using System.Threading;
using System.Threading.Tasks;
using Combince.Modules.PostComments.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.PostComments.Core.Abstractions;

public interface IPostCommentsDbContext
{
    DbSet<Comment> Comments { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}