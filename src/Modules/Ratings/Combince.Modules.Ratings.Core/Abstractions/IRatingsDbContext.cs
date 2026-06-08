using System.Threading;
using System.Threading.Tasks;
using Combince.Modules.Ratings.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Ratings.Core.Abstractions;

public interface IRatingsDbContext
{
    DbSet<PostRating> PostRatings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}