using Combince.Modules.Posts.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Posts.Core.Abstractions;

public interface IPostsDbContext
{
    DbSet<Post> Posts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}