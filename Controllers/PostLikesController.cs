using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blog.Models;
using Blog.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;


namespace Blog.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PostLikesController : ControllerBase
	{
		private readonly BlogContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public PostLikesController(BlogContext context,UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		// GET: api/PostLikes
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<PostLikeReadDTO>>> GetLikes()
		{
			var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			var likes = await _context.PostLikes
				.Include(l=>l.Post)
				.Include(l => l.ApplicationUser)
				.ToListAsync();

			var likeDTOs = likes.Select(item => new PostLikeReadDTO
			{
				Post = new PostReadDTO
				{
					Id = item.Post!.Id,
					Content = item.Post.Content,
					PhotoUrl = item.Post.PhotoUrl,
				},
				UserId = item.UserId,
				UserName = item.ApplicationUser!.FirstName + " " + item.ApplicationUser.LastName,
			}).ToList();

			return likeDTOs;
		}

		// GET: api/PostLikes/{postId}
		[HttpGet("{postId}")]
		[Authorize]
		public async Task<ActionResult<PostLikeReadDTO>> GetLike(long postId)
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
			var like = await _context.PostLikes
				.Include(l => l.ApplicationUser)
				.Include(l => l.Post)
				.FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);

			if (like == null)
			{
				return NotFound();
			}

			return new PostLikeReadDTO
			{
				Post = new PostReadDTO
				{
					Id = like.Post!.Id,
					Title = like.Post.Title,
					Content = like.Post.Content,
					PhotoUrl = like.Post.PhotoUrl,
				},
				UserId = like.UserId,
				UserName = like.ApplicationUser!.FirstName + " " + like.ApplicationUser.LastName,
			};
		}

		// POST: api/PostLikes
		[HttpPost]
		[Authorize]
		public async Task<ActionResult<PostLikeReadDTO>> PostLike(PostLikeCreateDTO likeDto)
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("You must login");

			var user = await _userManager.FindByIdAsync(userId);

			var existingLike = await _context.PostLikes
				.FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == likeDto.PostId);

			if (existingLike != null)
			{
				return BadRequest("Like already exists for this post by the user.");
			}

			var like = new PostLike
			{
				UserId = userId,
				PostId = likeDto.PostId
			};

			_context.PostLikes.Add(like);
			await _context.SaveChangesAsync();

			var newPostLike = await _context.PostLikes
				.Include(pl=>pl.Post)
				.FirstOrDefaultAsync(pl => pl.UserId==userId && pl.PostId==likeDto.PostId);

			return base.CreatedAtAction(nameof(GetLike), new { postId = like.PostId }, new PostLikeReadDTO
			{
				Post=new PostReadDTO
				{
					Id= newPostLike!.Post!.Id,
					Title = newPostLike.Post.Title,
					Content = newPostLike.Post.Content,
					PhotoUrl = newPostLike.Post.PhotoUrl,					
				},
				UserId = userId,
				UserName = user!.UserName!
			});
		}

		[HttpDelete]
		public async Task<IActionResult> DeletePostLike(long postId)
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User not found");

			var postLike = await _context.PostLikes.FirstOrDefaultAsync(p => p.PostId == postId && p.UserId==userId);
			if (postLike == null)
			{
				return NotFound();
			}

			 _context.Remove(postLike);
			await _context.SaveChangesAsync();
		
			return NoContent();
		}


		// GET: api/CommentLikes/user/{userId}
		[HttpGet("user/{userId}")]
		public async Task<ActionResult<IEnumerable<PostLikeReadDTO>>> GetUserPostLikes(string userId)
		{
			var likes = await _context.PostLikes
				.Where(l => l.UserId == userId)
				.Include(l => l.ApplicationUser)
				.Include(l => l.Post)
				.ToListAsync();

			if (likes.Count == 0)
			{
				return NotFound("No likes found for the specified user.");
			}

			var likeDTOs = likes.Select(item => new PostLikeReadDTO
			{
				UserId = item.UserId,
				Post = new PostReadDTO
				{
					Id = item.Post!.Id,
					Content = item.Post.Content,
					RegisterDate = item.Post.RegisterDate,
				
				},
				UserName = item.ApplicationUser!.UserName!
			}).ToList();

			return likeDTOs;
		}

	}
}
