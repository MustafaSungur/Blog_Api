using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
	public class CommentsController : ControllerBase
	{
		private readonly BlogContext _context;
		private readonly UserManager<ApplicationUser> _userManager;

		public CommentsController(BlogContext context, UserManager<ApplicationUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}

		[HttpGet]
		public async Task<IActionResult> GetComments()
		{
			var comments = await _context.Comments!
			   .Include(c => c.ApplicationUser)
			   .Include(c=>c.Likes)
			   .Where(c => c.ParentCommentId == null && c!.ApplicationUser!.Status! == true)
			   .ToListAsync();

			if (comments == null || comments.Count == 0)
			{
				return NotFound("No comments found.");
			}

			var commentResponses = new List<CommentReadDTO>();

			foreach (var comment in comments)
			{
				var commentResponse = await MapCommentDto(comment);
				commentResponses.Add(commentResponse);
			}

			return Ok(commentResponses);
		}

		private async Task<CommentReadDTO> MapCommentDto(Comment comment)
		{
			var commentResponse = new CommentReadDTO
			{
				Id = comment.Id,
				Content = comment.Content,
				CreatedDate = comment.CreatedDate,
				UserId = comment.UserId,
				PostId = comment.PostId,
				UserName = comment.ApplicationUser!.UserName,
				Likes = comment.Likes!
				.Select(cl => new CommentLikeReadDTO { UserName = cl.ApplicationUser!.UserName!, UserId = cl.ApplicationUser.Id }).ToList(),
				Replies = new List<CommentReadDTO>()
			};

			var replies = await _context.Comments!
				.Include(c => c.ApplicationUser)
				.Include(c => c.Likes)!
					.ThenInclude(l=>l.ApplicationUser)!
				.Where(c => c.ParentCommentId == comment.Id && c!.ApplicationUser!.Status! == true)
				.ToListAsync();

			foreach (var reply in replies)
			{
				var replyResponse = await MapCommentDto(reply);
				commentResponse.Replies.Add(replyResponse);
			}

			return commentResponse;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetComment(long id)
		{
			var comment = await _context.Comments
				.Include(c => c.ApplicationUser)!
				.Include(c => c.Likes)!
					.ThenInclude(l => l.ApplicationUser)
				.FirstOrDefaultAsync(c => c.Id == id);

			if (comment == null)
			{
				return NotFound();
			}

			var commentDto = await MapCommentDto(comment);

			return Ok(commentDto);
		}
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> PostComment(CommentCreateDTO commentDto)
		{
			var userName = User.FindFirstValue(ClaimTypes.Name);
			var user = await _userManager.FindByNameAsync(userName!) ?? throw new Exception("User not found");

			var comment = new Comment
			{
				Content = commentDto.Content,
				UserId = user!.Id,
				PostId = commentDto.PostId,
				ParentCommentId = commentDto.ParentCommentId,
				CreatedDate = DateTime.Now
			};

			_context.Comments.Add(comment);
			await _context.SaveChangesAsync();

			var newComment = await _context.Comments
				.Include(e => e.ApplicationUser)
				.FirstOrDefaultAsync(e => e.Id == comment.Id);

			if (newComment == null)
			{
				return NotFound("Comment not found after saving.");
			}

			var commentReadDto = new CommentReadDTO
			{
				Id = newComment.Id,
				Content = newComment.Content,
				UserId = newComment.UserId,
				UserName = newComment.ApplicationUser?.UserName ?? "Unknown",
				PostId = newComment.PostId,
				ParentCommentId = newComment.ParentCommentId,
				CreatedDate = newComment.CreatedDate,
				Likes = newComment.Likes?.Select(c => new CommentLikeReadDTO
				{
					UserName = c.ApplicationUser?.UserName ?? "Unknown",
					UserId = c.ApplicationUser!.Id
				}).ToList() ?? new List<CommentLikeReadDTO>()
			};

			return CreatedAtAction("GetComment", new { id = commentReadDto.Id }, commentReadDto);
		}


		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> DeleteComment(long id)
		{
			var currentUserID = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("You must login");
			var currentUser = _userManager.FindByIdAsync(currentUserID!).Result ?? throw new Exception("User not found");
			var role = _userManager.GetRolesAsync(currentUser!).Result;

			
			var comment = await _context.Comments
				.Include(c => c.Likes)!
				.Include(c => c.Replies)!
				.ThenInclude(r => r.Likes)
				.FirstOrDefaultAsync(c => c.Id == id);

			if (comment == null)
			{
				return NotFound();
			}

			if (currentUserID != comment.ApplicationUser?.Id && !role.Contains("Admin"))
			{
				return StatusCode(403);
			}


			// Remove likes of all replies first
			foreach (var reply in comment!.Replies!)
			{
				if (reply.Likes != null)
				{
					_context.CommentLikes.RemoveRange(reply.Likes);
				}
			}

			// Remove replies
			_context.Comments.RemoveRange(comment.Replies);

			// Remove likes of the comment
			if (comment.Likes != null)
			{
				_context.CommentLikes.RemoveRange(comment.Likes);
			}

			// Remove the comment
			_context.Comments.Remove(comment);

			await _context.SaveChangesAsync();

			return NoContent();
		}



		[HttpGet("user/{userId}")]
		public async Task<IActionResult> GetUserComments(string userId)
		{
			var currentUserID = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("You must login");
			var currentUser = _userManager.FindByIdAsync(currentUserID!).Result ?? throw new Exception("User not found");
			var role = _userManager.GetRolesAsync(currentUser!).Result;

			if (currentUserID != userId && !role.Contains("Admin"))
			{
				return StatusCode(403);
			}

			var userComments = await _context.Comments
				.Include(c => c.ApplicationUser)!
				.Include(c => c.Likes)!
					.ThenInclude(l=>l.ApplicationUser)
				.Where(c => c.UserId == userId)
				.ToListAsync();

			if (userComments == null || userComments.Count == 0)
			{
				return NotFound("No comments found for the specified user.");
			}

			var commentDTOs = new List<CommentReadDTO>();

			foreach (var comment in userComments)
			{
				var commentDto = await MapCommentDto(comment);
				commentDTOs.Add(commentDto);
			}

			return Ok(commentDTOs);
		}

	}
}
