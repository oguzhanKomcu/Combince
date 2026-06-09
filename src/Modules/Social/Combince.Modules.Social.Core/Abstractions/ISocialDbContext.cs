using Combince.Modules.Social.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Combince.Modules.Social.Core.Abstractions;

public interface ISocialDbContext
{
    DbSet<Follow> Follows { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}