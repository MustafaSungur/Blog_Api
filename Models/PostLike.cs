using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Models
{
	public class PostLike
	{
		[Key]
		public required string UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public ApplicationUser? ApplicationUser { get; set; }

		public required long PostId { get; set; }

		[ForeignKey(nameof(PostId))]
		public Post? Post { get; set; }
	}
}
