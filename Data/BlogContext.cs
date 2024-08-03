using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Blog.Models;

namespace Blog.Models
{
	public class BlogContext : IdentityDbContext<ApplicationUser>
	{
		public DbSet<Post> Posts { get; set; }
		public DbSet<Comment> Comments { get; set; }
		public DbSet<PostLike> PostLikes { get; set; }
		public DbSet<CommentLike> CommentLikes { get; set; }

		public BlogContext(DbContextOptions<BlogContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// Ensure cascade deletes for Posts and their related entities
			modelBuilder.Entity<Post>()
				.HasMany(p => p.Comments)
				.WithOne(c => c.Post)
				.HasForeignKey(c => c.PostId)
				.OnDelete(DeleteBehavior.Cascade); // Cascading delete for comments

			modelBuilder.Entity<Post>()
				.HasMany(p => p.Likes)
				.WithOne(pl => pl.Post)
				.HasForeignKey(pl => pl.PostId)
				.OnDelete(DeleteBehavior.Cascade); // Cascading delete for likes

			// Ensure cascade deletes for Comments and their related entities
			modelBuilder.Entity<Comment>()
				.HasMany(c => c.Likes)
				.WithOne(cl => cl.Comment)
				.HasForeignKey(cl => cl.CommentId)
				.OnDelete(DeleteBehavior.Cascade);  // Ensure this is set to Cascade

			modelBuilder.Entity<Comment>()
				.HasMany(c => c.Replies)
				.WithOne(c => c.ParentComment)
				.HasForeignKey(c => c.ParentCommentId)
				.OnDelete(DeleteBehavior.Cascade); // Cascading delete for replies

			// For users, typically restrict or set null to avoid unintended mass deletion
			modelBuilder.Entity<Comment>()
				.HasOne(c => c.ApplicationUser)
				.WithMany(u => u.Comments)
				.HasForeignKey(c => c.UserId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.SetNull); // Set null when user is deleted

			modelBuilder.Entity<PostLike>()
				.HasOne(l => l.ApplicationUser)
				.WithMany(u => u.PostLikes)
				.HasForeignKey(l => l.UserId)
				.IsRequired(false)
				.OnDelete(DeleteBehavior.SetNull); // Set null when user is deleted

			

			// Define composite key for CommentLikes as non-clustered
			modelBuilder.Entity<CommentLike>()
				.HasKey(cl => new { cl.UserId, cl.CommentId })
				.IsClustered(false);
		}
	}
}
