namespace Blog.DTOs
{
	public class PostCreateDTO
	{
		public string Title { get; set; } = "";
		public string Content { get; set; } = "";
		public IFormFile? Photo { get; set; }
	}

	public class PostReadDTO
	{
		public long Id { get; set; }
		public string? Title { get; set; } = "";
		public string? Content { get; set; } = "";
		public string? PhotoUrl { get; set; }
		public DateTime? RegisterDate { get; set; }
		public string? UserId { get; set; } = "";
		public string? UserName { get; set; } = "";
		public List<CommentReadDTO>? Comments { get; set; }
		public List<PostLikeReadDTO>? Likes { get; set; }
	}
}
