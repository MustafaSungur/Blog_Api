namespace Blog.DTOs
{
	public class CommentLikeCreateDTO
	{
		public long CommentId { get; set; } // Only need the CommentId to create a like
	}

	public class CommentLikeReadDTO
	{
		public required string UserId { get; set; }  // The user who liked the comment
		public required string UserName { get; set; } // Optional, for including user name
		public CommentReadDTO? Comment{ get; set; }

	}
}
