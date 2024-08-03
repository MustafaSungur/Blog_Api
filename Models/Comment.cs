using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Blog.DTOs;

namespace Blog.Models
{
	public class Comment
	{
		public long Id { get; set; }

		[StringLength(500)]
		public string Content { get; set; } = "";

		public required string UserId { get; set; }

		[ForeignKey(nameof(UserId))]
		public ApplicationUser? ApplicationUser { get; set; }

		public long PostId { get; set; }

		[ForeignKey(nameof(PostId))]
		public Post? Post { get; set; }

		public long? ParentCommentId { get; set; }

		[ForeignKey(nameof(ParentCommentId))]
		public Comment? ParentComment { get; set; }

		public List<Comment>? Replies { get; set; }

		public List<CommentLike>? Likes { get; set; }

		public DateTime CreatedDate { get; set; }
	}
}
