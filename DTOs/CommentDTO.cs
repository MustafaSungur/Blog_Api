namespace Blog.DTOs
{
	public class CommentCreateDTO
	{
		public string Content { get; set; } = "";
		public long PostId { get; set; }
		public long? ParentCommentId { get; set; }
	}

	public class CommentReadDTO
	{
		public long Id { get; set; }
		public string? Content { get; set; }
		public string? UserId { get; set; }
		public string? UserName { get; set; } 
		public long? PostId { get; set; }
		public List<CommentReadDTO>? Replies { get; set; } 
		public DateTime? CreatedDate { get; set; }
		public long? ParentCommentId { get; internal set; }
		public List<CommentLikeReadDTO>? Likes { get; set; }
	}
}
