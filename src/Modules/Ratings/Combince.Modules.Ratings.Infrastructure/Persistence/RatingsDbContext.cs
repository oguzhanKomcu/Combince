using Combince.Modules.Ratings.Core.Abstractions;
using Combince.Modules.Ratings.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Ratings.Infrastructure.Persistence;

public class RatingsDbContext : DbContext, IRatingsDbContext
{
    public RatingsDbContext(DbContextOptions<RatingsDbContext> options) : base(options) { }

    public DbSet<PostRating> PostRatings => Set<PostRating>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ratings");

        modelBuilder.Entity<PostRating>(builder =>
        {
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.PostId, x.UserId }).IsUnique();
        });

        base.OnModelCreating(modelBuilder);
    }
}