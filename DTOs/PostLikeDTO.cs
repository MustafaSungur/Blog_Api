namespace Blog.DTOs
{
	public class PostLikeCreateDTO
	{
		public long PostId { get; set; }
	}

	public class PostLikeReadDTO
	{
		public required string UserId { get; set; } 
		public required string UserName { get; set; } 
		public PostReadDTO? Post{ get; set; }
	}
}
