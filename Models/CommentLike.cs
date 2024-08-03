using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Blog.Models
{
	public class CommentLike
	{
		public required string UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public  ApplicationUser? ApplicationUser { get; set; } // Navigation property for the user

		public required long CommentId { get; set; } // ID of the comment that is liked

		[ForeignKey(nameof(CommentId))]
		public Comment? Comment { get; set; } // Navigation property for the comment
	}
}
