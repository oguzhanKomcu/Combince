using Combince.Modules.Social.Core.Abstractions;
using Combince.Modules.Social.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Social.Infrastructure.Persistence;

public class SocialDbContext : DbContext, ISocialDbContext
{
    public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options) { }

    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<SavedPost> SavedPosts => Set<SavedPost>();
    public DbSet<SocialUser> SocialUsers => Set<SocialUser>();

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

        modelBuilder.Entity<SavedPost>(builder =>
        {
            builder.ToTable("SavedPosts");
            builder.HasKey(s => s.Id);

            builder.HasIndex(s => new { s.UserId, s.PostId }).IsUnique();

            builder.Property(s => s.UserId).IsRequired();
            builder.Property(s => s.PostId).IsRequired();
            builder.Property(s => s.SavedAt).IsRequired();
        });

        modelBuilder.Entity<SocialUser>(builder =>
        {
            builder.ToTable("SocialUsers");
            builder.HasKey(u => u.UserId);
            builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
            builder.Property(u => u.ProfilePictureUrl).HasMaxLength(500);
        });

        base.OnModelCreating(modelBuilder);
    }
}