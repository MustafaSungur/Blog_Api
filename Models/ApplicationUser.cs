using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Blog.Models
{
	public class ApplicationUser : IdentityUser
	{
		[StringLength(50)]
		public string FirstName { get; set; } = "";

		[StringLength(50)]
		public string LastName { get; set; } = "";

		public DateTime BirthDate { get; set; }

		public DateTime RegisterDate { get; set; }

		public bool Status { get; set; } = true;

		public string? PhotoUrl { get; set; }

		[NotMapped]
		public string? Password { get; set; }

		[NotMapped]
		[Compare(nameof(Password))]
		public string? ConfirmPassword { get; set; }

		public List<Post>? Posts { get; set; }

		public List<Comment>? Comments { get; set; }

		public List<PostLike>? PostLikes { get; set; }

		public List<CommentLike>? CommentLikes { get; set; }
	}
}
