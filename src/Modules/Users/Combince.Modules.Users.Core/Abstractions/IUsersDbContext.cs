using Combince.Modules.Users.Core.Entities;
using Microsoft.EntityFrameworkCore; // Sadece DbSet taşımak için

namespace Combince.Modules.Users.Core.Abstractions;

public interface IUsersDbContext
{
    DbSet<User> Users { get; }
    DbSet<UserRefreshToken> UserRefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}