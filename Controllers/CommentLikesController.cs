using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Blog.Models;
using Blog.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Blog.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CommentLikesController : ControllerBase
	{
		private readonly BlogContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public CommentLikesController(BlogContext context,UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}


		// POST: api/CommentLikes
		[HttpPost]
		[Authorize]
		public async Task<ActionResult> PostLike(CommentLikeCreateDTO likeDto)
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

			var existingLike = await _context.CommentLikes
				.FirstOrDefaultAsync(l => l.UserId == userId && l.CommentId == likeDto.CommentId);

			if (existingLike != null)
			{
				return BadRequest("Like already exists for this comment by the user.");
			}

			var like = new CommentLike
			{
				UserId = userId,
				CommentId = likeDto.CommentId
			};

			_context.CommentLikes.Add(like);
			await _context.SaveChangesAsync();

			return Ok("Like added successfully.");
		}


		// DELETE: api/CommentLikes/{commentId}
		[HttpDelete("{commentId}")]
		[Authorize]
		public async Task<IActionResult> DeleteLike(long commentId)
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("User not found");
			var postLike = await _context.CommentLikes
				.Include(pl=>pl.ApplicationUser)
				.FirstOrDefaultAsync(c => c.CommentId ==commentId && c.UserId == userId);


			if (postLike == null)
			{
				return NotFound();
			}

			if (userId != postLike!.ApplicationUser!.Id!) 
			{
				return StatusCode(403);
			}


			_context.Remove(postLike);
			await _context.SaveChangesAsync();

			return NoContent();
			
		}

		// GET: api/CommentLikes/{userId}/{commentId}
		[HttpGet("{commentId}")]
		[Authorize]
		public async Task<ActionResult<CommentLikeReadDTO>> GetLike(long commentId)
		{
			string userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
			var like = await _context.CommentLikes
				.Include(l => l.ApplicationUser)
				.Include(l=> l.Comment)
				.FirstOrDefaultAsync(l => l.UserId == userId && l.CommentId == commentId);

			if (like == null)
			{
				return NotFound();
			}

			return new CommentLikeReadDTO
			{
				UserId = like.UserId,
				Comment = new CommentReadDTO
				{
					Id = like.Comment!.Id,
					Content = like.Comment.Content,
					CreatedDate = like.Comment.CreatedDate
					
				},
				UserName = like?.ApplicationUser?.UserName!
			};
		}

		// GET: api/CommentLikes/user/{userId}
		[HttpGet("user/{userId}")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<CommentLikeReadDTO>>> GetUserCommentLikes(string userId)
		{
			var currentUserID = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("You must login");
			var currentUser = _userManager.FindByIdAsync(currentUserID!).Result ?? throw new Exception("User not found");
			var role = _userManager.GetRolesAsync(currentUser!).Result;

			if (currentUserID != userId && !role.Contains("Admin"))
			{
				return StatusCode(403);
			}

			var likes = await _context.CommentLikes
				.Where(l => l.UserId == userId)
				.Include(l => l.ApplicationUser)
				.Include(l => l.Comment)
				.ToListAsync();

			if (likes.Count == 0)
			{
				return NotFound("No likes found for the specified user.");
			}

			var likeDTOs = likes.Select(item => new CommentLikeReadDTO
			{
				UserId = item.UserId,
				Comment = new CommentReadDTO
				{
					Id = item.Comment!.Id,
					Content = item.Comment.Content,
					CreatedDate= item.Comment.CreatedDate,
				},
				UserName = item.ApplicationUser!.UserName!
			}).ToList();

			return likeDTOs;
		}

	}
}
