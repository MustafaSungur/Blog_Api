using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Models
{
	public class Post
	{
		public long Id { get; set; }

		[StringLength(50)]
		public string Title { get; set; } = "";

		[StringLength(300)]
		public string Content { get; set; } = "";

		public string? PhotoUrl { get; set; }

		public DateTime RegisterDate { get; set; }

		public required string UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public ApplicationUser? ApplicationUser { get; set; }

		public List<Comment>? Comments { get; set; }

		public List<PostLike>? Likes { get; set; }
	}
}
