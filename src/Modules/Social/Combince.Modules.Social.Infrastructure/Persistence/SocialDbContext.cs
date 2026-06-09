using Combince.Modules.Social.Core.Entities;
using Combince.Modules.Social.Core.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Social.Infrastructure.Persistence;

public class SocialDbContext : DbContext, ISocialDbContext
{
    public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options) { }

    public DbSet<Follow> Follows => Set<Follow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("social");

        modelBuilder.Entity<Follow>(builder =>
        {
            builder.ToTable("Follows");
            builder.HasKey(f => f.Id);
            builder.HasIndex(f => new { f.FollowerId, f.FollowingId }).IsUnique();
            builder.Property(f => f.FollowerId).IsRequired();
            builder.Property(f => f.FollowingId).IsRequired();
            builder.Property(f => f.CreatedAt).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}