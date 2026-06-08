using System.Threading;
using System.Threading.Tasks;
using Combince.Modules.PostComments.Core.Abstractions;
using Combince.Modules.PostComments.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.PostComments.Infrastructure.Persistence;

public class PostCommentsDbContext : DbContext, IPostCommentsDbContext
{
    public PostCommentsDbContext(DbContextOptions<PostCommentsDbContext> options) : base(options)
    {
    }

    public DbSet<Comment> Comments => Set<Comment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Comment>(builder =>
        {
            builder.ToTable("Comments", "comments");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(c => c.PostId)
                .IsRequired();

            builder.Property(c => c.UserId)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired();

            builder.Property(c => c.UpdatedAt);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}