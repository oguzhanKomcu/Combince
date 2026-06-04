using Combince.Modules.Posts.Core.Abstractions;
using Combince.Modules.Posts.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Posts.Infrastructure.Persistence;

public class PostsDbContext : DbContext, IPostsDbContext
{
    public PostsDbContext(DbContextOptions<PostsDbContext> options) : base(options) { }

    public DbSet<Post> Posts => Set<Post>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("posts");

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Tags)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, System.Text.Json.JsonSerializerOptions.Default),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, System.Text.Json.JsonSerializerOptions.Default) ?? new List<string>());
        });

        base.OnModelCreating(modelBuilder);
    }
}